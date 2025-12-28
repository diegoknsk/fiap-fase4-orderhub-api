namespace FastFood.OrderHub.Application.InputModels.OrderManagement;

/// <summary>
/// InputModel para obter pedido por ID
/// </summary>
public class GetOrderByIdInputModel
{
    public Guid OrderId { get; set; }
}

