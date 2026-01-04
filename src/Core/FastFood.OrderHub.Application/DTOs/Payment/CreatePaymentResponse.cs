namespace FastFood.OrderHub.Application.DTOs.Payment;

/// <summary>
/// Response da criação de pagamento no PayStream
/// </summary>
public class CreatePaymentResponse
{
    public Guid PaymentId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
