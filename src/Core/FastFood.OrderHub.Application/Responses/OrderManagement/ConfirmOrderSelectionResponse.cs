namespace FastFood.OrderHub.Application.Responses.OrderManagement;

/// <summary>
/// ResponseModel para seleção do pedido confirmada
/// </summary>
public class ConfirmOrderSelectionResponse
{
    public Guid OrderId { get; set; }
    public int OrderStatus { get; set; }
    public decimal TotalPrice { get; set; }
}


