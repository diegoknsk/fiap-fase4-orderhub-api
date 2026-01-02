namespace FastFood.OrderHub.Application.OutputModels.OrderManagement;

/// <summary>
/// OutputModel para produto removido do pedido
/// </summary>
public class RemoveProductFromOrderOutputModel
{
    public Guid OrderId { get; set; }
    public Guid OrderedProductId { get; set; }
    public decimal TotalPrice { get; set; }
}



