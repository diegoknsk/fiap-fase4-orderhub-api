namespace FastFood.OrderHub.Application.InputModels.ProductManagement;

/// <summary>
/// InputModel para criar produto
/// </summary>
public class CreateProductInputModel
{
    public string? Name { get; set; }
    public int Category { get; set; }
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public List<CreateProductBaseIngredientInputModel> BaseIngredients { get; set; } = new();
}

/// <summary>
/// InputModel para ingrediente base
/// </summary>
public class CreateProductBaseIngredientInputModel
{
    public string? Name { get; set; }
    public decimal Price { get; set; }
}


