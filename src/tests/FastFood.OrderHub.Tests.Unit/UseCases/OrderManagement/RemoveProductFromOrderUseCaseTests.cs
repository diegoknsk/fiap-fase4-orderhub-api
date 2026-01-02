using FastFood.OrderHub.Application.DTOs;
using FastFood.OrderHub.Application.InputModels.OrderManagement;
using FastFood.OrderHub.Application.Ports;
using FastFood.OrderHub.Application.Presenters.OrderManagement;
using FastFood.OrderHub.Application.UseCases.OrderManagement;
using FastFood.OrderHub.Domain.Common.Enums;
using Moq;
using Xunit;

namespace FastFood.OrderHub.Tests.Unit.UseCases.OrderManagement;

/// <summary>
/// Testes unitários para RemoveProductFromOrderUseCase
/// </summary>
public class RemoveProductFromOrderUseCaseTests
{
    private readonly Mock<IOrderDataSource> _orderDataSourceMock;
    private readonly RemoveProductFromOrderPresenter _presenter;
    private readonly RemoveProductFromOrderUseCase _useCase;

    public RemoveProductFromOrderUseCaseTests()
    {
        _orderDataSourceMock = new Mock<IOrderDataSource>();
        _presenter = new RemoveProductFromOrderPresenter();
        _useCase = new RemoveProductFromOrderUseCase(_orderDataSourceMock.Object, _presenter);
    }

    [Fact]
    public async Task ExecuteAsync_WhenValidInput_ShouldRemoveProductFromOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderedProductId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var orderDto = new OrderDto
        {
            Id = orderId,
            Code = "ORD-001",
            CustomerId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            OrderStatus = (int)EnumOrderStatus.Started,
            TotalPrice = 50.00m,
            Items = new List<OrderedProductDto>
            {
                new OrderedProductDto
                {
                    Id = orderedProductId,
                    ProductId = productId,
                    OrderId = orderId,
                    Quantity = 2,
                    FinalPrice = 25.00m,
                    CustomIngredients = new List<OrderedProductIngredientDto>()
                }
            }
        };

        var input = new RemoveProductFromOrderInputModel
        {
            OrderId = orderId,
            OrderedProductId = orderedProductId
        };

        _orderDataSourceMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(orderDto);

        _orderDataSourceMock
            .Setup(x => x.UpdateAsync(It.IsAny<OrderDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.ExecuteAsync(input);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(orderId, result.OrderId);
        Assert.Equal(orderedProductId, result.OrderedProductId);
        Assert.Equal(0, result.TotalPrice);

        _orderDataSourceMock.Verify(
            x => x.GetByIdAsync(orderId),
            Times.Once);

        _orderDataSourceMock.Verify(
            x => x.UpdateAsync(It.Is<OrderDto>(o =>
                o.Id == orderId &&
                o.Items.Count == 0 &&
                o.TotalPrice == 0)),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOrderDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var input = new RemoveProductFromOrderInputModel
        {
            OrderId = orderId,
            OrderedProductId = Guid.NewGuid()
        };

        _orderDataSourceMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync((OrderDto?)null);

        // Act
        var result = await _useCase.ExecuteAsync(input);

        // Assert
        Assert.Null(result);
        _orderDataSourceMock.Verify(x => x.UpdateAsync(It.IsAny<OrderDto>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOrderedProductDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderedProductId = Guid.NewGuid();

        var orderDto = new OrderDto
        {
            Id = orderId,
            Code = "ORD-001",
            CustomerId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            OrderStatus = (int)EnumOrderStatus.Started,
            TotalPrice = 50.00m,
            Items = new List<OrderedProductDto>()
        };

        var input = new RemoveProductFromOrderInputModel
        {
            OrderId = orderId,
            OrderedProductId = orderedProductId
        };

        _orderDataSourceMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(orderDto);

        // Act
        var result = await _useCase.ExecuteAsync(input);

        // Assert
        Assert.Null(result);
        _orderDataSourceMock.Verify(x => x.UpdateAsync(It.IsAny<OrderDto>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMultipleProducts_ShouldRemoveOnlySpecifiedProduct()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderedProductId1 = Guid.NewGuid();
        var orderedProductId2 = Guid.NewGuid();
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();

        var orderDto = new OrderDto
        {
            Id = orderId,
            Code = "ORD-001",
            CustomerId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            OrderStatus = (int)EnumOrderStatus.Started,
            TotalPrice = 75.00m,
            Items = new List<OrderedProductDto>
            {
                new OrderedProductDto
                {
                    Id = orderedProductId1,
                    ProductId = productId1,
                    OrderId = orderId,
                    Quantity = 2,
                    FinalPrice = 25.00m,
                    CustomIngredients = new List<OrderedProductIngredientDto>()
                },
                new OrderedProductDto
                {
                    Id = orderedProductId2,
                    ProductId = productId2,
                    OrderId = orderId,
                    Quantity = 1,
                    FinalPrice = 50.00m,
                    CustomIngredients = new List<OrderedProductIngredientDto>()
                }
            }
        };

        var input = new RemoveProductFromOrderInputModel
        {
            OrderId = orderId,
            OrderedProductId = orderedProductId1
        };

        _orderDataSourceMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(orderDto);

        _orderDataSourceMock
            .Setup(x => x.UpdateAsync(It.IsAny<OrderDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.ExecuteAsync(input);

        // Assert
        Assert.NotNull(result);
        _orderDataSourceMock.Verify(
            x => x.UpdateAsync(It.Is<OrderDto>(o =>
                o.Id == orderId &&
                o.Items.Count == 1 &&
                o.Items[0].Id == orderedProductId2 &&
                o.TotalPrice == 50.00m)),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOrderHasCustomIngredients_ShouldRemoveProductWithIngredients()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderedProductId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();

        var orderDto = new OrderDto
        {
            Id = orderId,
            Code = "ORD-001",
            CustomerId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            OrderStatus = (int)EnumOrderStatus.Started,
            TotalPrice = 50.00m,
            Items = new List<OrderedProductDto>
            {
                new OrderedProductDto
                {
                    Id = orderedProductId,
                    ProductId = productId,
                    OrderId = orderId,
                    Quantity = 2,
                    FinalPrice = 25.00m,
                    CustomIngredients = new List<OrderedProductIngredientDto>
                    {
                        new OrderedProductIngredientDto
                        {
                            Id = ingredientId,
                            Name = "Extra Cheese",
                            Price = 2.50m,
                            Quantity = 2
                        }
                    }
                }
            }
        };

        var input = new RemoveProductFromOrderInputModel
        {
            OrderId = orderId,
            OrderedProductId = orderedProductId
        };

        _orderDataSourceMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(orderDto);

        _orderDataSourceMock
            .Setup(x => x.UpdateAsync(It.IsAny<OrderDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.ExecuteAsync(input);

        // Assert
        Assert.NotNull(result);
        _orderDataSourceMock.Verify(
            x => x.UpdateAsync(It.Is<OrderDto>(o =>
                o.Id == orderId &&
                o.Items.Count == 0 &&
                o.TotalPrice == 0)),
            Times.Once);
    }
}
