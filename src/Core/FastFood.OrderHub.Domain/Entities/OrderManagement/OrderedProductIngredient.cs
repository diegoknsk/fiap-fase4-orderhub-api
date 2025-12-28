namespace FastFood.OrderHub.Domain.Entities.OrderManagement;

/// <summary>
/// Ingrediente customizado de um produto pedido (snapshot no momento do pedido)
/// </summary>
public class OrderedProductIngredient
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public decimal Price { get; set; } // Snapshot do preço no momento do pedido
    public int Quantity { get; set; } // 0 a 10
    public Guid? OrderedProductId { get; set; }
    public Guid? ProductBaseIngredientId { get; set; }
}

