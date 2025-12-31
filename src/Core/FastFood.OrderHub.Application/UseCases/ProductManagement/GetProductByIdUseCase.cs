using FastFood.OrderHub.Application.InputModels.ProductManagement;
using FastFood.OrderHub.Application.OutputModels.ProductManagement;
using FastFood.OrderHub.Application.Ports;
using FastFood.OrderHub.Application.Presenters.ProductManagement;
using FastFood.OrderHub.Application.Responses.ProductManagement;

namespace FastFood.OrderHub.Application.UseCases.ProductManagement;

/// <summary>
/// UseCase para obter produto por ID
/// </summary>
public class GetProductByIdUseCase
{
    private readonly IProductDataSource _productDataSource;
    private readonly GetProductByIdPresenter _presenter;

    public GetProductByIdUseCase(
        IProductDataSource productDataSource,
        GetProductByIdPresenter presenter)
    {
        _productDataSource = productDataSource;
        _presenter = presenter;
    }

    public async Task<GetProductByIdResponse?> ExecuteAsync(GetProductByIdInputModel input)
    {
        var productDto = await _productDataSource.GetByIdAsync(input.ProductId);

        if (productDto == null)
            return null;

        var output = new GetProductByIdOutputModel
        {
            ProductId = productDto.Id,
            Name = productDto.Name,
            Category = productDto.Category,
            Price = productDto.Price,
            Description = productDto.Description,
            ImageUrl = productDto.ImageUrl,
            IsActive = productDto.IsActive,
            CreatedAt = productDto.CreatedAt,
            BaseIngredients = productDto.BaseIngredients.Select(bi => new ProductBaseIngredientOutputModel
            {
                Id = bi.Id,
                Name = bi.Name,
                Price = bi.Price
            }).ToList()
        };

        return _presenter.Present(output);
    }
}



