using FastFood.OrderHub.Application.OutputModels.OrderManagement;
using FastFood.OrderHub.Application.Presenters.OrderManagement;
using FastFood.OrderHub.Application.Responses.OrderManagement;
using Xunit;

namespace FastFood.OrderHub.Tests.Unit.Presenters;

/// <summary>
/// Testes unitários para RemoveProductFromOrderPresenter
/// </summary>
public class RemoveProductFromOrderPresenterTests
{
    private readonly RemoveProductFromOrderPresenter _presenter;

    public RemoveProductFromOrderPresenterTests()
    {
        _presenter = new RemoveProductFromOrderPresenter();
    }

    [Fact]
    public void Present_WhenValidOutput_ShouldReturnResponse()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderedProductId = Guid.NewGuid();
        var output = new RemoveProductFromOrderOutputModel
        {
            OrderId = orderId,
            OrderedProductId = orderedProductId,
            TotalPrice = 25.00m
        };

        // Act
        var result = _presenter.Present(output);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(orderId, result.OrderId);
        Assert.Equal(orderedProductId, result.OrderedProductId);
        Assert.Equal(25.00m, result.TotalPrice);
    }
}
