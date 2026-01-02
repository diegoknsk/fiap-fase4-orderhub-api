using FastFood.OrderHub.Domain.Entities.OrderManagement;
using Xunit;

namespace FastFood.OrderHub.Tests.Unit.Domain.Entities;

/// <summary>
/// Testes unitários para OrderedProductIngredient
/// </summary>
public class OrderedProductIngredientTests
{
    [Fact]
    public void OrderedProductIngredient_WhenCreated_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var ingredient = new OrderedProductIngredient();

        // Assert
        Assert.Equal(Guid.Empty, ingredient.Id);
        Assert.Null(ingredient.Name);
        Assert.Equal(0, ingredient.Price);
        Assert.Equal(0, ingredient.Quantity);
        Assert.Null(ingredient.OrderedProductId);
        Assert.Null(ingredient.ProductBaseIngredientId);
    }

    [Fact]
    public void OrderedProductIngredient_WhenPropertiesSet_ShouldStoreValues()
    {
        // Arrange
        var id = Guid.NewGuid();
        var orderedProductId = Guid.NewGuid();
        var baseIngredientId = Guid.NewGuid();
        var name = "Test Ingredient";
        var price = 5.50m;
        var quantity = 2;

        // Act
        var ingredient = new OrderedProductIngredient
        {
            Id = id,
            Name = name,
            Price = price,
            Quantity = quantity,
            OrderedProductId = orderedProductId,
            ProductBaseIngredientId = baseIngredientId
        };

        // Assert
        Assert.Equal(id, ingredient.Id);
        Assert.Equal(name, ingredient.Name);
        Assert.Equal(price, ingredient.Price);
        Assert.Equal(quantity, ingredient.Quantity);
        Assert.Equal(orderedProductId, ingredient.OrderedProductId);
        Assert.Equal(baseIngredientId, ingredient.ProductBaseIngredientId);
    }
}
