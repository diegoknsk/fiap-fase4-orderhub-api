namespace FastFood.OrderHub.Application.Responses.ProductManagement;

/// <summary>
/// ResponseModel para lista paginada de produtos
/// </summary>
public class GetProductsPagedResponse
{
    public List<ProductSummaryResponse> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
}

/// <summary>
/// ResponseModel resumido para produto
/// </summary>
public class ProductSummaryResponse
{
    public Guid ProductId { get; set; }
    public string? Name { get; set; }
    public int Category { get; set; }
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}



