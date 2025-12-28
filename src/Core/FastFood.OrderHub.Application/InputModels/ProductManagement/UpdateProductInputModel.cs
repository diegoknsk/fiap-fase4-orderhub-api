namespace FastFood.OrderHub.Application.InputModels.ProductManagement;

/// <summary>
/// InputModel para atualizar produto
/// </summary>
public class UpdateProductInputModel
{
    public Guid ProductId { get; set; }
    public string? Name { get; set; }
    public int Category { get; set; }
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public List<UpdateProductBaseIngredientInputModel> BaseIngredients { get; set; } = new();
}

/// <summary>
/// InputModel para ingrediente base na atualização
/// </summary>
public class UpdateProductBaseIngredientInputModel
{
    public Guid? Id { get; set; }
    public string? Name { get; set; }
    public decimal Price { get; set; }
}

