using FastFood.OrderHub.Application.OutputModels.OrderManagement;
using FastFood.OrderHub.Application.Responses.OrderManagement;

namespace FastFood.OrderHub.Application.Presenters.OrderManagement;

/// <summary>
/// Presenter para ConfirmOrderSelection
/// </summary>
public class ConfirmOrderSelectionPresenter
{
    public ConfirmOrderSelectionResponse Present(ConfirmOrderSelectionOutputModel output)
    {
        return new ConfirmOrderSelectionResponse
        {
            OrderId = output.OrderId,
            OrderStatus = output.OrderStatus,
            TotalPrice = output.TotalPrice
        };
    }
}

