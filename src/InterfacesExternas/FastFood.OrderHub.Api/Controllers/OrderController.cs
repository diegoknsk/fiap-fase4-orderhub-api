using FastFood.OrderHub.Application.InputModels.OrderManagement;
using FastFood.OrderHub.Application.Responses.OrderManagement;
using FastFood.OrderHub.Application.UseCases.OrderManagement;
using Microsoft.AspNetCore.Mvc;

namespace FastFood.OrderHub.Api.Controllers;

/// <summary>
/// Controller para gerenciamento de pedidos
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly GetOrderByIdUseCase _getOrderByIdUseCase;

    public OrderController(GetOrderByIdUseCase getOrderByIdUseCase)
    {
        _getOrderByIdUseCase = getOrderByIdUseCase;
    }

    /// <summary>
    /// Obter pedido por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(GetOrderByIdResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var input = new GetOrderByIdInputModel { OrderId = id };
        var response = await _getOrderByIdUseCase.ExecuteAsync(input);

        if (response == null)
            return NotFound();

        return Ok(response);
    }
}

