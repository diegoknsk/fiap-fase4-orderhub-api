namespace FastFood.OrderHub.Application.OutputModels.OrderManagement;

/// <summary>
/// OutputModel para pedido iniciado
/// </summary>
public class StartOrderOutputModel
{
    public Guid OrderId { get; set; }
    public string? Code { get; set; }
    public Guid? CustomerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public int OrderStatus { get; set; }
    public decimal TotalPrice { get; set; }
}

