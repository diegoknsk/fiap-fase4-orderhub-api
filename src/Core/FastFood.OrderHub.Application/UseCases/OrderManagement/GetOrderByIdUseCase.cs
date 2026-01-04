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

    public GetOrderByIdUseCase(
        IOrderDataSource orderDataSource,
        GetOrderByIdPresenter presenter)
    {
        _orderDataSource = orderDataSource;
        _presenter = presenter;
    }

    public async Task<GetOrderByIdResponse> ExecuteAsync(GetOrderByIdInputModel input)
    {
        var orderDto = await _orderDataSource.GetByIdAsync(input.OrderId);

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

