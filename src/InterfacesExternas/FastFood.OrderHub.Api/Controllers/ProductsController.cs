using FastFood.OrderHub.Application.InputModels.ProductManagement;
using FastFood.OrderHub.Application.Responses.ProductManagement;
using FastFood.OrderHub.Application.UseCases.ProductManagement;
using Microsoft.AspNetCore.Mvc;

namespace FastFood.OrderHub.Api.Controllers;

/// <summary>
/// Controller para gerenciamento de produtos
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly GetProductByIdUseCase _getProductByIdUseCase;

    public ProductsController(GetProductByIdUseCase getProductByIdUseCase)
    {
        _getProductByIdUseCase = getProductByIdUseCase;
    }

    /// <summary>
    /// Obter produto por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(GetProductByIdResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var input = new GetProductByIdInputModel { ProductId = id };
        var response = await _getProductByIdUseCase.ExecuteAsync(input);

        if (response == null)
            return NotFound();

        return Ok(response);
    }
}

