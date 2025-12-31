using FastFood.OrderHub.Application.OutputModels.ProductManagement;
using FastFood.OrderHub.Application.Responses.ProductManagement;

namespace FastFood.OrderHub.Application.Presenters.ProductManagement;

/// <summary>
/// Presenter para CreateProduct
/// </summary>
public class CreateProductPresenter
{
    public CreateProductResponse Present(CreateProductOutputModel output)
    {
        return new CreateProductResponse
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


