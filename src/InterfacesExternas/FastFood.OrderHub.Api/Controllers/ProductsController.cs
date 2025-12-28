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
    private readonly GetProductsPagedUseCase _getProductsPagedUseCase;
    private readonly CreateProductUseCase _createProductUseCase;
    private readonly UpdateProductUseCase _updateProductUseCase;
    private readonly DeleteProductUseCase _deleteProductUseCase;

    public ProductsController(
        GetProductByIdUseCase getProductByIdUseCase,
        GetProductsPagedUseCase getProductsPagedUseCase,
        CreateProductUseCase createProductUseCase,
        UpdateProductUseCase updateProductUseCase,
        DeleteProductUseCase deleteProductUseCase)
    {
        _getProductByIdUseCase = getProductByIdUseCase;
        _getProductsPagedUseCase = getProductsPagedUseCase;
        _createProductUseCase = createProductUseCase;
        _updateProductUseCase = updateProductUseCase;
        _deleteProductUseCase = deleteProductUseCase;
    }

    /// <summary>
    /// Listar produtos paginados
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(GetProductsPagedResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] int? category = null, [FromQuery] string? name = null)
    {
        var input = new GetProductsPagedInputModel
        {
            Page = page,
            PageSize = pageSize,
            Category = category,
            Name = name
        };

        var response = await _getProductsPagedUseCase.ExecuteAsync(input);
        return Ok(response);
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

    /// <summary>
    /// Criar produto
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateProductInputModel input)
    {
        try
        {
            var response = await _createProductUseCase.ExecuteAsync(input);
            return CreatedAtAction(nameof(GetById), new { id = response.ProductId }, response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Atualizar produto
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(UpdateProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductInputModel input)
    {
        try
        {
            input.ProductId = id;
            var response = await _updateProductUseCase.ExecuteAsync(input);

            if (response == null)
                return NotFound();

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Remover produto
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(DeleteProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var input = new DeleteProductInputModel { ProductId = id };
        var response = await _deleteProductUseCase.ExecuteAsync(input);

        if (response == null)
            return NotFound();

        return Ok(response);
    }
}

