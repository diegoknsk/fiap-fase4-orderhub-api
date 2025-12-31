using FastFood.OrderHub.Application.DTOs;
using FastFood.OrderHub.Application.InputModels.OrderManagement;
using FastFood.OrderHub.Application.OutputModels.OrderManagement;
using FastFood.OrderHub.Application.Ports;
using FastFood.OrderHub.Application.Presenters.OrderManagement;
using FastFood.OrderHub.Application.Responses.OrderManagement;
using FastFood.OrderHub.Domain.Common.Enums;
using FastFood.OrderHub.Domain.Entities.OrderManagement;

namespace FastFood.OrderHub.Application.UseCases.OrderManagement;

/// <summary>
/// UseCase para iniciar novo pedido
/// </summary>
public class StartOrderUseCase
{
    private readonly IOrderDataSource _orderDataSource;
    private readonly StartOrderPresenter _presenter;

    public StartOrderUseCase(
        IOrderDataSource orderDataSource,
        StartOrderPresenter presenter)
    {
        _orderDataSource = orderDataSource;
        _presenter = presenter;
    }

    public async Task<StartOrderResponse> ExecuteAsync(StartOrderInputModel input)
    {
        // Gerar código único do pedido
        var code = await _orderDataSource.GenerateOrderCodeAsync();

        // Criar entidade de domínio Order
        var order = new Order
        {
            Id = Guid.NewGuid(),
            Code = code,
            CustomerId = input.CustomerId,
            CreatedAt = DateTime.UtcNow,
            OrderStatus = EnumOrderStatus.Started,
            TotalPrice = 0,
            OrderedProducts = new List<OrderedProduct>()
        };

        // Converter entidade de domínio para DTO
        var orderDto = new OrderDto
        {
            Id = order.Id,
            Code = order.Code,
            CustomerId = order.CustomerId,
            CreatedAt = order.CreatedAt,
            OrderStatus = (int)order.OrderStatus,
            TotalPrice = order.TotalPrice,
            Items = new List<OrderedProductDto>()
        };

        // Salvar no DataSource
        await _orderDataSource.AddAsync(orderDto);

        // Criar OutputModel
        var output = new StartOrderOutputModel
        {
            OrderId = order.Id,
            Code = order.Code,
            CustomerId = order.CustomerId,
            CreatedAt = order.CreatedAt,
            OrderStatus = (int)order.OrderStatus,
            TotalPrice = order.TotalPrice
        };

        return _presenter.Present(output);
    }
}

