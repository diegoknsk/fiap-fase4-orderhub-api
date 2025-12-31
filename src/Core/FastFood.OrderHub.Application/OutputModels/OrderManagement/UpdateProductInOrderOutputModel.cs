namespace FastFood.OrderHub.Application.OutputModels.OrderManagement;

/// <summary>
/// OutputModel para produto atualizado no pedido
/// </summary>
public class UpdateProductInOrderOutputModel
{
    public Guid OrderId { get; set; }
    public Guid OrderedProductId { get; set; }
    public decimal TotalPrice { get; set; }
}


