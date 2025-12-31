using FastFood.OrderHub.Application.InputModels.OrderManagement;
using FastFood.OrderHub.Application.Models.Common;
using FastFood.OrderHub.Application.Responses.OrderManagement;
using FastFood.OrderHub.Application.UseCases.OrderManagement;
using Microsoft.AspNetCore.Authorization;
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
    private readonly StartOrderUseCase _startOrderUseCase;
    private readonly AddProductToOrderUseCase _addProductToOrderUseCase;
    private readonly UpdateProductInOrderUseCase _updateProductInOrderUseCase;
    private readonly RemoveProductFromOrderUseCase _removeProductFromOrderUseCase;
    private readonly ConfirmOrderSelectionUseCase _confirmOrderSelectionUseCase;
    private readonly GetPagedOrdersUseCase _getPagedOrdersUseCase;

    public OrderController(
        GetOrderByIdUseCase getOrderByIdUseCase,
        StartOrderUseCase startOrderUseCase,
        AddProductToOrderUseCase addProductToOrderUseCase,
        UpdateProductInOrderUseCase updateProductInOrderUseCase,
        RemoveProductFromOrderUseCase removeProductFromOrderUseCase,
        ConfirmOrderSelectionUseCase confirmOrderSelectionUseCase,
        GetPagedOrdersUseCase getPagedOrdersUseCase)
    {
        _getOrderByIdUseCase = getOrderByIdUseCase;
        _startOrderUseCase = startOrderUseCase;
        _addProductToOrderUseCase = addProductToOrderUseCase;
        _updateProductInOrderUseCase = updateProductInOrderUseCase;
        _removeProductFromOrderUseCase = removeProductFromOrderUseCase;
        _confirmOrderSelectionUseCase = confirmOrderSelectionUseCase;
        _getPagedOrdersUseCase = getPagedOrdersUseCase;
    }

    /// <summary>
    /// Listar pedidos paginados
    /// </summary>
    [HttpGet]
    [Authorize(AuthenticationSchemes = "Cognito", Policy = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<GetPagedOrdersResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] int? status = null)
    {
        var input = new GetPagedOrdersInputModel
        {
            Page = page,
            PageSize = pageSize,
            Status = status
        };

        var response = await _getPagedOrdersUseCase.ExecuteAsync(input);
        return Ok(ApiResponse<GetPagedOrdersResponse>.Ok(response));
    }

    /// <summary>
    /// Obter pedido por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(AuthenticationSchemes = "Cognito", Policy = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<GetOrderByIdResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<GetOrderByIdResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var input = new GetOrderByIdInputModel { OrderId = id };
        var response = await _getOrderByIdUseCase.ExecuteAsync(input);

        if (response == null)
            return NotFound(ApiResponse<GetOrderByIdResponse>.Fail("Pedido não encontrado."));

        return Ok(ApiResponse<GetOrderByIdResponse>.Ok(response));
    }

    /// <summary>
    /// Iniciar novo pedido
    /// </summary>
    /// 
    [Authorize(AuthenticationSchemes = "CustomerBearer", Policy = "Customer")]
    [HttpPost("start")]
    [ProducesResponseType(typeof(ApiResponse<StartOrderResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<StartOrderResponse>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Start([FromBody] StartOrderInputModel input)
    {
        try
        {
            var response = await _startOrderUseCase.ExecuteAsync(input);
            return CreatedAtAction(nameof(GetById), new { id = response.OrderId }, ApiResponse<StartOrderResponse>.Ok(response, "Pedido iniciado com sucesso."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<StartOrderResponse>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<StartOrderResponse>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Adicionar produto ao pedido
    /// </summary>
    [Authorize(AuthenticationSchemes = "CustomerBearer", Policy = "Customer")]
    [HttpPost("add-product")]
    [ProducesResponseType(typeof(ApiResponse<AddProductToOrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AddProductToOrderResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<AddProductToOrderResponse>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddProduct([FromBody] AddProductToOrderInputModel input)
    {
        try
        {
            var response = await _addProductToOrderUseCase.ExecuteAsync(input);

            if (response == null)
                return NotFound(ApiResponse<AddProductToOrderResponse>.Fail("Pedido ou produto não encontrado."));

            return Ok(ApiResponse<AddProductToOrderResponse>.Ok(response, "Produto adicionado ao pedido com sucesso."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<AddProductToOrderResponse>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<AddProductToOrderResponse>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Atualizar produto no pedido
    /// </summary>
    [Authorize(AuthenticationSchemes = "CustomerBearer", Policy = "Customer")]
    [HttpPut("update-product")]
    [ProducesResponseType(typeof(ApiResponse<UpdateProductInOrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UpdateProductInOrderResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<UpdateProductInOrderResponse>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProduct([FromBody] UpdateProductInOrderInputModel input)
    {
        try
        {
            var response = await _updateProductInOrderUseCase.ExecuteAsync(input);

            if (response == null)
                return NotFound(ApiResponse<UpdateProductInOrderResponse>.Fail("Pedido ou produto não encontrado."));

            return Ok(ApiResponse<UpdateProductInOrderResponse>.Ok(response, "Produto atualizado no pedido com sucesso."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<UpdateProductInOrderResponse>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<UpdateProductInOrderResponse>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Remover produto do pedido
    /// </summary>
    [Authorize(AuthenticationSchemes = "CustomerBearer", Policy = "Customer")]
    [HttpDelete("remove-product")]
    [ProducesResponseType(typeof(ApiResponse<RemoveProductFromOrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RemoveProductFromOrderResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveProduct([FromBody] RemoveProductFromOrderInputModel input)
    {
        var response = await _removeProductFromOrderUseCase.ExecuteAsync(input);

        if (response == null)
            return NotFound(ApiResponse<RemoveProductFromOrderResponse>.Fail("Pedido ou produto não encontrado."));

        return Ok(ApiResponse<RemoveProductFromOrderResponse>.Ok(response, "Produto removido do pedido com sucesso."));
    }

    /// <summary>
    /// Confirmar seleção do pedido
    /// </summary>
    [Authorize(AuthenticationSchemes = "CustomerBearer", Policy = "Customer")]
    [HttpPost("{id:guid}/confirm-selection")]
    [ProducesResponseType(typeof(ApiResponse<ConfirmOrderSelectionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ConfirmOrderSelectionResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ConfirmOrderSelectionResponse>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmSelection(Guid id)
    {
        try
        {
            var input = new ConfirmOrderSelectionInputModel { OrderId = id };
            var response = await _confirmOrderSelectionUseCase.ExecuteAsync(input);

            if (response == null)
                return NotFound(ApiResponse<ConfirmOrderSelectionResponse>.Fail("Pedido não encontrado."));

            return Ok(ApiResponse<ConfirmOrderSelectionResponse>.Ok(response, "Seleção do pedido confirmada com sucesso."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<ConfirmOrderSelectionResponse>.Fail(ex.Message));
        }
    }
}
