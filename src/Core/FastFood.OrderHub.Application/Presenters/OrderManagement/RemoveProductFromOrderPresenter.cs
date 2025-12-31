using FastFood.OrderHub.Application.OutputModels.OrderManagement;
using FastFood.OrderHub.Application.Responses.OrderManagement;

namespace FastFood.OrderHub.Application.Presenters.OrderManagement;

/// <summary>
/// Presenter para RemoveProductFromOrder
/// </summary>
public class RemoveProductFromOrderPresenter
{
    public RemoveProductFromOrderResponse Present(RemoveProductFromOrderOutputModel output)
    {
        return new RemoveProductFromOrderResponse
        {
            OrderId = output.OrderId,
            OrderedProductId = output.OrderedProductId,
            TotalPrice = output.TotalPrice
        };
    }
}



