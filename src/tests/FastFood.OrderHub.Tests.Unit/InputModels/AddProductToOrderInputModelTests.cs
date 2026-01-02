using FastFood.OrderHub.Application.InputModels.OrderManagement;
using Xunit;

namespace FastFood.OrderHub.Tests.Unit.InputModels;

/// <summary>
/// Testes unitários para AddProductToOrderInputModel
/// </summary>
public class AddProductToOrderInputModelTests
{
    [Fact]
    public void AddProductToOrderInputModel_WhenCreated_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var input = new AddProductToOrderInputModel();

        // Assert
        Assert.Equal(Guid.Empty, input.OrderId);
        Assert.Equal(Guid.Empty, input.ProductId);
        Assert.Equal(0, input.Quantity);
        Assert.Null(input.Observation);
        Assert.NotNull(input.CustomIngredients);
        Assert.Empty(input.CustomIngredients);
    }

    [Fact]
    public void AddProductToOrderInputModel_WhenPropertiesSet_ShouldStoreValues()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var baseIngredientId = Guid.NewGuid();

        // Act
        var input = new AddProductToOrderInputModel
        {
            OrderId = orderId,
            ProductId = productId,
            Quantity = 2,
            Observation = "Test observation",
            CustomIngredients = new List<CustomIngredientInputModel>
            {
                new CustomIngredientInputModel
                {
                    ProductBaseIngredientId = baseIngredientId,
                    Quantity = 5
                }
            }
        };

        // Assert
        Assert.Equal(orderId, input.OrderId);
        Assert.Equal(productId, input.ProductId);
        Assert.Equal(2, input.Quantity);
        Assert.Equal("Test observation", input.Observation);
        Assert.Single(input.CustomIngredients);
        Assert.Equal(baseIngredientId, input.CustomIngredients[0].ProductBaseIngredientId);
        Assert.Equal(5, input.CustomIngredients[0].Quantity);
    }

    [Fact]
    public void CustomIngredientInputModel_WhenCreated_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var input = new CustomIngredientInputModel();

        // Assert
        Assert.Equal(Guid.Empty, input.ProductBaseIngredientId);
        Assert.Equal(0, input.Quantity);
    }
}
