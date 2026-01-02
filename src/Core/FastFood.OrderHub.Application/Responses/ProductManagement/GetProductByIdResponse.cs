namespace FastFood.OrderHub.Application.Responses.ProductManagement;

/// <summary>
/// ResponseModel para produto obtido por ID
/// </summary>
public class GetProductByIdResponse
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

/// <summary>
/// ResponseModel para ingrediente base
/// </summary>
public class ProductBaseIngredientResponse
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public decimal Price { get; set; }
}



