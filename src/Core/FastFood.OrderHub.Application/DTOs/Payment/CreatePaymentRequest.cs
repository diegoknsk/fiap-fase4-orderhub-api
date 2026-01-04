using System.Text.Json;

namespace FastFood.OrderHub.Application.DTOs.Payment;

/// <summary>
/// Request para criar pagamento no PayStream
/// </summary>
public class CreatePaymentRequest
{
    public Guid OrderId { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderSnapshot OrderSnapshot { get; set; } = null!;

    /// <summary>
    /// Converte o request para o formato esperado pelo PayStream (com orderSnapshot como string JSON)
    /// </summary>
    public object ToPayStreamRequest()
    {
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        return new
        {
            orderId = OrderId,
            totalAmount = TotalAmount,
            orderSnapshot = JsonSerializer.Serialize(OrderSnapshot, jsonOptions)
        };
    }
}
