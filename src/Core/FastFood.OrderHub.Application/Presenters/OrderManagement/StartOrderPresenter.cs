using FastFood.OrderHub.Application.OutputModels.OrderManagement;
using FastFood.OrderHub.Application.Responses.OrderManagement;

namespace FastFood.OrderHub.Application.Presenters.OrderManagement;

/// <summary>
/// Presenter para StartOrder
/// </summary>
public class StartOrderPresenter
{
    public StartOrderResponse Present(StartOrderOutputModel output)
    {
        return new StartOrderResponse
        {
            OrderId = output.OrderId,
            Code = output.Code,
            CustomerId = output.CustomerId,
            CreatedAt = output.CreatedAt,
            OrderStatus = output.OrderStatus,
            TotalPrice = output.TotalPrice
        };
    }
}

