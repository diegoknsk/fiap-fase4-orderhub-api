using FastFood.OrderHub.Application.OutputModels.OrderManagement;
using FastFood.OrderHub.Application.Presenters.OrderManagement;
using FastFood.OrderHub.Application.Responses.OrderManagement;
using FastFood.OrderHub.Domain.Common.Enums;
using Xunit;

namespace FastFood.OrderHub.Tests.Unit.Presenters;

/// <summary>
/// Testes unitários para GetPagedOrdersPresenter
/// </summary>
public class GetPagedOrdersPresenterTests
{
    private readonly GetPagedOrdersPresenter _presenter;

    public GetPagedOrdersPresenterTests()
    {
        _presenter = new GetPagedOrdersPresenter();
    }

    [Fact]
    public void Present_WhenValidOutput_ShouldReturnResponse()
    {
        // Arrange
        var output = new GetPagedOrdersOutputModel
        {
            Page = 1,
            PageSize = 10,
            TotalCount = 25,
            TotalPages = 3,
            HasNextPage = true,
            Items = new List<OrderSummaryOutputModel>
            {
                new OrderSummaryOutputModel
                {
                    OrderId = Guid.NewGuid(),
                    Code = "ORD-001",
                    CustomerId = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    OrderStatus = (int)EnumOrderStatus.Started,
                    TotalPrice = 50.00m
                }
            }
        };

        // Act
        var result = _presenter.Present(output);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(25, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.True(result.HasNextPage);
        Assert.Single(result.Items);
        Assert.Equal("ORD-001", result.Items[0].Code);
    }

    [Fact]
    public void Present_WithMultipleItems_ShouldMapAllItems()
    {
        // Arrange
        var output = new GetPagedOrdersOutputModel
        {
            Page = 1,
            PageSize = 10,
            TotalCount = 2,
            TotalPages = 1,
            HasNextPage = false,
            Items = new List<OrderSummaryOutputModel>
            {
                new OrderSummaryOutputModel
                {
                    OrderId = Guid.NewGuid(),
                    Code = "ORD-001",
                    CustomerId = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    OrderStatus = (int)EnumOrderStatus.Started,
                    TotalPrice = 50.00m
                },
                new OrderSummaryOutputModel
                {
                    OrderId = Guid.NewGuid(),
                    Code = "ORD-002",
                    CustomerId = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    OrderStatus = (int)EnumOrderStatus.AwaitingPayment,
                    TotalPrice = 75.00m
                }
            }
        };

        // Act
        var result = _presenter.Present(output);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal("ORD-001", result.Items[0].Code);
        Assert.Equal("ORD-002", result.Items[1].Code);
    }

    [Fact]
    public void Present_WithEmptyItems_ShouldReturnEmptyList()
    {
        // Arrange
        var output = new GetPagedOrdersOutputModel
        {
            Page = 1,
            PageSize = 10,
            TotalCount = 0,
            TotalPages = 0,
            HasNextPage = false,
            Items = new List<OrderSummaryOutputModel>()
        };

        // Act
        var result = _presenter.Present(output);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
        Assert.False(result.HasNextPage);
    }
}
