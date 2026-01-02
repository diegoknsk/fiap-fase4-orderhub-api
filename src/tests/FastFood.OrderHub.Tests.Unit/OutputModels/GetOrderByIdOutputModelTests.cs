using FastFood.OrderHub.Application.OutputModels.OrderManagement;
using FastFood.OrderHub.Domain.Common.Enums;
using Xunit;

namespace FastFood.OrderHub.Tests.Unit.OutputModels;

/// <summary>
/// Testes unitários para GetOrderByIdOutputModel e OrderedProductOutputModel
/// </summary>
public class GetOrderByIdOutputModelTests
{
    [Fact]
    public void GetOrderByIdOutputModel_WhenCreated_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var output = new GetOrderByIdOutputModel();

        // Assert
        Assert.Equal(Guid.Empty, output.OrderId);
        Assert.Null(output.Code);
        Assert.Null(output.CustomerId);
        Assert.Equal(default(DateTime), output.CreatedAt);
        Assert.Equal(0, output.OrderStatus);
        Assert.Equal(0, output.TotalPrice);
        Assert.NotNull(output.Items);
        Assert.Empty(output.Items);
    }

    [Fact]
    public void GetOrderByIdOutputModel_WhenPropertiesSet_ShouldStoreValues()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var code = "ORD-001";
        var createdAt = DateTime.UtcNow;
        var status = (int)EnumOrderStatus.Started;
        var totalPrice = 50.00m;

        // Act
        var output = new GetOrderByIdOutputModel
        {
            OrderId = orderId,
            Code = code,
            CustomerId = customerId,
            CreatedAt = createdAt,
            OrderStatus = status,
            TotalPrice = totalPrice,
            Items = new List<OrderedProductOutputModel>()
        };

        // Assert
        Assert.Equal(orderId, output.OrderId);
        Assert.Equal(code, output.Code);
        Assert.Equal(customerId, output.CustomerId);
        Assert.Equal(createdAt, output.CreatedAt);
        Assert.Equal(status, output.OrderStatus);
        Assert.Equal(totalPrice, output.TotalPrice);
    }

    [Fact]
    public void OrderedProductOutputModel_WhenCreated_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var output = new OrderedProductOutputModel();

        // Assert
        Assert.Equal(Guid.Empty, output.Id);
        Assert.Equal(Guid.Empty, output.ProductId);
        Assert.Null(output.ProductName);
        Assert.Null(output.Category);
        Assert.Equal(0, output.Quantity);
        Assert.Equal(0, output.FinalPrice);
        Assert.Null(output.Observation);
        Assert.NotNull(output.CustomIngredients);
        Assert.Empty(output.CustomIngredients);
    }

    [Fact]
    public void OrderedProductIngredientOutputModel_WhenCreated_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var output = new OrderedProductIngredientOutputModel();

        // Assert
        Assert.Equal(Guid.Empty, output.Id);
        Assert.Null(output.Name);
        Assert.Equal(0, output.Price);
        Assert.Equal(0, output.Quantity);
    }
}
