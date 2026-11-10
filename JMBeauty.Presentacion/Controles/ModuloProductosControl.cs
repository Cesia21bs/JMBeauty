using System.Drawing;
using System.Windows.Forms;
using JMBeauty.Datos;
using JMBeauty.Entidades;
using JMBeauty.Negocio;

namespace JMBeauty.Presentacion.Controles;

public sealed class ModuloProductosControl : UserControl
{
    private readonly string _clave;
    private List<Producto> _todosLosProductos = [];
    private List<CategoriaProducto> _categorias = [];

    private DataGridView _grid = null!;
    private TextBox _txtBuscar = null!;
    private Button _btnNuevo = null!;
    private Button _btnEditar = null!;
    private Button _btnActualizar = null!;
    private Label _lblConteo = null!;

    public ModuloProductosControl(string clave)
    {
        _clave = clave;
        InicializarComponentes();
        _ = CargarDatosAsync();
    }

    private void InicializarComponentes()
    {
        Dock = DockStyle.Fill;
        BackColor = TemaJMBeauty.CremaClaro;
        Font = TemaJMBeauty.FuenteCuerpo;

        // Barra de Herramientas
        var pnlBarra = new Panel
        {
            Dock = DockStyle.Top,
            Height = 65,
            BackColor = Color.White,
            Padding = new Padding(15, 12, 15, 12)
        };

        var lblBuscar = new Label { Text = "🔍 Buscar:", Location = new Point(15, 20), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _txtBuscar = new TextBox { Location = new Point(85, 17), Width = 280, Font = TemaJMBeauty.FuenteCuerpo };
        _txtBuscar.TextChanged += (s, e) => FiltrarProductos();

        _btnNuevo = TemaJMBeauty.CrearBotonAccion("➕ Nuevo Producto", TemaJMBeauty.Dorado, Color.White, 140);
        _btnNuevo.Location = new Point(380, 14);
        _btnNuevo.Click += (s, e) => AbrirEdicion(null);

        _btnEditar = TemaJMBeauty.CrearBotonAccion("✏️ Editar", TemaJMBeauty.RosaPastel, Color.White, 100);
        _btnEditar.Location = new Point(530, 14);
        _btnEditar.Click += (s, e) => EditarSeleccionado();

        _btnActualizar = TemaJMBeauty.CrearBotonAccion("🔄 Actualizar", Color.DimGray, Color.White, 110);
        _btnActualizar.Location = new Point(640, 14);
        _btnActualizar.Click += async (s, e) => await CargarDatosAsync();

        _lblConteo = new Label
        {
            Text = "Cargando catálogo...",
            AutoSize = true,
            Location = new Point(770, 21),
            ForeColor = Color.Gray,
            Font = TemaJMBeauty.FuenteCuerpoBold
        };

        pnlBarra.Controls.AddRange([lblBuscar, _txtBuscar, _btnNuevo, _btnEditar, _btnActualizar, _lblConteo]);

        // Configuración de la Tabla / Grid
        _grid = new DataGridView { Dock = DockStyle.Fill };
        TemaJMBeauty.AplicarEstiloGrid(_grid);

        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Codigo", HeaderText = "CÓDIGO", Width = 110 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Nombre", HeaderText = "PRODUCTO / COSMÉTICO", Width = 260 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CategoriaNombre", HeaderText = "CATEGORÍA", Width = 170 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "PrecioCosto",
            HeaderText = "COSTO (Lps.)",
            Width = 110,
            DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "PrecioVenta",
            HeaderText = "PRECIO VENTA (Lps.)",
            Width = 140,
            DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "ExistenciaMinima",
            HeaderText = "MÍNIMO",
            Width = 90,
            DefaultCellStyle = { Format = "N0", Alignment = DataGridViewContentAlignment.MiddleCenter }
        });

        // Columna Calculada de Uso
        var colTipo = new DataGridViewTextBoxColumn { HeaderText = "TIPO USO", Width = 130 };
        _grid.Columns.Add(colTipo);

        // Columna Estado
        var colEstado = new DataGridViewTextBoxColumn { HeaderText = "ESTADO", Width = 100 };
        _grid.Columns.Add(colEstado);

        _grid.CellFormatting += (s, e) =>
        {
            if (e.RowIndex < 0 || e.RowIndex >= _grid.Rows.Count) return;
            if (_grid.Rows[e.RowIndex].DataBoundItem is Producto p)
            {
                if (e.ColumnIndex == 6) // Tipo
                {
                    e.Value = p.EsInsumoInterno ? "Técnico / Insumo" : "Venta Público";
                    e.FormattingApplied = true;
                }
                else if (e.ColumnIndex == 7) // Estado
                {
                    e.Value = p.Activo ? "Activo" : "Inactivo";
                    e.FormattingApplied = true;
                }
            }
        };

        _grid.DoubleClick += (s, e) => EditarSeleccionado();

        var pnlContenedor = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(15)
        };
        pnlContenedor.Controls.Add(_grid);

        Controls.Add(pnlContenedor);
        Controls.Add(pnlBarra);
    }

    private async Task CargarDatosAsync()
    {
        Cursor = Cursors.WaitCursor;
        _lblConteo.Text = "Consultando base de datos...";

        try
        {
            _categorias = await ProductoNegocio.ListarCategoriasAsync(_clave);
            _todosLosProductos = await ProductoNegocio.ListarAsync(_clave);
            FiltrarProductos();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar productos:\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private void FiltrarProductos()
    {
        var filtro = _txtBuscar.Text.Trim().ToLowerInvariant();
        var lista = string.IsNullOrWhiteSpace(filtro)
            ? _todosLosProductos
            : _todosLosProductos.Where(p =>
                p.Codigo.ToLowerInvariant().Contains(filtro) ||
                p.Nombre.ToLowerInvariant().Contains(filtro) ||
                p.CategoriaNombre.ToLowerInvariant().Contains(filtro)).ToList();

        _grid.DataSource = null;
        _grid.DataSource = lista;
        _lblConteo.Text = $"Total: {lista.Count} producto(s)";
    }

    private void EditarSeleccionado()
    {
        if (_grid.CurrentRow?.DataBoundItem is Producto seleccionado)
        {
            AbrirEdicion(seleccionado);
        }
    }

    private void AbrirEdicion(Producto? producto)
    {
        using var frm = new FormProductoEdicion(_clave, _categorias, producto);
        if (frm.ShowDialog(this) == DialogResult.OK)
        {
            _ = CargarDatosAsync();
        }
    }
}

