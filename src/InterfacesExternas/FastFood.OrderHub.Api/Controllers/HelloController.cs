using Microsoft.AspNetCore.Mvc;

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
        return Ok(new { message = "olá mundo" });
    }
}

