namespace FastFood.OrderHub.Application.OutputModels.ProductManagement;

/// <summary>
/// OutputModel para produto deletado
/// </summary>
public class DeleteProductOutputModel
{
    public Guid ProductId { get; set; }
    public bool Success { get; set; }
}



