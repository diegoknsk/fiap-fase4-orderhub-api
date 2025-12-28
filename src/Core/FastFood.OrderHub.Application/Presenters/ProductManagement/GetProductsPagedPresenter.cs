using FastFood.OrderHub.Application.OutputModels.ProductManagement;
using FastFood.OrderHub.Application.Responses.ProductManagement;

namespace FastFood.OrderHub.Application.Presenters.ProductManagement;

/// <summary>
/// Presenter para GetProductsPaged
/// </summary>
public class GetProductsPagedPresenter
{
    public GetProductsPagedResponse Present(GetProductsPagedOutputModel output)
    {
        return new GetProductsPagedResponse
        {
            Items = output.Items.Select(item => new ProductSummaryResponse
            {
                ProductId = item.ProductId,
                Name = item.Name,
                Category = item.Category,
                Price = item.Price,
                Description = item.Description,
                ImageUrl = item.ImageUrl,
                IsActive = item.IsActive,
                CreatedAt = item.CreatedAt
            }).ToList(),
            Page = output.Page,
            PageSize = output.PageSize,
            TotalCount = output.TotalCount,
            TotalPages = output.TotalPages,
            HasPreviousPage = output.HasPreviousPage,
            HasNextPage = output.HasNextPage
        };
    }
}

