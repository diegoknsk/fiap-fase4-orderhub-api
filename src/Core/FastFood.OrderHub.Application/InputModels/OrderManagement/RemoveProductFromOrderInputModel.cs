namespace FastFood.OrderHub.Application.InputModels.OrderManagement;

/// <summary>
/// InputModel para remover produto do pedido
/// </summary>
public class RemoveProductFromOrderInputModel
{
    public Guid OrderId { get; set; }
    public Guid OrderedProductId { get; set; }
}



