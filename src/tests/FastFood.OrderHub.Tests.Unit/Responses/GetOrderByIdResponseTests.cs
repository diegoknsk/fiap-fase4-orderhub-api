using FastFood.OrderHub.Application.Responses.OrderManagement;
using FastFood.OrderHub.Domain.Common.Enums;
using Xunit;

namespace FastFood.OrderHub.Tests.Unit.Responses;

/// <summary>
/// Testes unitários para GetOrderByIdResponse e OrderedProductResponse
/// </summary>
public class GetOrderByIdResponseTests
{
    [Fact]
    public void GetOrderByIdResponse_WhenCreated_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var response = new GetOrderByIdResponse();

        // Assert
        Assert.Equal(Guid.Empty, response.OrderId);
        Assert.Null(response.Code);
        Assert.Null(response.CustomerId);
        Assert.Equal(default(DateTime), response.CreatedAt);
        Assert.Equal(0, response.OrderStatus);
        Assert.Equal(0, response.TotalPrice);
        Assert.NotNull(response.Items);
        Assert.Empty(response.Items);
    }

    [Fact]
    public void GetOrderByIdResponse_WhenPropertiesSet_ShouldStoreValues()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var code = "ORD-001";
        var createdAt = DateTime.UtcNow;
        var status = (int)EnumOrderStatus.Started;
        var totalPrice = 50.00m;

        // Act
        var response = new GetOrderByIdResponse
        {
            OrderId = orderId,
            Code = code,
            CustomerId = customerId,
            CreatedAt = createdAt,
            OrderStatus = status,
            TotalPrice = totalPrice,
            Items = new List<OrderedProductResponse>()
        };

        // Assert
        Assert.Equal(orderId, response.OrderId);
        Assert.Equal(code, response.Code);
        Assert.Equal(customerId, response.CustomerId);
        Assert.Equal(createdAt, response.CreatedAt);
        Assert.Equal(status, response.OrderStatus);
        Assert.Equal(totalPrice, response.TotalPrice);
    }

    [Fact]
    public void OrderedProductResponse_WhenCreated_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var response = new OrderedProductResponse();

        // Assert
        Assert.Equal(Guid.Empty, response.Id);
        Assert.Equal(Guid.Empty, response.ProductId);
        Assert.Null(response.ProductName);
        Assert.Null(response.Category);
        Assert.Equal(0, response.Quantity);
        Assert.Equal(0, response.FinalPrice);
        Assert.Null(response.Observation);
        Assert.NotNull(response.CustomIngredients);
        Assert.Empty(response.CustomIngredients);
    }

    [Fact]
    public void OrderedProductIngredientResponse_WhenCreated_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var response = new OrderedProductIngredientResponse();

        // Assert
        Assert.Equal(Guid.Empty, response.Id);
        Assert.Null(response.Name);
        Assert.Equal(0, response.Price);
        Assert.Equal(0, response.Quantity);
    }
}
