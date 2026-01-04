namespace FastFood.OrderHub.Application.DTOs.Payment;

/// <summary>
/// Snapshot híbrido do pedido para envio ao PayStream (sem PII)
/// </summary>
public class OrderSnapshot
{
    public OrderInfo Order { get; set; } = null!;
    public PricingInfo Pricing { get; set; } = null!;
    public List<ItemInfo> Items { get; set; } = new();
    public int Version { get; set; }
}

/// <summary>
/// Informações do pedido no snapshot
/// </summary>
public class OrderInfo
{
    public Guid OrderId { get; set; }
    public string Code { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Informações de preço no snapshot
/// </summary>
public class PricingInfo
{
    public decimal TotalPrice { get; set; }
    public string Currency { get; set; } = "BRL";
}

/// <summary>
/// Informações do item no snapshot
/// </summary>
public class ItemInfo
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal FinalPrice { get; set; }
    public string? Observation { get; set; }
    public List<CustomIngredientInfo> CustomIngredients { get; set; } = new();
}

/// <summary>
/// Informações de ingrediente customizado no snapshot
/// </summary>
public class CustomIngredientInfo
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
