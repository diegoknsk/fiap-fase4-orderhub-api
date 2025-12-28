using FastFood.OrderHub.Application.OutputModels.OrderManagement;
using FastFood.OrderHub.Application.Responses.OrderManagement;

namespace FastFood.OrderHub.Application.Presenters.OrderManagement;

/// <summary>
/// Presenter para AddProductToOrder
/// </summary>
public class AddProductToOrderPresenter
{
    public AddProductToOrderResponse Present(AddProductToOrderOutputModel output)
    {
        return new AddProductToOrderResponse
        {
            OrderId = output.OrderId,
            OrderedProductId = output.OrderedProductId,
            TotalPrice = output.TotalPrice
        };
    }
}

