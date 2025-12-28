namespace FastFood.OrderHub.Application.DTOs;

/// <summary>
/// DTO para pedido
/// </summary>
public class OrderDto
{
    public Guid Id { get; set; }
    public string? Code { get; set; }
    public Guid? CustomerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public int OrderStatus { get; set; } // EnumOrderStatus
    public int PaymentStatus { get; set; } // EnumPaymentStatus
    public decimal TotalPrice { get; set; }
    public string? OrderSource { get; set; }
    public List<OrderedProductDto> Items { get; set; } = new();
}

