using System.Data;
using Npgsql;

namespace JMBeauty.Datos;

public sealed class Conexion : IDisposable, IAsyncDisposable
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly string _connectionString;

    public const string VariablePassword = "JMBEAUTY_DB_PASSWORD";
    public const string DefaultHost = "localhost";
    public const int DefaultPort = 5432;
    public const string DefaultDatabase = "jmbeauty_db";
    public const string DefaultUsername = "postgres";

    public Conexion(string password, string host = DefaultHost, int port = DefaultPort, string database = DefaultDatabase, string username = DefaultUsername)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password, nameof(password));

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = host,
            Port = port,
            Database = database,
            Username = username,
            Password = password,
            Timeout = 5,
            CommandTimeout = 30,
            Pooling = true,
            MinPoolSize = 1,
            MaxPoolSize = 20
        };

        _connectionString = builder.ConnectionString;
        _dataSource = NpgsqlDataSource.Create(_connectionString);
    }

    public static string ObtenerPasswordDeEntorno()
    {
        var userVar = Environment.GetEnvironmentVariable(VariablePassword, EnvironmentVariableTarget.User);
        if (!string.IsNullOrWhiteSpace(userVar) && userVar != "123")
            return userVar;

        var papeleria = Environment.GetEnvironmentVariable("PAPELERIA_DB_PASSWORD", EnvironmentVariableTarget.User);
        if (!string.IsNullOrWhiteSpace(papeleria))
            return papeleria;

        var procVar = Environment.GetEnvironmentVariable(VariablePassword, EnvironmentVariableTarget.Process);
        if (!string.IsNullOrWhiteSpace(procVar) && procVar != "123")
            return procVar;

        return "postgres";
    }

    public static void GuardarPassword(string password)
    {
        Environment.SetEnvironmentVariable(VariablePassword, password, EnvironmentVariableTarget.User);
        Environment.SetEnvironmentVariable(VariablePassword, password, EnvironmentVariableTarget.Process);
    }

    public async Task<bool> ProbarConexionAsync(CancellationToken cancellationToken = default)
    {
        await using var cmd = _dataSource.CreateCommand("SELECT current_database();");
        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return result is string dbName && !string.IsNullOrWhiteSpace(dbName);
    }

    public NpgsqlCommand CrearComando(string sql)
    {
        return _dataSource.CreateCommand(sql);
    }

    public async Task<NpgsqlConnection> AbrirConexionAsync(CancellationToken cancellationToken = default)
    {
        return await _dataSource.OpenConnectionAsync(cancellationToken);
    }

    public void Dispose()
    {
        _dataSource.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _dataSource.DisposeAsync();
    }
}

