namespace FastFood.OrderHub.Application.OutputModels.OrderManagement;

/// <summary>
/// OutputModel para produto adicionado ao pedido
/// </summary>
public class AddProductToOrderOutputModel
{
    public Guid OrderId { get; set; }
    public Guid OrderedProductId { get; set; }
    public decimal TotalPrice { get; set; }
}


