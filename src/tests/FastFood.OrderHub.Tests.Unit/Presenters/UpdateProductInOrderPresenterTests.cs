using FastFood.OrderHub.Application.OutputModels.OrderManagement;
using FastFood.OrderHub.Application.Presenters.OrderManagement;
using FastFood.OrderHub.Application.Responses.OrderManagement;
using Xunit;

namespace FastFood.OrderHub.Tests.Unit.Presenters;

/// <summary>
/// Testes unitários para UpdateProductInOrderPresenter
/// </summary>
public class UpdateProductInOrderPresenterTests
{
    private readonly UpdateProductInOrderPresenter _presenter;

    public UpdateProductInOrderPresenterTests()
    {
        _presenter = new UpdateProductInOrderPresenter();
    }

    [Fact]
    public void Present_WhenValidOutput_ShouldReturnResponse()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderedProductId = Guid.NewGuid();
        var output = new UpdateProductInOrderOutputModel
        {
            OrderId = orderId,
            OrderedProductId = orderedProductId,
            TotalPrice = 75.00m
        };

        // Act
        var result = _presenter.Present(output);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(orderId, result.OrderId);
        Assert.Equal(orderedProductId, result.OrderedProductId);
        Assert.Equal(75.00m, result.TotalPrice);
    }
}
