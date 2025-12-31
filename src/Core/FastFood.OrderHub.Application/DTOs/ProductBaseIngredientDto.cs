namespace FastFood.OrderHub.Application.DTOs;

/// <summary>
/// DTO para ingrediente base de produto
/// </summary>
public class ProductBaseIngredientDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public decimal Price { get; set; }
    public Guid ProductId { get; set; }
}


