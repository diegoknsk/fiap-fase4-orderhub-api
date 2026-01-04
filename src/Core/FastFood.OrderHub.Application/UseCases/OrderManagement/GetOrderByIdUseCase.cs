using FastFood.OrderHub.Application.DTOs;
using FastFood.OrderHub.Application.Exceptions;
using FastFood.OrderHub.Application.InputModels.OrderManagement;
using FastFood.OrderHub.Application.OutputModels.OrderManagement;
using FastFood.OrderHub.Application.Ports;
using FastFood.OrderHub.Application.Presenters.OrderManagement;
using FastFood.OrderHub.Application.Responses.OrderManagement;

namespace FastFood.OrderHub.Application.UseCases.OrderManagement;

/// <summary>
/// UseCase para obter pedido por ID
/// </summary>
public class GetOrderByIdUseCase
{
    private readonly IOrderDataSource _orderDataSource;
    private readonly GetOrderByIdPresenter _presenter;
    private readonly IRequestContext _requestContext;

    public GetOrderByIdUseCase(
        IOrderDataSource orderDataSource,
        GetOrderByIdPresenter presenter,
        IRequestContext requestContext)
    {
        _orderDataSource = orderDataSource;
        _presenter = presenter;
        _requestContext = requestContext;
    }

    public async Task<GetOrderByIdResponse> ExecuteAsync(GetOrderByIdInputModel input)
    {
        OrderDto? orderDto;

        if (_requestContext.IsAdmin)
        {
            // Admin pode buscar qualquer pedido apenas por orderId
            orderDto = await _orderDataSource.GetByIdAsync(input.OrderId);
        }
        else
        {
            // Customer: obter CustomerId do token
            if (string.IsNullOrWhiteSpace(_requestContext.CustomerId))
                throw new BusinessException("CustomerId não encontrado no token.");

            if (!Guid.TryParse(_requestContext.CustomerId, out var customerId))
                throw new BusinessException("CustomerId inválido no token.");

            // Buscar pedido por orderId + customerId (garante que o pedido pertence ao cliente)
            orderDto = await _orderDataSource.GetByIdForCustomerAsync(input.OrderId, customerId);
        }

        if (orderDto == null)
            throw new BusinessException("Pedido não encontrado.");

        var output = AdaptToOutputModel(orderDto);
        return _presenter.Present(output);
    }

    private GetOrderByIdOutputModel AdaptToOutputModel(DTOs.OrderDto orderDto)
    {
        return new GetOrderByIdOutputModel
        {
            OrderId = orderDto.Id,
            Code = orderDto.Code,
            CustomerId = orderDto.CustomerId,
            CreatedAt = orderDto.CreatedAt,
            OrderStatus = orderDto.OrderStatus,
            TotalPrice = orderDto.TotalPrice,
            Items = orderDto.Items.Select(item => new OrderedProductOutputModel
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Category = item.Category,
                Quantity = item.Quantity,
                FinalPrice = item.FinalPrice,
                Observation = item.Observation,
                CustomIngredients = item.CustomIngredients.Select(ci => new OrderedProductIngredientOutputModel
                {
                    Id = ci.Id,
                    Name = ci.Name,
                    Price = ci.Price,
                    Quantity = ci.Quantity
                }).ToList()
            }).ToList()
        };
    }
}

