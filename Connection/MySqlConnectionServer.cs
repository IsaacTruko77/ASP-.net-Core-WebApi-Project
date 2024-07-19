using System;
using System.Data;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;

public class MySqlConnectionHandler
{
    private readonly string _connectionString;
    private MySqlConnection _connection;

    public MySqlConnectionHandler(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("MySqlConnectionString");
    }

    private bool Open()
    {
        _connection = new MySqlConnection(_connectionString);
        try
        {
            _connection.Open();
            Console.WriteLine("Connection established.");
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("An error occurred while connecting to the database:");
            Console.WriteLine(e.Message);
            return false;
        }
    }

    public DataTable ExecuteQuery(MySqlCommand command)
    {
        DataTable table = new DataTable();
        using (_connection = new MySqlConnection(_connectionString))
        {
            if (Open())
            {
                command.Connection = _connection;
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                try
                {
                    adapter.Fill(table);
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occurred while executing the query:");
                    Console.WriteLine(e.Message);
                }
                _connection.Close();
                _connection.Dispose();
            }
        }
        return table;
    }

    public bool ExecuteNonQuery(MySqlCommand command)
    {
        bool success = false;
        using (_connection = new MySqlConnection(_connectionString))
        {
            if (Open())
            {
                command.Connection = _connection;
                try
                {
                    command.ExecuteNonQuery();
                    success = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occurred while executing the query:");
                    Console.WriteLine(e.Message);
                    success = false;
                }
                _connection.Close();
                _connection.Dispose();
            }
        }
        return success;
    }

    public bool ExecuteTransaction(Func<MySqlConnection, MySqlTransaction, bool> transactionalOperations)
    {
        if (Open())
        {
            using (var transaction = _connection.BeginTransaction())
            {
                try
                {
                    bool success = transactionalOperations(_connection, transaction);
                    if (success)
                    {
                        transaction.Commit();
                    }
                    else
                    {
                        transaction.Rollback();
                    }
                    return success;
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occurred during the transaction:");
                    Console.WriteLine(e.Message);
                    transaction.Rollback();
                    return false;
                }
                finally
                {
                    _connection.Close();
                }
            }
        }
        return false;
    }

    public async Task<bool> ExecuteTransactionAsync(Func<MySqlConnection, MySqlTransaction, Task<bool>> transactionalOperations)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        using var transaction = await connection.BeginTransactionAsync();
        try
        {
            Console.WriteLine("Transacción Async iniciada.");
            bool success = await transactionalOperations(connection, transaction);
            if (success)
            {
                await transaction.CommitAsync();
            }
            else
            {
                await transaction.RollbackAsync();
            }
            return success;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error durante la transacción: {ex.Message}");
            await transaction.RollbackAsync();
            throw;
        }
    }
    
    public async Task<int> ExecuteNonQueryAsync(MySqlCommand command)
    {
        using (var connection = new MySqlConnection(_connectionString))
        {
            Console.WriteLine("NoNQuery Async ejecutada.");
            await connection.OpenAsync();
            command.Connection = connection;
            return await command.ExecuteNonQueryAsync();
        }
    }

    public async Task<object> ExecuteScalarAsync(MySqlCommand command)
{
    using (var connection = new MySqlConnection(_connectionString))
    {
        await connection.OpenAsync();
        command.Connection = connection;
        return await command.ExecuteScalarAsync();
    }
}

}
