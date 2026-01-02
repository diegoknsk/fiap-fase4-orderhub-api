using FastFood.OrderHub.Application.OutputModels.OrderManagement;
using FastFood.OrderHub.Application.Presenters.OrderManagement;
using FastFood.OrderHub.Application.Responses.OrderManagement;
using FastFood.OrderHub.Domain.Common.Enums;
using Xunit;

namespace FastFood.OrderHub.Tests.Unit.Presenters;

/// <summary>
/// Testes unitários para StartOrderPresenter
/// </summary>
public class StartOrderPresenterTests
{
    private readonly StartOrderPresenter _presenter;

    public StartOrderPresenterTests()
    {
        _presenter = new StartOrderPresenter();
    }

    [Fact]
    public void Present_WhenValidOutput_ShouldReturnResponse()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var output = new StartOrderOutputModel
        {
            OrderId = orderId,
            Code = "ORD-001",
            CustomerId = customerId,
            CreatedAt = DateTime.UtcNow,
            OrderStatus = (int)EnumOrderStatus.Started,
            TotalPrice = 0
        };

        // Act
        var result = _presenter.Present(output);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(orderId, result.OrderId);
        Assert.Equal("ORD-001", result.Code);
        Assert.Equal(customerId, result.CustomerId);
        Assert.Equal(output.CreatedAt, result.CreatedAt);
        Assert.Equal((int)EnumOrderStatus.Started, result.OrderStatus);
        Assert.Equal(0, result.TotalPrice);
    }
}
