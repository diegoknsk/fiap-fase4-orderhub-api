using FastFood.OrderHub.Application.InputModels.ProductManagement;
using FastFood.OrderHub.Application.Models.Common;
using FastFood.OrderHub.Application.Responses.ProductManagement;
using FastFood.OrderHub.Application.UseCases.ProductManagement;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize(AuthenticationSchemes = "Cognito", Policy = "Admin")]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<GetProductsPagedResponse>), StatusCodes.Status200OK)]
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
        return Ok(ApiResponse<GetProductsPagedResponse>.Ok(response));
    }

    /// <summary>
    /// Obter produto por ID
    /// </summary>
    [Authorize(AuthenticationSchemes = "Cognito", Policy = "Admin")]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<GetProductByIdResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<GetProductByIdResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var input = new GetProductByIdInputModel { ProductId = id };
        var response = await _getProductByIdUseCase.ExecuteAsync(input);

        if (response == null)
            return NotFound(ApiResponse<GetProductByIdResponse>.Fail("Produto não encontrado."));

        return Ok(ApiResponse<GetProductByIdResponse>.Ok(response));
    }

    /// <summary>
    /// Criar produto
    /// </summary>
    [Authorize(AuthenticationSchemes = "Cognito", Policy = "Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateProductResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<CreateProductResponse>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateProductInputModel input)
    {
        try
        {
            var response = await _createProductUseCase.ExecuteAsync(input);
            return CreatedAtAction(nameof(GetById), new { id = response.ProductId }, ApiResponse<CreateProductResponse>.Ok(response, "Produto criado com sucesso."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<CreateProductResponse>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Atualizar produto
    /// </summary>
    [Authorize(AuthenticationSchemes = "Cognito", Policy = "Admin")]
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<UpdateProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UpdateProductResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<UpdateProductResponse>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductInputModel input)
    {
        try
        {
            input.ProductId = id;
            var response = await _updateProductUseCase.ExecuteAsync(input);

            if (response == null)
                return NotFound(ApiResponse<UpdateProductResponse>.Fail("Produto não encontrado."));

            return Ok(ApiResponse<UpdateProductResponse>.Ok(response, "Produto atualizado com sucesso."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<UpdateProductResponse>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Remover produto
    /// </summary>
    [Authorize(AuthenticationSchemes = "Cognito", Policy = "Admin")]
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<DeleteProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DeleteProductResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var input = new DeleteProductInputModel { ProductId = id };
        var response = await _deleteProductUseCase.ExecuteAsync(input);

        if (response == null)
            return NotFound(ApiResponse<DeleteProductResponse>.Fail("Produto não encontrado."));

        return Ok(ApiResponse<DeleteProductResponse>.Ok(response, "Produto removido com sucesso."));
    }
}

