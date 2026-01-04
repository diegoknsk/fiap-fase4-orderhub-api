using FastFood.OrderHub.Application.Models.Common;
using Microsoft.AspNetCore.Mvc;

namespace FastFood.OrderHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(ApiResponse<object>.Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow.ToString("O")
        }));
    }
}

