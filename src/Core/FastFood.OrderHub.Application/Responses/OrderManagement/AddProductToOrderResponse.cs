namespace FastFood.OrderHub.Application.Responses.OrderManagement;

/// <summary>
/// ResponseModel para produto adicionado ao pedido
/// </summary>
public class AddProductToOrderResponse
{
    public Guid OrderId { get; set; }
    public Guid OrderedProductId { get; set; }
    public decimal TotalPrice { get; set; }
}



