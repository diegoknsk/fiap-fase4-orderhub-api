using FastFood.OrderHub.Application.OutputModels.OrderManagement;
using FastFood.OrderHub.Application.Responses.OrderManagement;

namespace FastFood.OrderHub.Application.Presenters.OrderManagement;

/// <summary>
/// Presenter para UpdateProductInOrder
/// </summary>
public class UpdateProductInOrderPresenter
{
    public UpdateProductInOrderResponse Present(UpdateProductInOrderOutputModel output)
    {
        return new UpdateProductInOrderResponse
        {
            OrderId = output.OrderId,
            OrderedProductId = output.OrderedProductId,
            TotalPrice = output.TotalPrice
        };
    }
}


