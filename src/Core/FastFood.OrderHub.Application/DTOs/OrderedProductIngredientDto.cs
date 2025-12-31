namespace FastFood.OrderHub.Application.DTOs;

/// <summary>
/// DTO para ingrediente customizado de produto pedido
/// </summary>
public class OrderedProductIngredientDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public Guid? OrderedProductId { get; set; }
    public Guid? ProductBaseIngredientId { get; set; }
}



