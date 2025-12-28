namespace FastFood.OrderHub.Application.OutputModels.OrderManagement;

/// <summary>
/// OutputModel para pedido obtido por ID
/// </summary>
public class GetOrderByIdOutputModel
{
    public Guid OrderId { get; set; }
    public string? Code { get; set; }
    public Guid? CustomerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public int OrderStatus { get; set; }
    public int PaymentStatus { get; set; }
    public decimal TotalPrice { get; set; }
    public List<OrderedProductOutputModel> Items { get; set; } = new();
}

/// <summary>
/// OutputModel para produto pedido
/// </summary>
public class OrderedProductOutputModel
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public int? Category { get; set; }
    public int Quantity { get; set; }
    public decimal FinalPrice { get; set; }
    public string? Observation { get; set; }
    public List<OrderedProductIngredientOutputModel> CustomIngredients { get; set; } = new();
}

/// <summary>
/// OutputModel para ingrediente customizado
/// </summary>
public class OrderedProductIngredientOutputModel
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

