namespace FastFood.OrderHub.Application.Responses.ProductManagement;

/// <summary>
/// ResponseModel para produto deletado
/// </summary>
public class DeleteProductResponse
{
    public Guid ProductId { get; set; }
    public bool Success { get; set; }
}


