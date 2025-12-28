namespace FastFood.OrderHub.Application.InputModels.OrderManagement;

/// <summary>
/// InputModel para atualizar produto no pedido
/// </summary>
public class UpdateProductInOrderInputModel
{
    public Guid OrderId { get; set; }
    public Guid OrderedProductId { get; set; }
    public int Quantity { get; set; }
    public string? Observation { get; set; }
    public List<CustomIngredientInputModel> CustomIngredients { get; set; } = new();
}

