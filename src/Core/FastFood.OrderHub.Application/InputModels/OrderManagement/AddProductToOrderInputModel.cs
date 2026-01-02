namespace FastFood.OrderHub.Application.InputModels.OrderManagement;

/// <summary>
/// InputModel para adicionar produto ao pedido
/// </summary>
public class AddProductToOrderInputModel
{
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public string? Observation { get; set; }
    public List<CustomIngredientInputModel> CustomIngredients { get; set; } = new();
}

/// <summary>
/// InputModel para ingrediente customizado
/// </summary>
public class CustomIngredientInputModel
{
    public Guid ProductBaseIngredientId { get; set; }
    public int Quantity { get; set; }
}



