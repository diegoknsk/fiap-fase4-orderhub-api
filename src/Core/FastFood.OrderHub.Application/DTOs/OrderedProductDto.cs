namespace FastFood.OrderHub.Application.DTOs;

/// <summary>
/// DTO para produto pedido
/// </summary>
public class OrderedProductDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid? OrderId { get; set; }
    public List<OrderedProductIngredientDto> CustomIngredients { get; set; } = new();
    public string? Observation { get; set; }
    public decimal FinalPrice { get; set; }
    public int Quantity { get; set; }
    
    // Snapshot do produto no momento do pedido (para DynamoDB)
    public string? ProductName { get; set; }
    public int? Category { get; set; }
}



