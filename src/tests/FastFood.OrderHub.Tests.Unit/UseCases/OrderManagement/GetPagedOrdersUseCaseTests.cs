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
/// Testes unitários para GetPagedOrdersUseCase
/// </summary>
public class GetPagedOrdersUseCaseTests
{
    private readonly Mock<IOrderDataSource> _orderDataSourceMock;
    private readonly GetPagedOrdersPresenter _presenter;
    private readonly GetPagedOrdersUseCase _useCase;

    public GetPagedOrdersUseCaseTests()
    {
        _orderDataSourceMock = new Mock<IOrderDataSource>();
        _presenter = new GetPagedOrdersPresenter();
        _useCase = new GetPagedOrdersUseCase(_orderDataSourceMock.Object, _presenter);
    }

    [Fact]
    public async Task ExecuteAsync_WhenValidInput_ShouldReturnPagedOrders()
    {
        // Arrange
        var orders = new List<OrderDto>
        {
            new OrderDto
            {
                Id = Guid.NewGuid(),
                Code = "ORD-001",
                CustomerId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                OrderStatus = (int)EnumOrderStatus.Started,
                TotalPrice = 50.00m,
                Items = new List<OrderedProductDto>()
            },
            new OrderDto
            {
                Id = Guid.NewGuid(),
                Code = "ORD-002",
                CustomerId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                OrderStatus = (int)EnumOrderStatus.Started,
                TotalPrice = 75.00m,
                Items = new List<OrderedProductDto>()
            }
        };

        var input = new GetPagedOrdersInputModel
        {
            Page = 1,
            PageSize = 10
        };

        _orderDataSourceMock
            .Setup(x => x.GetPagedAsync(1, 10, null))
            .ReturnsAsync(orders);

        // Act
        var result = await _useCase.ExecuteAsync(input);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(1, result.TotalPages);
        Assert.False(result.HasNextPage);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPageIsLessThanOne_ShouldSetPageToOne()
    {
        // Arrange
        var orders = new List<OrderDto>();
        var input = new GetPagedOrdersInputModel
        {
            Page = 0,
            PageSize = 10
        };

        _orderDataSourceMock
            .Setup(x => x.GetPagedAsync(1, 10, null))
            .ReturnsAsync(orders);

        // Act
        var result = await _useCase.ExecuteAsync(input);

        // Assert
        Assert.Equal(1, result.Page);
        _orderDataSourceMock.Verify(x => x.GetPagedAsync(1, 10, null), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPageSizeIsLessThanOne_ShouldSetPageSizeToTen()
    {
        // Arrange
        var orders = new List<OrderDto>();
        var input = new GetPagedOrdersInputModel
        {
            Page = 1,
            PageSize = 0
        };

        _orderDataSourceMock
            .Setup(x => x.GetPagedAsync(1, 10, null))
            .ReturnsAsync(orders);

        // Act
        var result = await _useCase.ExecuteAsync(input);

        // Assert
        Assert.Equal(10, result.PageSize);
        _orderDataSourceMock.Verify(x => x.GetPagedAsync(1, 10, null), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPageSizeIsGreaterThan100_ShouldSetPageSizeTo100()
    {
        // Arrange
        var orders = new List<OrderDto>();
        var input = new GetPagedOrdersInputModel
        {
            Page = 1,
            PageSize = 150
        };

        _orderDataSourceMock
            .Setup(x => x.GetPagedAsync(1, 100, null))
            .ReturnsAsync(orders);

        // Act
        var result = await _useCase.ExecuteAsync(input);

        // Assert
        Assert.Equal(100, result.PageSize);
        _orderDataSourceMock.Verify(x => x.GetPagedAsync(1, 100, null), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenStatusIsProvided_ShouldFilterByStatus()
    {
        // Arrange
        var orders = new List<OrderDto>();
        var input = new GetPagedOrdersInputModel
        {
            Page = 1,
            PageSize = 10,
            Status = (int)EnumOrderStatus.Started
        };

        _orderDataSourceMock
            .Setup(x => x.GetPagedAsync(1, 10, (int)EnumOrderStatus.Started))
            .ReturnsAsync(orders);

        // Act
        var result = await _useCase.ExecuteAsync(input);

        // Assert
        _orderDataSourceMock.Verify(
            x => x.GetPagedAsync(1, 10, (int)EnumOrderStatus.Started),
            Times.Once);
    }
}
