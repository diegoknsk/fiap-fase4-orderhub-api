using FastFood.OrderHub.Domain.Entities.OrderManagement;
using Xunit;

namespace FastFood.OrderHub.Tests.Unit.Domain.Entities;

/// <summary>
/// Testes unitários para entidade OrderedProduct
/// </summary>
public class OrderedProductTests
{
    [Fact]
    public void OrderedProduct_WhenCreated_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var orderedProduct = new OrderedProduct();

        // Assert
        Assert.Equal(Guid.Empty, orderedProduct.Id);
        Assert.Equal(Guid.Empty, orderedProduct.ProductId);
        Assert.Null(orderedProduct.Product);
        Assert.Null(orderedProduct.OrderId);
        Assert.NotNull(orderedProduct.CustomIngredients);
        Assert.Empty(orderedProduct.CustomIngredients);
        Assert.Null(orderedProduct.Observation);
        Assert.Equal(0, orderedProduct.FinalPrice);
        Assert.Equal(0, orderedProduct.Quantity);
    }

    [Fact]
    public void CalculateFinalPrice_WhenProductHasPrice_ShouldCalculateCorrectly()
    {
        // Arrange
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Price = 10.00m
        };
        var orderedProduct = new OrderedProduct
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Product = product,
            Quantity = 2
        };

        // Act
        var finalPrice = orderedProduct.CalculateFinalPrice();

        // Assert
        Assert.Equal(20.00m, finalPrice);
        Assert.Equal(20.00m, orderedProduct.FinalPrice);
    }

    [Fact]
    public void CalculateFinalPrice_WhenProductIsNull_ShouldReturnZero()
    {
        // Arrange
        var orderedProduct = new OrderedProduct
        {
            Id = Guid.NewGuid(),
            Quantity = 2
        };

        // Act
        var finalPrice = orderedProduct.CalculateFinalPrice();

        // Assert
        Assert.Equal(0, finalPrice);
        Assert.Equal(0, orderedProduct.FinalPrice);
    }

    [Fact]
    public void CalculateFinalPrice_WithCustomIngredients_ShouldIncludeIngredientsPrice()
    {
        // Arrange
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Price = 10.00m
        };
        var orderedProduct = new OrderedProduct
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Product = product,
            Quantity = 2,
            CustomIngredients = new List<OrderedProductIngredient>
            {
                new OrderedProductIngredient
                {
                    Id = Guid.NewGuid(),
                    Name = "Extra Cheese",
                    Price = 2.50m,
                    Quantity = 2
                },
                new OrderedProductIngredient
                {
                    Id = Guid.NewGuid(),
                    Name = "Bacon",
                    Price = 3.00m,
                    Quantity = 1
                }
            }
        };

        // Act
        var finalPrice = orderedProduct.CalculateFinalPrice();

        // Assert
        // Base: 10.00 * 2 = 20.00
        // Ingredients: (2.50 * 2) + (3.00 * 1) = 5.00 + 3.00 = 8.00
        // Total: (10.00 + 8.00) * 2 = 36.00
        Assert.Equal(36.00m, finalPrice);
        Assert.Equal(36.00m, orderedProduct.FinalPrice);
    }

    [Fact]
    public void SetQuantity_ShouldUpdateQuantityAndRecalculatePrice()
    {
        // Arrange
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Price = 10.00m
        };
        var orderedProduct = new OrderedProduct
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Product = product,
            Quantity = 2
        };
        orderedProduct.CalculateFinalPrice();
        var initialPrice = orderedProduct.FinalPrice;

        // Act
        orderedProduct.SetQuantity(3);

        // Assert
        Assert.Equal(3, orderedProduct.Quantity);
        Assert.Equal(30.00m, orderedProduct.FinalPrice);
        Assert.NotEqual(initialPrice, orderedProduct.FinalPrice);
    }

    [Fact]
    public void SetIngredientQuantity_WhenIngredientExists_ShouldUpdateQuantity()
    {
        // Arrange
        var product = new Product { Id = Guid.NewGuid(), Price = 10.00m };
        var ingredientId = Guid.NewGuid();
        var orderedProduct = new OrderedProduct
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Product = product,
            Quantity = 1,
            CustomIngredients = new List<OrderedProductIngredient>
            {
                new OrderedProductIngredient
                {
                    Id = ingredientId,
                    Name = "Extra Cheese",
                    Price = 2.50m,
                    Quantity = 2
                }
            }
        };
        orderedProduct.CalculateFinalPrice();
        var initialPrice = orderedProduct.FinalPrice;

        // Act
        orderedProduct.SetIngredientQuantity(ingredientId, 5);

        // Assert
        Assert.Equal(5, orderedProduct.CustomIngredients.First().Quantity);
        Assert.NotEqual(initialPrice, orderedProduct.FinalPrice);
    }

    [Fact]
    public void SetIngredientQuantity_WhenQuantityExceedsMax_ShouldClampToMax()
    {
        // Arrange
        var product = new Product { Id = Guid.NewGuid(), Price = 10.00m };
        var ingredientId = Guid.NewGuid();
        var orderedProduct = new OrderedProduct
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Product = product,
            Quantity = 1,
            CustomIngredients = new List<OrderedProductIngredient>
            {
                new OrderedProductIngredient
                {
                    Id = ingredientId,
                    Name = "Extra Cheese",
                    Price = 2.50m,
                    Quantity = 2
                }
            }
        };

        // Act
        orderedProduct.SetIngredientQuantity(ingredientId, 15); // Excede máximo de 10

        // Assert
        Assert.Equal(10, orderedProduct.CustomIngredients.First().Quantity);
    }

    [Fact]
    public void SetIngredientQuantity_WhenQuantityIsNegative_ShouldClampToZero()
    {
        // Arrange
        var product = new Product { Id = Guid.NewGuid(), Price = 10.00m };
        var ingredientId = Guid.NewGuid();
        var orderedProduct = new OrderedProduct
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Product = product,
            Quantity = 1,
            CustomIngredients = new List<OrderedProductIngredient>
            {
                new OrderedProductIngredient
                {
                    Id = ingredientId,
                    Name = "Extra Cheese",
                    Price = 2.50m,
                    Quantity = 2
                }
            }
        };

        // Act
        orderedProduct.SetIngredientQuantity(ingredientId, -5);

        // Assert
        Assert.Equal(0, orderedProduct.CustomIngredients.First().Quantity);
    }

    [Fact]
    public void SetIngredientQuantity_WhenIngredientDoesNotExist_ShouldNotChangeAnything()
    {
        // Arrange
        var product = new Product { Id = Guid.NewGuid(), Price = 10.00m };
        var orderedProduct = new OrderedProduct
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Product = product,
            Quantity = 1,
            CustomIngredients = new List<OrderedProductIngredient>
            {
                new OrderedProductIngredient
                {
                    Id = Guid.NewGuid(),
                    Name = "Extra Cheese",
                    Price = 2.50m,
                    Quantity = 2
                }
            }
        };
        orderedProduct.CalculateFinalPrice();
        var initialPrice = orderedProduct.FinalPrice;
        var initialQuantity = orderedProduct.CustomIngredients.First().Quantity;

        // Act
        orderedProduct.SetIngredientQuantity(Guid.NewGuid(), 5); // ID que não existe

        // Assert
        Assert.Equal(initialQuantity, orderedProduct.CustomIngredients.First().Quantity);
        Assert.Equal(initialPrice, orderedProduct.FinalPrice);
    }

    [Fact]
    public void SetObservation_ShouldUpdateObservation()
    {
        // Arrange
        var orderedProduct = new OrderedProduct
        {
            Id = Guid.NewGuid(),
            Observation = "Original observation"
        };

        // Act
        orderedProduct.SetObservation("New observation");

        // Assert
        Assert.Equal("New observation", orderedProduct.Observation);
    }

    [Fact]
    public void SetObservation_WhenNull_ShouldSetToNull()
    {
        // Arrange
        var orderedProduct = new OrderedProduct
        {
            Id = Guid.NewGuid(),
            Observation = "Original observation"
        };

        // Act
        orderedProduct.SetObservation(null);

        // Assert
        Assert.Null(orderedProduct.Observation);
    }
}
