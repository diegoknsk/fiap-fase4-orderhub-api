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
/// Testes unitários para ConfirmOrderSelectionUseCase
/// </summary>
public class ConfirmOrderSelectionUseCaseTests
{
    private readonly Mock<IOrderDataSource> _orderDataSourceMock;
    private readonly ConfirmOrderSelectionPresenter _presenter;
    private readonly ConfirmOrderSelectionUseCase _useCase;

    public ConfirmOrderSelectionUseCaseTests()
    {
        _orderDataSourceMock = new Mock<IOrderDataSource>();
        _presenter = new ConfirmOrderSelectionPresenter();
        _useCase = new ConfirmOrderSelectionUseCase(_orderDataSourceMock.Object, _presenter);
    }

    [Fact]
    public async Task ExecuteAsync_WhenValidOrder_ShouldConfirmOrderSelection()
    {
        // Arrange
        var orderId = Guid.NewGuid();
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
                    Id = Guid.NewGuid(),
                    ProductId = Guid.NewGuid(),
                    OrderId = orderId,
                    Quantity = 2,
                    FinalPrice = 25.00m,
                    CustomIngredients = new List<OrderedProductIngredientDto>()
                }
            }
        };

        var input = new ConfirmOrderSelectionInputModel
        {
            OrderId = orderId
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
        Assert.Equal((int)EnumOrderStatus.AwaitingPayment, result.OrderStatus);
        Assert.Equal(50.00m, result.TotalPrice);

        _orderDataSourceMock.Verify(
            x => x.GetByIdAsync(orderId),
            Times.Once);

        _orderDataSourceMock.Verify(
            x => x.UpdateAsync(It.Is<OrderDto>(o =>
                o.Id == orderId &&
                o.OrderStatus == (int)EnumOrderStatus.AwaitingPayment)),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOrderDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var input = new ConfirmOrderSelectionInputModel
        {
            OrderId = orderId
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
    public async Task ExecuteAsync_WhenOrderHasNoItems_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderDto = new OrderDto
        {
            Id = orderId,
            Code = "ORD-001",
            CustomerId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            OrderStatus = (int)EnumOrderStatus.Started,
            TotalPrice = 0,
            Items = new List<OrderedProductDto>()
        };

        var input = new ConfirmOrderSelectionInputModel
        {
            OrderId = orderId
        };

        _orderDataSourceMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(orderDto);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _useCase.ExecuteAsync(input));
        _orderDataSourceMock.Verify(x => x.UpdateAsync(It.IsAny<OrderDto>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOrderStatusIsNotStarted_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderDto = new OrderDto
        {
            Id = orderId,
            Code = "ORD-001",
            CustomerId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            OrderStatus = (int)EnumOrderStatus.AwaitingPayment,
            TotalPrice = 50.00m,
            Items = new List<OrderedProductDto>
            {
                new OrderedProductDto
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.NewGuid(),
                    OrderId = orderId,
                    Quantity = 2,
                    FinalPrice = 25.00m,
                    CustomIngredients = new List<OrderedProductIngredientDto>()
                }
            }
        };

        var input = new ConfirmOrderSelectionInputModel
        {
            OrderId = orderId
        };

        _orderDataSourceMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(orderDto);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _useCase.ExecuteAsync(input));
        _orderDataSourceMock.Verify(x => x.UpdateAsync(It.IsAny<OrderDto>()), Times.Never);
    }
}
