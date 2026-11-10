using JMBeauty.Datos;

namespace JMBeauty.Negocio;

public sealed class ConexionNegocio
{
    public static void ValidarClave(string? clave)
    {
        if (string.IsNullOrWhiteSpace(clave))
        {
            throw new ArgumentException("La clave de conexión a la base de datos no puede estar vacía. Verifique la variable de entorno JMBEAUTY_DB_PASSWORD.");
        }
    }

    public static async Task<bool> ProbarAsync(string clave, CancellationToken cancellationToken = default)
    {
        ValidarClave(clave);
        using var conexion = new Conexion(clave);
        return await conexion.ProbarConexionAsync(cancellationToken);
    }
}

