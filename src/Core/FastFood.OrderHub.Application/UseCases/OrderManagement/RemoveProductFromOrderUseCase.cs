using FastFood.OrderHub.Application.DTOs;
using FastFood.OrderHub.Application.Exceptions;
using FastFood.OrderHub.Application.InputModels.OrderManagement;
using FastFood.OrderHub.Application.OutputModels.OrderManagement;
using FastFood.OrderHub.Application.Ports;
using FastFood.OrderHub.Application.Presenters.OrderManagement;
using FastFood.OrderHub.Application.Responses.OrderManagement;
using FastFood.OrderHub.Domain.Common.Enums;
using FastFood.OrderHub.Domain.Entities.OrderManagement;

namespace FastFood.OrderHub.Application.UseCases.OrderManagement;

/// <summary>
/// UseCase para remover produto do pedido
/// </summary>
public class RemoveProductFromOrderUseCase
{
    private readonly IOrderDataSource _orderDataSource;
    private readonly RemoveProductFromOrderPresenter _presenter;

    public RemoveProductFromOrderUseCase(
        IOrderDataSource orderDataSource,
        RemoveProductFromOrderPresenter presenter)
    {
        _orderDataSource = orderDataSource;
        _presenter = presenter;
    }

    public async Task<RemoveProductFromOrderResponse> ExecuteAsync(RemoveProductFromOrderInputModel input)
    {
        // Buscar pedido completo com Items
        var orderDto = await _orderDataSource.GetByIdAsync(input.OrderId);
        if (orderDto == null)
            throw new BusinessException("Pedido não encontrado.");

        // Converter OrderDto para entidade de domínio Order
        var order = ConvertToDomainEntity(orderDto);

        // Verificar se OrderedProduct existe
        var orderedProduct = order.OrderedProducts.FirstOrDefault(op => op.Id == input.OrderedProductId);
        if (orderedProduct == null)
            throw new BusinessException("Produto não encontrado no pedido.");

        // Remover produto usando método de domínio (recalcula TotalPrice automaticamente)
        order.RemoveProduct(input.OrderedProductId);

        // Converter entidade de domínio de volta para DTO
        orderDto = ConvertToDto(order, orderDto.OrderSource);

        // Salvar Order completo atualizado
        await _orderDataSource.UpdateAsync(orderDto);

        var output = AdaptToOutputModel(order, input.OrderedProductId);
        return _presenter.Present(output);
    }

    private Order ConvertToDomainEntity(OrderDto dto)
    {
        var order = new Order
        {
            Id = dto.Id,
            Code = dto.Code,
            CustomerId = dto.CustomerId,
            CreatedAt = dto.CreatedAt,
            OrderStatus = (EnumOrderStatus)dto.OrderStatus,
            TotalPrice = dto.TotalPrice,
            OrderedProducts = dto.Items.Select(item => new OrderedProduct
            {
                Id = item.Id,
                ProductId = item.ProductId,
                OrderId = item.OrderId,
                Quantity = item.Quantity,
                Observation = item.Observation,
                FinalPrice = item.FinalPrice,
                CustomIngredients = item.CustomIngredients.Select(ci => new OrderedProductIngredient
                {
                    Id = ci.Id,
                    Name = ci.Name,
                    Price = ci.Price,
                    Quantity = ci.Quantity,
                    OrderedProductId = ci.OrderedProductId,
                    ProductBaseIngredientId = ci.ProductBaseIngredientId
                }).ToList()
            }).ToList()
        };

        return order;
    }

    private OrderDto ConvertToDto(Order order, string? orderSource = null)
    {
        return new OrderDto
        {
            Id = order.Id,
            Code = order.Code,
            CustomerId = order.CustomerId,
            CreatedAt = order.CreatedAt,
            OrderStatus = (int)order.OrderStatus,
            TotalPrice = order.TotalPrice,
            OrderSource = orderSource,
            Items = order.OrderedProducts.Select(op => new OrderedProductDto
            {
                Id = op.Id,
                ProductId = op.ProductId,
                OrderId = op.OrderId,
                Quantity = op.Quantity,
                Observation = op.Observation,
                FinalPrice = op.FinalPrice,
                ProductName = op.Product?.Name,
                Category = op.Product != null ? (int)op.Product.Category : null,
                CustomIngredients = op.CustomIngredients.Select(ci => new OrderedProductIngredientDto
                {
                    Id = ci.Id,
                    Name = ci.Name,
                    Price = ci.Price,
                    Quantity = ci.Quantity,
                    OrderedProductId = ci.OrderedProductId,
                    ProductBaseIngredientId = ci.ProductBaseIngredientId
                }).ToList()
            }).ToList()
        };
    }

    private RemoveProductFromOrderOutputModel AdaptToOutputModel(Order order, Guid orderedProductId)
    {
        return new RemoveProductFromOrderOutputModel
        {
            OrderId = order.Id,
            OrderedProductId = orderedProductId,
            TotalPrice = order.TotalPrice
        };
    }
}
