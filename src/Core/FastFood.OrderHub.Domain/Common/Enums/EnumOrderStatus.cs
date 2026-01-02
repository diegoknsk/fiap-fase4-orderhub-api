namespace FastFood.OrderHub.Domain.Common.Enums;

/// <summary>
/// Status do pedido
/// </summary>
public enum EnumOrderStatus
{
    Started = 1,
    AwaitingPayment = 2,
    PaymentConfirmed = 3,
    InPreparation = 4,
    ReadyForPickup = 5,
    Completed = 6,
    Cancelled = 7
}



