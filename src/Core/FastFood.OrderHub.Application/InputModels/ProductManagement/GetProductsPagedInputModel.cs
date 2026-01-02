namespace FastFood.OrderHub.Application.InputModels.ProductManagement;

/// <summary>
/// InputModel para listar produtos paginados
/// </summary>
public class GetProductsPagedInputModel
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int? Category { get; set; }
    public string? Name { get; set; }
}



