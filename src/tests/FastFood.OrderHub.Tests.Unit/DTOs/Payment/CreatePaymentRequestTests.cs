using FastFood.OrderHub.Application.DTOs.Payment;
using System.Text.Json;
using Xunit;

namespace FastFood.OrderHub.Tests.Unit.DTOs.Payment;

/// <summary>
/// Testes unitários para CreatePaymentRequest
/// </summary>
public class CreatePaymentRequestTests
{
    [Fact]
    public void ToPayStreamRequest_ShouldIncludeOrderIdAndTotalAmount()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var totalAmount = 75.50m;
        var request = new CreatePaymentRequest
        {
            OrderId = orderId,
            TotalAmount = totalAmount,
            OrderSnapshot = new OrderSnapshot
            {
                Order = new OrderInfo { OrderId = orderId, Code = "ORD-001", CreatedAt = DateTime.UtcNow },
                Pricing = new PricingInfo { TotalPrice = totalAmount, Currency = "BRL" },
                Items = new List<ItemInfo>(),
                Version = 1
            }
        };

        // Act
        var payStreamRequest = request.ToPayStreamRequest();

        // Assert
        var json = JsonSerializer.Serialize(payStreamRequest, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        var jsonDoc = JsonDocument.Parse(json);
        var root = jsonDoc.RootElement;
        
        Assert.Equal(orderId, root.GetProperty("orderId").GetGuid());
        Assert.Equal(totalAmount, root.GetProperty("totalAmount").GetDecimal());
    }
}
