namespace FastFood.OrderHub.Application.OutputModels.OrderManagement;

/// <summary>
/// OutputModel para seleção do pedido confirmada
/// </summary>
public class ConfirmOrderSelectionOutputModel
{
    public Guid OrderId { get; set; }
    public int OrderStatus { get; set; }
    public decimal TotalPrice { get; set; }
}

