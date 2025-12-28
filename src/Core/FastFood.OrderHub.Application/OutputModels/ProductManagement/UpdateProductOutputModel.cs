namespace FastFood.OrderHub.Application.OutputModels.ProductManagement;

/// <summary>
/// OutputModel para produto atualizado
/// </summary>
public class UpdateProductOutputModel
{
    public Guid ProductId { get; set; }
    public string? Name { get; set; }
    public int Category { get; set; }
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ProductBaseIngredientOutputModel> BaseIngredients { get; set; } = new();
}

