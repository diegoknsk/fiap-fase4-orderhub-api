using FastFood.OrderHub.Application.OutputModels.OrderManagement;
using FastFood.OrderHub.Application.Responses.OrderManagement;

namespace FastFood.OrderHub.Application.Presenters.OrderManagement;

/// <summary>
/// Presenter para GetPagedOrders
/// </summary>
public class GetPagedOrdersPresenter
{
    public GetPagedOrdersResponse Present(GetPagedOrdersOutputModel output)
    {
        return new GetPagedOrdersResponse
        {
            Items = output.Items.Select(item => new OrderSummaryResponse
            {
                OrderId = item.OrderId,
                Code = item.Code,
                CustomerId = item.CustomerId,
                CreatedAt = item.CreatedAt,
                OrderStatus = item.OrderStatus,
                TotalPrice = item.TotalPrice
            }).ToList(),
            Page = output.Page,
            PageSize = output.PageSize,
            TotalCount = output.TotalCount,
            TotalPages = output.TotalPages,
            HasNextPage = output.HasNextPage
        };
    }
}

