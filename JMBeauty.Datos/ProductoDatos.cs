using System.Data;
using JMBeauty.Entidades;
using Npgsql;

namespace JMBeauty.Datos;

public sealed class ProductoDatos
{
    private readonly Conexion _conexion;

    public ProductoDatos(Conexion conexion)
    {
        _conexion = conexion ?? throw new ArgumentNullException(nameof(conexion));
    }

    public async Task<List<CategoriaProducto>> ListarCategoriasAsync(CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT categoria_producto_id, nombre, activo FROM public.categoria_producto ORDER BY nombre;";
        var lista = new List<CategoriaProducto>();
        await using var cmd = _conexion.CrearComando(sql);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            lista.Add(new CategoriaProducto
            {
                CategoriaProductoId = reader.GetInt32(0),
                Nombre = reader.GetString(1),
                Activo = reader.GetBoolean(2)
            });
        }
        return lista;
    }

    public async Task<List<Producto>> ListarAsync(bool soloActivos = false, CancellationToken cancellationToken = default)
    {
        var sql = """
            SELECT p.producto_id, p.categoria_producto_id, cp.nombre AS categoria_nombre,
                   p.codigo, p.nombre, p.precio_costo, p.precio_venta, p.existencia_minima,
                   p.es_insumo_interno, p.activo, p.fecha_creacion
            FROM public.producto p
            JOIN public.categoria_producto cp ON p.categoria_producto_id = cp.categoria_producto_id
            """ + (soloActivos ? " WHERE p.activo = TRUE" : "") + " ORDER BY cp.nombre, p.nombre;";

        var lista = new List<Producto>();
        await using var cmd = _conexion.CrearComando(sql);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            lista.Add(new Producto
            {
                ProductoId = reader.GetInt32(0),
                CategoriaProductoId = reader.GetInt32(1),
                CategoriaNombre = reader.GetString(2),
                Codigo = reader.GetString(3),
                Nombre = reader.GetString(4),
                PrecioCosto = reader.GetDecimal(5),
                PrecioVenta = reader.GetDecimal(6),
                ExistenciaMinima = reader.GetDecimal(7),
                EsInsumoInterno = reader.GetBoolean(8),
                Activo = reader.GetBoolean(9),
                FechaCreacion = reader.GetDateTime(10)
            });
        }
        return lista;
    }

    public async Task<List<InventarioItem>> ListarInventarioActualAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT producto_id, codigo, producto, categoria, precio_costo,
                   precio_venta, existencia_minima, es_insumo_interno,
                   existencia_actual, alerta_bajo_minimo
            FROM public.vw_inventario_actual
            ORDER BY alerta_bajo_minimo DESC, producto;
            """;

        var lista = new List<InventarioItem>();
        await using var cmd = _conexion.CrearComando(sql);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            lista.Add(new InventarioItem
            {
                ProductoId = reader.GetInt32(0),
                Codigo = reader.GetString(1),
                Producto = reader.GetString(2),
                Categoria = reader.GetString(3),
                PrecioCosto = reader.GetDecimal(4),
                PrecioVenta = reader.GetDecimal(5),
                ExistenciaMinima = reader.GetDecimal(6),
                EsInsumoInterno = reader.GetBoolean(7),
                ExistenciaActual = reader.GetDecimal(8),
                AlertaBajoMinimo = reader.GetBoolean(9)
            });
        }
        return lista;
    }

    public async Task<int> GuardarAsync(Producto p, Producto? original, CancellationToken cancellationToken = default)
    {
        if (original is null)
        {
            const string sqlInsert = """
                INSERT INTO public.producto (
                    categoria_producto_id, codigo, nombre, precio_costo,
                    precio_venta, existencia_minima, es_insumo_interno, activo
                )
                VALUES (
                    @cat, @cod, @nom, @costo,
                    @venta, @min, @ins, @act
                )
                RETURNING producto_id;
                """;

            await using var cmd = _conexion.CrearComando(sqlInsert);
            cmd.Parameters.AddWithValue("@cat", p.CategoriaProductoId);
            cmd.Parameters.AddWithValue("@cod", p.Codigo);
            cmd.Parameters.AddWithValue("@nom", p.Nombre);
            cmd.Parameters.AddWithValue("@costo", p.PrecioCosto);
            cmd.Parameters.AddWithValue("@venta", p.PrecioVenta);
            cmd.Parameters.AddWithValue("@min", p.ExistenciaMinima);
            cmd.Parameters.AddWithValue("@ins", p.EsInsumoInterno);
            cmd.Parameters.AddWithValue("@act", p.Activo);

            try
            {
                var id = await cmd.ExecuteScalarAsync(cancellationToken);
                return Convert.ToInt32(id);
            }
            catch (PostgresException ex) when (ex.SqlState == "23505")
            {
                throw new InvalidOperationException("Ya existe un producto con el código especificado.", ex);
            }
        }
        else
        {
            const string sqlUpdate = """
                UPDATE public.producto
                SET categoria_producto_id = @cat,
                    codigo = @cod,
                    nombre = @nom,
                    precio_costo = @costo,
                    precio_venta = @venta,
                    existencia_minima = @min,
                    es_insumo_interno = @ins,
                    activo = @act
                WHERE producto_id = @id
                  AND codigo = @ocod
                  AND nombre = @onom
                  AND precio_costo = @ocosto
                  AND precio_venta = @oventa
                  AND existencia_minima = @omin
                  AND es_insumo_interno = @oins
                  AND activo = @oact
                RETURNING producto_id;
                """;

            await using var cmd = _conexion.CrearComando(sqlUpdate);
            cmd.Parameters.AddWithValue("@id", p.ProductoId);
            cmd.Parameters.AddWithValue("@cat", p.CategoriaProductoId);
            cmd.Parameters.AddWithValue("@cod", p.Codigo);
            cmd.Parameters.AddWithValue("@nom", p.Nombre);
            cmd.Parameters.AddWithValue("@costo", p.PrecioCosto);
            cmd.Parameters.AddWithValue("@venta", p.PrecioVenta);
            cmd.Parameters.AddWithValue("@min", p.ExistenciaMinima);
            cmd.Parameters.AddWithValue("@ins", p.EsInsumoInterno);
            cmd.Parameters.AddWithValue("@act", p.Activo);

            // Concurrencia
            cmd.Parameters.AddWithValue("@ocod", original.Codigo);
            cmd.Parameters.AddWithValue("@onom", original.Nombre);
            cmd.Parameters.AddWithValue("@ocosto", original.PrecioCosto);
            cmd.Parameters.AddWithValue("@oventa", original.PrecioVenta);
            cmd.Parameters.AddWithValue("@omin", original.ExistenciaMinima);
            cmd.Parameters.AddWithValue("@oins", original.EsInsumoInterno);
            cmd.Parameters.AddWithValue("@oact", original.Activo);

            try
            {
                var result = await cmd.ExecuteScalarAsync(cancellationToken);
                if (result is null)
                {
                    throw new InvalidOperationException("El producto fue modificado por otro usuario en la red. Recargue los datos.");
                }
                return p.ProductoId;
            }
            catch (PostgresException ex) when (ex.SqlState == "23505")
            {
                throw new InvalidOperationException("Ya existe un producto con el código especificado.", ex);
            }
        }
    }
}

