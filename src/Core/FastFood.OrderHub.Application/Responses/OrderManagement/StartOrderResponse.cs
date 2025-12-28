namespace FastFood.OrderHub.Application.Responses.OrderManagement;

/// <summary>
/// ResponseModel para pedido iniciado
/// </summary>
public class StartOrderResponse
{
    public Guid OrderId { get; set; }
    public string? Code { get; set; }
    public Guid? CustomerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public int OrderStatus { get; set; }
    public decimal TotalPrice { get; set; }
}

