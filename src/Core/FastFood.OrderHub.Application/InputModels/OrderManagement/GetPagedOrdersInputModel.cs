namespace FastFood.OrderHub.Application.InputModels.OrderManagement;

/// <summary>
/// InputModel para listar pedidos paginados
/// </summary>
public class GetPagedOrdersInputModel
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int? Status { get; set; }
}



