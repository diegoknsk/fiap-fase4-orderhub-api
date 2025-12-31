using FastFood.OrderHub.Application.OutputModels.ProductManagement;
using FastFood.OrderHub.Application.Responses.ProductManagement;

namespace FastFood.OrderHub.Application.Presenters.ProductManagement;

/// <summary>
/// Presenter para GetProductById
/// </summary>
public class GetProductByIdPresenter
{
    public GetProductByIdResponse Present(GetProductByIdOutputModel output)
    {
        return new GetProductByIdResponse
        {
            ProductId = output.ProductId,
            Name = output.Name,
            Category = output.Category,
            Price = output.Price,
            Description = output.Description,
            ImageUrl = output.ImageUrl,
            IsActive = output.IsActive,
            CreatedAt = output.CreatedAt,
            BaseIngredients = output.BaseIngredients.Select(bi => new ProductBaseIngredientResponse
            {
                Id = bi.Id,
                Name = bi.Name,
                Price = bi.Price
            }).ToList()
        };
    }
}


