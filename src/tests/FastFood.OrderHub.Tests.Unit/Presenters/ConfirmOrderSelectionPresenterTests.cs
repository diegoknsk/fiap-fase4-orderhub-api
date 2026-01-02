using FastFood.OrderHub.Application.OutputModels.OrderManagement;
using FastFood.OrderHub.Application.Presenters.OrderManagement;
using FastFood.OrderHub.Application.Responses.OrderManagement;
using FastFood.OrderHub.Domain.Common.Enums;
using Xunit;

namespace FastFood.OrderHub.Tests.Unit.Presenters;

/// <summary>
/// Testes unitários para ConfirmOrderSelectionPresenter
/// </summary>
public class ConfirmOrderSelectionPresenterTests
{
    private readonly ConfirmOrderSelectionPresenter _presenter;

    public ConfirmOrderSelectionPresenterTests()
    {
        _presenter = new ConfirmOrderSelectionPresenter();
    }

    [Fact]
    public void Present_WhenValidOutput_ShouldReturnResponse()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var output = new ConfirmOrderSelectionOutputModel
        {
            OrderId = orderId,
            OrderStatus = (int)EnumOrderStatus.AwaitingPayment,
            TotalPrice = 50.00m
        };

        // Act
        var result = _presenter.Present(output);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(orderId, result.OrderId);
        Assert.Equal((int)EnumOrderStatus.AwaitingPayment, result.OrderStatus);
        Assert.Equal(50.00m, result.TotalPrice);
    }
}
