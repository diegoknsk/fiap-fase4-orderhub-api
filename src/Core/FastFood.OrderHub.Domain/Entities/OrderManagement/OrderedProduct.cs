namespace FastFood.OrderHub.Domain.Entities.OrderManagement;

/// <summary>
/// Entidade de domínio: Produto pedido
/// </summary>
public class OrderedProduct
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public Guid? OrderId { get; set; }
    public ICollection<OrderedProductIngredient> CustomIngredients { get; set; } = new List<OrderedProductIngredient>();
    public string? Observation { get; set; }
    public decimal FinalPrice { get; set; }
    public int Quantity { get; set; }

    /// <summary>
    /// Calcula o preço final do produto pedido (basePrice + customizações) * quantity
    /// </summary>
    public decimal CalculateFinalPrice()
    {
        var basePrice = Product?.Price ?? 0;
        var customizationsPrice = CustomIngredients.Sum(ci => ci.Price * ci.Quantity);
        FinalPrice = (basePrice + customizationsPrice) * Quantity;
        return FinalPrice;
    }

    /// <summary>
    /// Atualiza a quantidade e recalcula o preço
    /// </summary>
    public void SetQuantity(int quantity)
    {
        Quantity = quantity;
        CalculateFinalPrice();
    }

    /// <summary>
    /// Atualiza a quantidade de um ingrediente
    /// </summary>
    public void SetIngredientQuantity(Guid ingredientId, int quantity)
    {
        var ingredient = CustomIngredients.FirstOrDefault(ci => ci.Id == ingredientId);
        if (ingredient != null)
        {
            ingredient.Quantity = Math.Clamp(quantity, 0, 10);
            CalculateFinalPrice();
        }
    }

    /// <summary>
    /// Atualiza a observação
    /// </summary>
    public void SetObservation(string? observation)
    {
        Observation = observation;
    }
}

