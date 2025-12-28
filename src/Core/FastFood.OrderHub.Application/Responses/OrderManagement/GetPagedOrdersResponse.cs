namespace FastFood.OrderHub.Application.Responses.OrderManagement;

/// <summary>
/// ResponseModel para lista paginada de pedidos
/// </summary>
public class GetPagedOrdersResponse
{
    public List<OrderSummaryResponse> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
}

/// <summary>
/// ResponseModel para resumo de pedido
/// </summary>
public class OrderSummaryResponse
{
    public Guid OrderId { get; set; }
    public string? Code { get; set; }
    public Guid? CustomerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public int OrderStatus { get; set; }
    public decimal TotalPrice { get; set; }
}

