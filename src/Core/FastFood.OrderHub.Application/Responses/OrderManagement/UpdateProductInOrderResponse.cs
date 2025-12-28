namespace FastFood.OrderHub.Application.Responses.OrderManagement;

/// <summary>
/// ResponseModel para produto atualizado no pedido
/// </summary>
public class UpdateProductInOrderResponse
{
    public Guid OrderId { get; set; }
    public Guid OrderedProductId { get; set; }
    public decimal TotalPrice { get; set; }
}

