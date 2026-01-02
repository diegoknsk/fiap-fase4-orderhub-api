using FastFood.OrderHub.Application.OutputModels.OrderManagement;
using FastFood.OrderHub.Application.Presenters.OrderManagement;
using FastFood.OrderHub.Application.Responses.OrderManagement;
using FastFood.OrderHub.Domain.Common.Enums;
using Xunit;

namespace FastFood.OrderHub.Tests.Unit.Presenters;

/// <summary>
/// Testes unitários para GetOrderByIdPresenter
/// </summary>
public class GetOrderByIdPresenterTests
{
    private readonly GetOrderByIdPresenter _presenter;

    public GetOrderByIdPresenterTests()
    {
        _presenter = new GetOrderByIdPresenter();
    }

    [Fact]
    public void Present_WhenValidOutput_ShouldReturnResponse()
    {
        // Arrange
        var output = new GetOrderByIdOutputModel
        {
            OrderId = Guid.NewGuid(),
            Code = "ORD-001",
            CustomerId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            OrderStatus = (int)EnumOrderStatus.Started,
            TotalPrice = 50.00m,
            Items = new List<OrderedProductOutputModel>()
        };

        // Act
        var result = _presenter.Present(output);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(output.OrderId, result.OrderId);
        Assert.Equal(output.Code, result.Code);
        Assert.Equal(output.CustomerId, result.CustomerId);
        Assert.Equal(output.CreatedAt, result.CreatedAt);
        Assert.Equal(output.OrderStatus, result.OrderStatus);
        Assert.Equal(output.TotalPrice, result.TotalPrice);
        Assert.NotNull(result.Items);
        Assert.Empty(result.Items);
    }

    [Fact]
    public void Present_WithItems_ShouldMapItemsCorrectly()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var orderedProductId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();

        var output = new GetOrderByIdOutputModel
        {
            OrderId = orderId,
            Code = "ORD-001",
            CustomerId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            OrderStatus = (int)EnumOrderStatus.Started,
            TotalPrice = 50.00m,
            Items = new List<OrderedProductOutputModel>
            {
                new OrderedProductOutputModel
                {
                    Id = orderedProductId,
                    ProductId = productId,
                    ProductName = "Test Product",
                    Category = 1,
                    Quantity = 2,
                    FinalPrice = 25.00m,
                    Observation = "Test observation",
                    CustomIngredients = new List<OrderedProductIngredientOutputModel>
                    {
                        new OrderedProductIngredientOutputModel
                        {
                            Id = ingredientId,
                            Name = "Extra Cheese",
                            Price = 2.50m,
                            Quantity = 2
                        }
                    }
                }
            }
        };

        // Act
        var result = _presenter.Present(output);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(orderedProductId, result.Items[0].Id);
        Assert.Equal(productId, result.Items[0].ProductId);
        Assert.Equal("Test Product", result.Items[0].ProductName);
        Assert.Equal(1, result.Items[0].Category);
        Assert.Equal(2, result.Items[0].Quantity);
        Assert.Equal(25.00m, result.Items[0].FinalPrice);
        Assert.Equal("Test observation", result.Items[0].Observation);
        Assert.Single(result.Items[0].CustomIngredients);
        Assert.Equal(ingredientId, result.Items[0].CustomIngredients[0].Id);
        Assert.Equal("Extra Cheese", result.Items[0].CustomIngredients[0].Name);
        Assert.Equal(2.50m, result.Items[0].CustomIngredients[0].Price);
        Assert.Equal(2, result.Items[0].CustomIngredients[0].Quantity);
    }

    [Fact]
    public void Present_WithMultipleItems_ShouldMapAllItems()
    {
        // Arrange
        var output = new GetOrderByIdOutputModel
        {
            OrderId = Guid.NewGuid(),
            Code = "ORD-001",
            CustomerId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            OrderStatus = (int)EnumOrderStatus.Started,
            TotalPrice = 75.00m,
            Items = new List<OrderedProductOutputModel>
            {
                new OrderedProductOutputModel
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.NewGuid(),
                    Quantity = 2,
                    FinalPrice = 25.00m,
                    CustomIngredients = new List<OrderedProductIngredientOutputModel>()
                },
                new OrderedProductOutputModel
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.NewGuid(),
                    Quantity = 1,
                    FinalPrice = 50.00m,
                    CustomIngredients = new List<OrderedProductIngredientOutputModel>()
                }
            }
        };

        // Act
        var result = _presenter.Present(output);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public void Present_WithNullValues_ShouldHandleNulls()
    {
        // Arrange
        var output = new GetOrderByIdOutputModel
        {
            OrderId = Guid.NewGuid(),
            Code = null,
            CustomerId = null,
            CreatedAt = DateTime.UtcNow,
            OrderStatus = (int)EnumOrderStatus.Started,
            TotalPrice = 0,
            Items = new List<OrderedProductOutputModel>
            {
                new OrderedProductOutputModel
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.NewGuid(),
                    ProductName = null,
                    Category = null,
                    Quantity = 1,
                    FinalPrice = 0,
                    Observation = null,
                    CustomIngredients = new List<OrderedProductIngredientOutputModel>()
                }
            }
        };

        // Act
        var result = _presenter.Present(output);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Code);
        Assert.Null(result.CustomerId);
        Assert.Null(result.Items[0].ProductName);
        Assert.Null(result.Items[0].Category);
        Assert.Null(result.Items[0].Observation);
    }
}
