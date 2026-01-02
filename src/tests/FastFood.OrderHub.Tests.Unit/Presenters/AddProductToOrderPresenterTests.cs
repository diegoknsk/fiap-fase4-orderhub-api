using FastFood.OrderHub.Application.OutputModels.OrderManagement;
using FastFood.OrderHub.Application.Presenters.OrderManagement;
using FastFood.OrderHub.Application.Responses.OrderManagement;
using Xunit;

namespace FastFood.OrderHub.Tests.Unit.Presenters;

/// <summary>
/// Testes unitários para AddProductToOrderPresenter
/// </summary>
public class AddProductToOrderPresenterTests
{
    private readonly AddProductToOrderPresenter _presenter;

    public AddProductToOrderPresenterTests()
    {
        _presenter = new AddProductToOrderPresenter();
    }

    [Fact]
    public void Present_WhenValidOutput_ShouldReturnResponse()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderedProductId = Guid.NewGuid();
        var output = new AddProductToOrderOutputModel
        {
            OrderId = orderId,
            OrderedProductId = orderedProductId,
            TotalPrice = 50.00m
        };

        // Act
        var result = _presenter.Present(output);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(orderId, result.OrderId);
        Assert.Equal(orderedProductId, result.OrderedProductId);
        Assert.Equal(50.00m, result.TotalPrice);
    }
}
