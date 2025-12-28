namespace FastFood.OrderHub.Application.Responses.OrderManagement;

/// <summary>
/// ResponseModel para pedido obtido por ID
/// </summary>
public class GetOrderByIdResponse
{
    public Guid OrderId { get; set; }
    public string? Code { get; set; }
    public Guid? CustomerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public int OrderStatus { get; set; }
    public int PaymentStatus { get; set; }
    public decimal TotalPrice { get; set; }
    public List<OrderedProductResponse> Items { get; set; } = new();
}

/// <summary>
/// ResponseModel para produto pedido
/// </summary>
public class OrderedProductResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public int? Category { get; set; }
    public int Quantity { get; set; }
    public decimal FinalPrice { get; set; }
    public string? Observation { get; set; }
    public List<OrderedProductIngredientResponse> CustomIngredients { get; set; } = new();
}

/// <summary>
/// ResponseModel para ingrediente customizado
/// </summary>
public class OrderedProductIngredientResponse
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

