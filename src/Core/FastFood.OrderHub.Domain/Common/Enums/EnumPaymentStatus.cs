namespace FastFood.OrderHub.Domain.Common.Enums;

/// <summary>
/// Status do pagamento
/// </summary>
public enum EnumPaymentStatus
{
    NotStarted = 1,
    AwaitingPayment = 2,
    Paid = 3,
    Failed = 4,
    Refunded = 5
}

