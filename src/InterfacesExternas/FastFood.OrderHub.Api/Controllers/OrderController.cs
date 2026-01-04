using FastFood.OrderHub.Application.Exceptions;
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
public class OrderController(
    GetOrderByIdUseCase getOrderByIdUseCase,
    StartOrderUseCase startOrderUseCase,
    AddProductToOrderUseCase addProductToOrderUseCase,
    UpdateProductInOrderUseCase updateProductInOrderUseCase,
    RemoveProductFromOrderUseCase removeProductFromOrderUseCase,
    ConfirmOrderSelectionUseCase confirmOrderSelectionUseCase,
    GetPagedOrdersUseCase getPagedOrdersUseCase) : ControllerBase
{

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

        var response = await getPagedOrdersUseCase.ExecuteAsync(input);
        return Ok(ApiResponse<GetPagedOrdersResponse>.Ok(response));
    }

    /// <summary>
    /// Obter pedido por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(AuthenticationSchemes = "Cognito,CustomerBearer")]
    [ProducesResponseType(typeof(ApiResponse<GetOrderByIdResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<GetOrderByIdResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var input = new GetOrderByIdInputModel { OrderId = id };
            var response = await getOrderByIdUseCase.ExecuteAsync(input);
            return Ok(ApiResponse<GetOrderByIdResponse>.Ok(response));
        }
        catch (BusinessException ex)
        {
            return NotFound(ApiResponse<GetOrderByIdResponse>.Fail(ex.Message));
        }
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
            var response = await startOrderUseCase.ExecuteAsync(input);
            return CreatedAtAction(nameof(GetById), new { id = response.OrderId }, ApiResponse<StartOrderResponse>.Ok(response, "Pedido iniciado com sucesso."));
        }
        catch (BusinessException ex)
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
            var response = await addProductToOrderUseCase.ExecuteAsync(input);
            return Ok(ApiResponse<AddProductToOrderResponse>.Ok(response, "Produto adicionado ao pedido com sucesso."));
        }
        catch (BusinessException ex)
        {
            if (ex.Message.Contains("não encontrado"))
                return NotFound(ApiResponse<AddProductToOrderResponse>.Fail(ex.Message));
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
            var response = await updateProductInOrderUseCase.ExecuteAsync(input);
            return Ok(ApiResponse<UpdateProductInOrderResponse>.Ok(response, "Produto atualizado no pedido com sucesso."));
        }
        catch (BusinessException ex)
        {
            if (ex.Message.Contains("não encontrado"))
                return NotFound(ApiResponse<UpdateProductInOrderResponse>.Fail(ex.Message));
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
        try
        {
            var response = await removeProductFromOrderUseCase.ExecuteAsync(input);
            return Ok(ApiResponse<RemoveProductFromOrderResponse>.Ok(response, "Produto removido do pedido com sucesso."));
        }
        catch (BusinessException ex)
        {
            return NotFound(ApiResponse<RemoveProductFromOrderResponse>.Fail(ex.Message));
        }
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
            var response = await confirmOrderSelectionUseCase.ExecuteAsync(input);
            return Ok(ApiResponse<ConfirmOrderSelectionResponse>.Ok(response, "Seleção do pedido confirmada com sucesso."));
        }
        catch (BusinessException ex)
        {
            if (ex.Message.Contains("não encontrado"))
                return NotFound(ApiResponse<ConfirmOrderSelectionResponse>.Fail(ex.Message));
            return BadRequest(ApiResponse<ConfirmOrderSelectionResponse>.Fail(ex.Message));
        }
    }
}
