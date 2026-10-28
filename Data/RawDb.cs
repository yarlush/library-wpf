using System.Data;
using Microsoft.Data.SqlClient;

namespace LibraryWpf.Data;

public static class RawDb
{
    public const string ConnectionString =
        @"Server=(localdb)\MSSQLLocalDB;Database=LibraryDB;Integrated Security=True;TrustServerCertificate=True;Encrypt=False;";

    public static DataTable Query(string sql, params SqlParameter[] parameters)
    {
        using var connection = new SqlConnection(ConnectionString);
        using var command = new SqlCommand(sql, connection);
        if (parameters.Length > 0)
            command.Parameters.AddRange(parameters);

        using var adapter = new SqlDataAdapter(command);
        var table = new DataTable();
        adapter.Fill(table);
        return table;
    }

    public static int Execute(string sql, params SqlParameter[] parameters)
    {
        using var connection = new SqlConnection(ConnectionString);
        using var command = new SqlCommand(sql, connection);
        if (parameters.Length > 0)
            command.Parameters.AddRange(parameters);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    public static Dictionary<string, object?> ExecuteProcedure(string procedureName, params SqlParameter[] parameters)
    {
        using var connection = new SqlConnection(ConnectionString);
        using var command = new SqlCommand(procedureName, connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        if (parameters.Length > 0)
            command.Parameters.AddRange(parameters);

        connection.Open();
        command.ExecuteNonQuery();

        return parameters
            .Where(p => p.Direction is ParameterDirection.Output or ParameterDirection.InputOutput)
            .ToDictionary(p => p.ParameterName, p => p.Value is DBNull ? null : p.Value);
    }

    public static SqlParameter P(string name, object? value)
        => new(name, value ?? DBNull.Value);

    public static SqlParameter Output(string name, SqlDbType type)
        => new(name, type) { Direction = ParameterDirection.Output };
}
