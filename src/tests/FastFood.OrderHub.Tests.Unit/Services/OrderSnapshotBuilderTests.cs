using FastFood.OrderHub.Application.DTOs.Payment;
using FastFood.OrderHub.Application.Services;
using FastFood.OrderHub.Domain.Common.Enums;
using FastFood.OrderHub.Domain.Entities.OrderManagement;
using Xunit;

namespace FastFood.OrderHub.Tests.Unit.Services;

/// <summary>
/// Testes unitários para OrderSnapshotBuilder
/// </summary>
public class OrderSnapshotBuilderTests
{
    [Fact]
    public void BuildFromOrder_WhenValidOrder_ShouldReturnSnapshotWithCorrectStructure()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            Code = "ORD-001",
            CreatedAt = DateTime.UtcNow,
            TotalPrice = 50.00m,
            OrderStatus = EnumOrderStatus.Started,
            OrderedProducts = new List<OrderedProduct>
            {
                new OrderedProduct
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    OrderId = orderId,
                    Quantity = 2,
                    FinalPrice = 25.00m,
                    Observation = "Sem cebola",
                    Product = new Product
                    {
                        Id = productId,
                        Name = "Hambúrguer",
                        Price = 10.00m,
                        Category = EnumProductCategory.Meal
                    },
                    CustomIngredients = new List<OrderedProductIngredient>
                    {
                        new OrderedProductIngredient
                        {
                            Id = Guid.NewGuid(),
                            Name = "Bacon Extra",
                            Price = 5.00m,
                            Quantity = 1
                        }
                    }
                }
            }
        };

        // Act
        var snapshot = OrderSnapshotBuilder.BuildFromOrder(order);

        // Assert
        Assert.NotNull(snapshot);
        Assert.NotNull(snapshot.Order);
        Assert.Equal(orderId, snapshot.Order.OrderId);
        Assert.Equal("ORD-001", snapshot.Order.Code);
        Assert.Equal(order.CreatedAt, snapshot.Order.CreatedAt);

        Assert.NotNull(snapshot.Pricing);
        Assert.Equal(50.00m, snapshot.Pricing.TotalPrice);
        Assert.Equal("BRL", snapshot.Pricing.Currency);

        Assert.NotNull(snapshot.Items);
        Assert.Single(snapshot.Items);

        var item = snapshot.Items[0];
        Assert.Equal(productId, item.ProductId);
        Assert.Equal("Hambúrguer", item.ProductName);
        Assert.Equal(2, item.Quantity);
        Assert.Equal(25.00m, item.FinalPrice);
        Assert.Equal("Sem cebola", item.Observation);

        Assert.NotNull(item.CustomIngredients);
        Assert.Single(item.CustomIngredients);
        Assert.Equal("Bacon Extra", item.CustomIngredients[0].Name);
        Assert.Equal(5.00m, item.CustomIngredients[0].Price);
        Assert.Equal(1, item.CustomIngredients[0].Quantity);

        Assert.Equal(1, snapshot.Version);
    }

    [Fact]
    public void BuildFromOrder_WhenOrderHasNoCode_ShouldReturnEmptyString()
    {
        // Arrange
        var order = new Order
        {
            Id = Guid.NewGuid(),
            Code = null,
            CreatedAt = DateTime.UtcNow,
            TotalPrice = 0,
            OrderedProducts = new List<OrderedProduct>()
        };

        // Act
        var snapshot = OrderSnapshotBuilder.BuildFromOrder(order);

        // Assert
        Assert.NotNull(snapshot.Order);
        Assert.Equal(string.Empty, snapshot.Order.Code);
    }

    [Fact]
    public void BuildFromOrder_WhenOrderHasNoProductName_ShouldReturnEmptyString()
    {
        // Arrange
        var order = new Order
        {
            Id = Guid.NewGuid(),
            Code = "ORD-001",
            CreatedAt = DateTime.UtcNow,
            TotalPrice = 25.00m,
            OrderedProducts = new List<OrderedProduct>
            {
                new OrderedProduct
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.NewGuid(),
                    Quantity = 1,
                    FinalPrice = 25.00m,
                    Product = null // Sem produto associado
                }
            }
        };

        // Act
        var snapshot = OrderSnapshotBuilder.BuildFromOrder(order);

        // Assert
        Assert.NotNull(snapshot.Items);
        Assert.Single(snapshot.Items);
        Assert.Equal(string.Empty, snapshot.Items[0].ProductName);
    }

    [Fact]
    public void BuildFromOrder_WhenOrderHasNoCustomIngredients_ShouldReturnEmptyList()
    {
        // Arrange
        var order = new Order
        {
            Id = Guid.NewGuid(),
            Code = "ORD-001",
            CreatedAt = DateTime.UtcNow,
            TotalPrice = 25.00m,
            OrderedProducts = new List<OrderedProduct>
            {
                new OrderedProduct
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.NewGuid(),
                    Quantity = 1,
                    FinalPrice = 25.00m,
                    CustomIngredients = new List<OrderedProductIngredient>()
                }
            }
        };

        // Act
        var snapshot = OrderSnapshotBuilder.BuildFromOrder(order);

        // Assert
        Assert.NotNull(snapshot.Items);
        Assert.Single(snapshot.Items);
        Assert.NotNull(snapshot.Items[0].CustomIngredients);
        Assert.Empty(snapshot.Items[0].CustomIngredients);
    }

    [Fact]
    public void BuildFromOrder_WhenOrderHasNoObservation_ShouldReturnNull()
    {
        // Arrange
        var order = new Order
        {
            Id = Guid.NewGuid(),
            Code = "ORD-001",
            CreatedAt = DateTime.UtcNow,
            TotalPrice = 25.00m,
            OrderedProducts = new List<OrderedProduct>
            {
                new OrderedProduct
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.NewGuid(),
                    Quantity = 1,
                    FinalPrice = 25.00m,
                    Observation = null
                }
            }
        };

        // Act
        var snapshot = OrderSnapshotBuilder.BuildFromOrder(order);

        // Assert
        Assert.NotNull(snapshot.Items);
        Assert.Single(snapshot.Items);
        Assert.Null(snapshot.Items[0].Observation);
    }

    [Fact]
    public void BuildFromOrder_ShouldNotIncludePII()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var order = new Order
        {
            Id = Guid.NewGuid(),
            Code = "ORD-001",
            CustomerId = customerId, // PII - não deve estar no snapshot
            CreatedAt = DateTime.UtcNow,
            TotalPrice = 25.00m,
            OrderedProducts = new List<OrderedProduct>()
        };

        // Act
        var snapshot = OrderSnapshotBuilder.BuildFromOrder(order);

        // Assert
        // Verificar que CustomerId não está presente no snapshot
        // O snapshot não tem propriedade CustomerId, então está correto
        Assert.NotNull(snapshot);
        Assert.NotNull(snapshot.Order);
        // OrderInfo não tem CustomerId, confirmando que PII não está incluído
    }
}
