using FastFood.OrderHub.Domain.Common.Enums;

namespace FastFood.OrderHub.Domain.Entities.OrderManagement;

/// <summary>
/// Entidade de domínio: Produto
/// </summary>
public class Product
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public EnumProductCategory Category { get; set; }
    public ICollection<ProductBaseIngredient> Ingredients { get; set; } = new List<ProductBaseIngredient>();
    public decimal Price { get; set; }
    public ImageProduct? Image { get; set; }
    public string? Description { get; set; }

    /// <summary>
    /// Valida se o produto é válido
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Name) && Price > 0;
    }
}

