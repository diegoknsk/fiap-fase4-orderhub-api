namespace FastFood.OrderHub.Application.DTOs;

/// <summary>
/// DTO para produto
/// </summary>
public class ProductDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public int Category { get; set; } // EnumProductCategory
    public List<ProductBaseIngredientDto> BaseIngredients { get; set; } = new();
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}



