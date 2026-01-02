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
/// Testes unitários para StartOrderUseCase
/// </summary>
public class StartOrderUseCaseTests
{
    private readonly Mock<IOrderDataSource> _orderDataSourceMock;
    private readonly StartOrderPresenter _presenter;
    private readonly StartOrderUseCase _useCase;

    public StartOrderUseCaseTests()
    {
        _orderDataSourceMock = new Mock<IOrderDataSource>();
        _presenter = new StartOrderPresenter();
        _useCase = new StartOrderUseCase(_orderDataSourceMock.Object, _presenter);
    }

    [Fact]
    public async Task ExecuteAsync_WhenValidInput_ShouldCreateOrder()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var orderCode = "ORD-001";
        var input = new StartOrderInputModel
        {
            CustomerId = customerId
        };

        _orderDataSourceMock
            .Setup(x => x.GenerateOrderCodeAsync())
            .ReturnsAsync(orderCode);

        _orderDataSourceMock
            .Setup(x => x.AddAsync(It.IsAny<OrderDto>()))
            .ReturnsAsync(Guid.NewGuid());

        // Act
        var result = await _useCase.ExecuteAsync(input);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.OrderId);
        Assert.Equal(orderCode, result.Code);
        Assert.Equal(customerId, result.CustomerId);
        Assert.Equal((int)EnumOrderStatus.Started, result.OrderStatus);
        Assert.Equal(0, result.TotalPrice);
        Assert.True(result.CreatedAt <= DateTime.UtcNow);

        _orderDataSourceMock.Verify(
            x => x.GenerateOrderCodeAsync(),
            Times.Once);

        _orderDataSourceMock.Verify(
            x => x.AddAsync(It.Is<OrderDto>(o =>
                o.CustomerId == customerId &&
                o.Code == orderCode &&
                o.OrderStatus == (int)EnumOrderStatus.Started &&
                o.TotalPrice == 0 &&
                o.Items.Count == 0)),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldGenerateUniqueOrderId()
    {
        // Arrange
        var input = new StartOrderInputModel
        {
            CustomerId = Guid.NewGuid()
        };

        _orderDataSourceMock
            .Setup(x => x.GenerateOrderCodeAsync())
            .ReturnsAsync("ORD-001");

        _orderDataSourceMock
            .Setup(x => x.AddAsync(It.IsAny<OrderDto>()))
            .ReturnsAsync(Guid.NewGuid());

        // Act
        var result = await _useCase.ExecuteAsync(input);

        // Assert
        Assert.NotEqual(Guid.Empty, result.OrderId);
    }
}
