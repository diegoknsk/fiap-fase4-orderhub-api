using FastFood.OrderHub.Application.Models.Common;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace FastFood.OrderHub.Api.Controllers;

/// <summary>
/// Controller de teste para validação inicial da API
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HelloController : ControllerBase
{
    /// <summary>
    /// Endpoint de teste que retorna uma mensagem "olá mundo"
    /// </summary>
    /// <returns>Mensagem de saudação em JSON</returns>
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(ApiResponse<object>.Ok(new { message = "olá mundo" }));
    }

    /// <summary>
    /// Endpoint de teste de conexão com PostgreSQL RDS
    /// </summary>
    /// <returns>Resultado do teste de conexão</returns>
    [HttpGet("test-postgres")]
    public async Task<IActionResult> TestPostgres()
    {
        var postgresConnectionString = "Host=fastfoodinfradbauth-auth-db.chcwyc6wsd8c.us-east-1.rds.amazonaws.com;Port=5432;Database=dbAuth;Username=dbadmin;Password=admin123";
        
        try
        {
            await using var connection = new NpgsqlConnection(postgresConnectionString);
            await connection.OpenAsync();
            
            // Primeiro, listar todas as tabelas para ver o que existe
            var tablesList = new List<string>();
            await using (var cmdTables = new NpgsqlCommand(@"
                SELECT table_name 
                FROM information_schema.tables 
                WHERE table_schema = 'public' 
                ORDER BY table_name", connection))
            {
                await using var readerTables = await cmdTables.ExecuteReaderAsync();
                while (await readerTables.ReadAsync())
                {
                    tablesList.Add(readerTables.GetString(0));
                }
            }
            
            // Tentar encontrar a tabela Customers (pode ser Customers, customers, ou "Customers")
            string? tableName = null;
            if (tablesList.Contains("Customers"))
                tableName = "Customers";
            else if (tablesList.Contains("customers"))
                tableName = "customers";
            else if (tablesList.Any(t => t.Equals("Customers", StringComparison.OrdinalIgnoreCase)))
                tableName = tablesList.First(t => t.Equals("Customers", StringComparison.OrdinalIgnoreCase));
            
            if (tableName == null)
            {
                return Ok(ApiResponse<object>.Ok(new
                {
                    success = true,
                    connectionStatus = "CONEXÃO COM RDS ESTABELECIDA COM SUCESSO! ✅",
                    message = "Tabela Customers não encontrada. A migration ainda não foi executada.",
                    availableTables = tablesList,
                    totalTables = tablesList.Count
                }));
            }
            
            // Se encontrou a tabela, executar o SELECT
            await using var command = new NpgsqlCommand($"SELECT * FROM \"{tableName}\"", connection);
            await using var reader = await command.ExecuteReaderAsync();
            
            var customers = new List<Dictionary<string, object>>();
            int rowCount = 0;
            
            while (await reader.ReadAsync())
            {
                rowCount++;
                var customer = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    customer[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                }
                customers.Add(customer);
            }
            
            return Ok(ApiResponse<object>.Ok(new
            {
                success = true,
                connectionStatus = "CONEXÃO COM RDS ESTABELECIDA COM SUCESSO! ✅",
                message = "Query executada com sucesso!",
                tableName = tableName,
                totalRecords = rowCount,
                customers = customers,
                availableTables = tablesList
            }));
        }
        catch (Exception ex)
        {
            return Ok(ApiResponse<object>.Ok(new
            {
                success = false,
                connectionStatus = "FALHA NA CONEXÃO COM RDS! ❌",
                message = "Falha ao conectar/executar query no PostgreSQL RDS",
                error = ex.Message,
                errorType = ex.GetType().Name,
                innerException = ex.InnerException?.Message
            }));
        }
    }
}


