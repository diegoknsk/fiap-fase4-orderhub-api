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
    public void ToPayStreamRequest_ShouldSerializeOrderSnapshotAsString()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var request = new CreatePaymentRequest
        {
            OrderId = orderId,
            TotalAmount = 50.00m,
            OrderSnapshot = new OrderSnapshot
            {
                Order = new OrderInfo
                {
                    OrderId = orderId,
                    Code = "ORD-001",
                    CreatedAt = DateTime.UtcNow
                },
                Pricing = new PricingInfo
                {
                    TotalPrice = 50.00m,
                    Currency = "BRL"
                },
                Items = new List<ItemInfo>
                {
                    new ItemInfo
                    {
                        ProductId = Guid.NewGuid(),
                        ProductName = "Hambúrguer",
                        Quantity = 2,
                        FinalPrice = 25.00m
                    }
                },
                Version = 1
            }
        };

        // Act
        var payStreamRequest = request.ToPayStreamRequest();

        // Assert
        Assert.NotNull(payStreamRequest);
        
        // Serializar para JSON para verificar estrutura
        var json = JsonSerializer.Serialize(payStreamRequest, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        var jsonDoc = JsonDocument.Parse(json);
        var root = jsonDoc.RootElement;
        
        // Verificar que orderSnapshot é uma string JSON
        Assert.True(root.TryGetProperty("orderSnapshot", out var orderSnapshotElement));
        Assert.Equal(JsonValueKind.String, orderSnapshotElement.ValueKind);
        
        // Verificar que a string JSON pode ser deserializada
        var snapshotString = orderSnapshotElement.GetString();
        Assert.NotNull(snapshotString);
        var deserializedSnapshot = JsonSerializer.Deserialize<OrderSnapshot>(snapshotString!);
        Assert.NotNull(deserializedSnapshot);
        Assert.Equal(orderId, deserializedSnapshot.Order.OrderId);
    }

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
