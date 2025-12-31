namespace FastFood.OrderHub.Application.Responses.ProductManagement;

/// <summary>
/// ResponseModel para produto atualizado
/// </summary>
public class UpdateProductResponse
{
    public Guid ProductId { get; set; }
    public string? Name { get; set; }
    public int Category { get; set; }
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ProductBaseIngredientResponse> BaseIngredients { get; set; } = new();
}


