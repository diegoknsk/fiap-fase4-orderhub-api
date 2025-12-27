using Microsoft.AspNetCore.Mvc;

namespace FastFood.OrderHub.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow.ToString("O")
        });
    }
}

