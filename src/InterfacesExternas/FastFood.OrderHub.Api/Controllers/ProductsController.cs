using FastFood.OrderHub.Application.Exceptions;
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
public class ProductsController(
    GetProductByIdUseCase getProductByIdUseCase,
    GetProductsPagedUseCase getProductsPagedUseCase,
    CreateProductUseCase createProductUseCase,
    UpdateProductUseCase updateProductUseCase,
    DeleteProductUseCase deleteProductUseCase) : ControllerBase
{

    /// <summary>
    /// Listar produtos paginados
    /// </summary>
    [Authorize(AuthenticationSchemes = "Cognito,CustomerBearer")]
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

        var response = await getProductsPagedUseCase.ExecuteAsync(input);
        return Ok(ApiResponse<GetProductsPagedResponse>.Ok(response));
    }

    /// <summary>
    /// Obter produto por ID
    /// </summary>
    [Authorize(AuthenticationSchemes = "Cognito,CustomerBearer")]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<GetProductByIdResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<GetProductByIdResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var input = new GetProductByIdInputModel { ProductId = id };
            var response = await getProductByIdUseCase.ExecuteAsync(input);
            return Ok(ApiResponse<GetProductByIdResponse>.Ok(response));
        }
        catch (BusinessException ex)
        {
            return NotFound(ApiResponse<GetProductByIdResponse>.Fail(ex.Message));
        }
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
            var response = await createProductUseCase.ExecuteAsync(input);
            return CreatedAtAction(nameof(GetById), new { id = response.ProductId }, ApiResponse<CreateProductResponse>.Ok(response, "Produto criado com sucesso."));
        }
        catch (BusinessException ex)
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
            var response = await updateProductUseCase.ExecuteAsync(input);
            return Ok(ApiResponse<UpdateProductResponse>.Ok(response, "Produto atualizado com sucesso."));
        }
        catch (BusinessException ex)
        {
            if (ex.Message.Contains("não encontrado"))
                return NotFound(ApiResponse<UpdateProductResponse>.Fail(ex.Message));
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
        try
        {
            var input = new DeleteProductInputModel { ProductId = id };
            var response = await deleteProductUseCase.ExecuteAsync(input);
            return Ok(ApiResponse<DeleteProductResponse>.Ok(response, "Produto removido com sucesso."));
        }
        catch (BusinessException ex)
        {
            return NotFound(ApiResponse<DeleteProductResponse>.Fail(ex.Message));
        }
    }
}

