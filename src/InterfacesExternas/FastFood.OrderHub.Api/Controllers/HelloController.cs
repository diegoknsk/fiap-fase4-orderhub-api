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
            
            await using var command = new NpgsqlCommand("SELECT * FROM Customers", connection);
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
                message = "Conexão com RDS estabelecida com sucesso!",
                totalRecords = rowCount,
                customers = customers
            }));
        }
        catch (Exception ex)
        {
            return Ok(ApiResponse<object>.Ok(new
            {
                success = false,
                message = "Falha ao conectar/executar query no PostgreSQL RDS",
                error = ex.Message,
                errorType = ex.GetType().Name,
                innerException = ex.InnerException?.Message
            }));
        }
    }
}


