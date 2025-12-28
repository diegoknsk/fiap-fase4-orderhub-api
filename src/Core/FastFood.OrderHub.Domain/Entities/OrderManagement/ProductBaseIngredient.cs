namespace FastFood.OrderHub.Domain.Entities.OrderManagement;

/// <summary>
/// Ingrediente base de um produto
/// </summary>
public class ProductBaseIngredient
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public decimal Price { get; set; }
    public Guid ProductId { get; set; }
}

