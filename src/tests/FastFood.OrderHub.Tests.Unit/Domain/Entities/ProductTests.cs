using FastFood.OrderHub.Domain.Common.Enums;
using FastFood.OrderHub.Domain.Entities.OrderManagement;
using Xunit;

namespace FastFood.OrderHub.Tests.Unit.Domain.Entities;

/// <summary>
/// Testes unitários para entidade Product
/// </summary>
public class ProductTests
{
    [Fact]
    public void Product_WhenCreated_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var product = new Product();

        // Assert
        Assert.Equal(Guid.Empty, product.Id);
        Assert.Null(product.Name);
        Assert.Equal(default(EnumProductCategory), product.Category);
        Assert.NotNull(product.Ingredients);
        Assert.Empty(product.Ingredients);
        Assert.Equal(0, product.Price);
        Assert.Null(product.Image);
        Assert.Null(product.Description);
    }

    [Fact]
    public void IsValid_WhenNameIsEmpty_ShouldReturnFalse()
    {
        // Arrange
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = string.Empty,
            Price = 10.00m
        };

        // Act
        var result = product.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_WhenNameIsNull_ShouldReturnFalse()
    {
        // Arrange
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = null,
            Price = 10.00m
        };

        // Act
        var result = product.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_WhenPriceIsZero_ShouldReturnFalse()
    {
        // Arrange
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Price = 0
        };

        // Act
        var result = product.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_WhenPriceIsNegative_ShouldReturnFalse()
    {
        // Arrange
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Price = -10.00m
        };

        // Act
        var result = product.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_WhenNameAndPriceAreValid_ShouldReturnTrue()
    {
        // Arrange
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Price = 10.00m
        };

        // Act
        var result = product.IsValid();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_WhenNameHasWhitespace_ShouldReturnFalse()
    {
        // Arrange
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "   ",
            Price = 10.00m
        };

        // Act
        var result = product.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Product_WhenPropertiesSet_ShouldStoreValues()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Test Product";
        var category = EnumProductCategory.Meal;
        var price = 15.50m;
        var description = "Test Description";

        // Act
        var product = new Product
        {
            Id = id,
            Name = name,
            Category = category,
            Price = price,
            Description = description
        };

        // Assert
        Assert.Equal(id, product.Id);
        Assert.Equal(name, product.Name);
        Assert.Equal(category, product.Category);
        Assert.Equal(price, product.Price);
        Assert.Equal(description, product.Description);
    }

    [Fact]
    public void Product_WhenIngredientsAdded_ShouldStoreIngredients()
    {
        // Arrange
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Price = 10.00m
        };

        var ingredient1 = new ProductBaseIngredient
        {
            Id = Guid.NewGuid(),
            Name = "Cheese",
            Price = 2.50m,
            ProductId = product.Id
        };

        var ingredient2 = new ProductBaseIngredient
        {
            Id = Guid.NewGuid(),
            Name = "Bacon",
            Price = 3.00m,
            ProductId = product.Id
        };

        // Act
        product.Ingredients.Add(ingredient1);
        product.Ingredients.Add(ingredient2);

        // Assert
        Assert.Equal(2, product.Ingredients.Count);
        Assert.Contains(ingredient1, product.Ingredients);
        Assert.Contains(ingredient2, product.Ingredients);
    }
}
