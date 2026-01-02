using FastFood.OrderHub.Domain.Common.Enums;
using FastFood.OrderHub.Domain.Entities.OrderManagement;
using Xunit;

namespace FastFood.OrderHub.Tests.Unit.Domain.Entities;

/// <summary>
/// Testes unitários para entidade Order
/// </summary>
public class OrderTests
{
    [Fact]
    public void Order_WhenCreated_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var order = new Order();

        // Assert
        Assert.Equal(Guid.Empty, order.Id);
        Assert.Null(order.Code);
        Assert.NotNull(order.OrderedProducts);
        Assert.Empty(order.OrderedProducts);
        Assert.Equal(default(EnumOrderStatus), order.OrderStatus);
        Assert.Equal(default(DateTime), order.CreatedAt);
        Assert.Null(order.CustomerId);
        Assert.Equal(0, order.TotalPrice);
    }

    [Fact]
    public void AddProduct_WhenProductAdded_ShouldAddToOrderedProducts()
    {
        // Arrange
        var order = new Order { Id = Guid.NewGuid() };
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
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

        // Act
        order.AddProduct(orderedProduct);

        // Assert
        Assert.Single(order.OrderedProducts);
        Assert.Equal(order.Id, orderedProduct.OrderId);
        Assert.True(order.TotalPrice > 0);
    }

    [Fact]
    public void AddProduct_WhenMultipleProductsAdded_ShouldCalculateTotalPrice()
    {
        // Arrange
        var order = new Order { Id = Guid.NewGuid() };
        var product1 = new Product { Id = Guid.NewGuid(), Price = 10.00m };
        var product2 = new Product { Id = Guid.NewGuid(), Price = 20.00m };
        
        var orderedProduct1 = new OrderedProduct
        {
            Id = Guid.NewGuid(),
            ProductId = product1.Id,
            Product = product1,
            Quantity = 2
        };
        orderedProduct1.CalculateFinalPrice();

        var orderedProduct2 = new OrderedProduct
        {
            Id = Guid.NewGuid(),
            ProductId = product2.Id,
            Product = product2,
            Quantity = 1
        };
        orderedProduct2.CalculateFinalPrice();

        // Act
        order.AddProduct(orderedProduct1);
        order.AddProduct(orderedProduct2);

        // Assert
        Assert.Equal(2, order.OrderedProducts.Count);
        Assert.Equal(orderedProduct1.FinalPrice + orderedProduct2.FinalPrice, order.TotalPrice);
    }

    [Fact]
    public void RemoveProduct_WhenProductExists_ShouldRemoveFromOrderedProducts()
    {
        // Arrange
        var order = new Order { Id = Guid.NewGuid() };
        var product = new Product { Id = Guid.NewGuid(), Price = 10.00m };
        var orderedProduct = new OrderedProduct
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Product = product,
            Quantity = 2
        };
        orderedProduct.CalculateFinalPrice();
        order.AddProduct(orderedProduct);
        var initialTotal = order.TotalPrice;

        // Act
        order.RemoveProduct(orderedProduct.Id);

        // Assert
        Assert.Empty(order.OrderedProducts);
        Assert.Equal(0, order.TotalPrice);
    }

    [Fact]
    public void RemoveProduct_WhenProductDoesNotExist_ShouldNotChangeOrder()
    {
        // Arrange
        var order = new Order { Id = Guid.NewGuid() };
        var product = new Product { Id = Guid.NewGuid(), Price = 10.00m };
        var orderedProduct = new OrderedProduct
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Product = product,
            Quantity = 2
        };
        orderedProduct.CalculateFinalPrice();
        order.AddProduct(orderedProduct);
        var initialTotal = order.TotalPrice;

        // Act
        order.RemoveProduct(Guid.NewGuid()); // ID que não existe

        // Assert
        Assert.Single(order.OrderedProducts);
        Assert.Equal(initialTotal, order.TotalPrice);
    }

    [Fact]
    public void CalculateTotalPrice_ShouldReturnSumOfAllProducts()
    {
        // Arrange
        var order = new Order { Id = Guid.NewGuid() };
        var product1 = new Product { Id = Guid.NewGuid(), Price = 10.00m };
        var product2 = new Product { Id = Guid.NewGuid(), Price = 20.00m };
        
        var orderedProduct1 = new OrderedProduct
        {
            Id = Guid.NewGuid(),
            ProductId = product1.Id,
            Product = product1,
            Quantity = 2
        };
        orderedProduct1.CalculateFinalPrice();

        var orderedProduct2 = new OrderedProduct
        {
            Id = Guid.NewGuid(),
            ProductId = product2.Id,
            Product = product2,
            Quantity = 1
        };
        orderedProduct2.CalculateFinalPrice();

        order.OrderedProducts.Add(orderedProduct1);
        order.OrderedProducts.Add(orderedProduct2);

        // Act
        var totalPrice = order.CalculateTotalPrice();

        // Assert
        Assert.Equal(orderedProduct1.FinalPrice + orderedProduct2.FinalPrice, totalPrice);
        Assert.Equal(totalPrice, order.TotalPrice);
    }

    [Fact]
    public void FinalizeOrderSelection_ShouldChangeStatusToAwaitingPayment()
    {
        // Arrange
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderStatus = EnumOrderStatus.Started
        };

        // Act
        order.FinalizeOrderSelection();

        // Assert
        Assert.Equal(EnumOrderStatus.AwaitingPayment, order.OrderStatus);
    }

    [Fact]
    public void UpdateStatus_ShouldChangeStatus()
    {
        // Arrange
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderStatus = EnumOrderStatus.Started
        };

        // Act
        order.UpdateStatus(EnumOrderStatus.InPreparation);

        // Assert
        Assert.Equal(EnumOrderStatus.InPreparation, order.OrderStatus);
    }

    [Fact]
    public void SendToKitchen_ShouldChangeStatusToInPreparation()
    {
        // Arrange
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderStatus = EnumOrderStatus.AwaitingPayment
        };

        // Act
        order.SendToKitchen();

        // Assert
        Assert.Equal(EnumOrderStatus.InPreparation, order.OrderStatus);
    }

    [Fact]
    public void SetInPreparation_ShouldChangeStatusToInPreparation()
    {
        // Arrange
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderStatus = EnumOrderStatus.Started
        };

        // Act
        order.SetInPreparation();

        // Assert
        Assert.Equal(EnumOrderStatus.InPreparation, order.OrderStatus);
    }

    [Fact]
    public void SetReadyForPickup_ShouldChangeStatusToReadyForPickup()
    {
        // Arrange
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderStatus = EnumOrderStatus.InPreparation
        };

        // Act
        order.SetReadyForPickup();

        // Assert
        Assert.Equal(EnumOrderStatus.ReadyForPickup, order.OrderStatus);
    }
}
