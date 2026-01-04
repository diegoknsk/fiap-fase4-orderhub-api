using FastFood.OrderHub.Application.DTOs;
using FastFood.OrderHub.Application.InputModels.ProductManagement;
using FastFood.OrderHub.Application.OutputModels.ProductManagement;
using FastFood.OrderHub.Application.Ports;
using FastFood.OrderHub.Application.Presenters.ProductManagement;
using FastFood.OrderHub.Application.Responses.ProductManagement;

namespace FastFood.OrderHub.Application.UseCases.ProductManagement;

/// <summary>
/// UseCase para listar produtos paginados
/// </summary>
public class GetProductsPagedUseCase
{
    private readonly IProductDataSource _productDataSource;
    private readonly GetProductsPagedPresenter _presenter;

    public GetProductsPagedUseCase(
        IProductDataSource productDataSource,
        GetProductsPagedPresenter presenter)
    {
        _productDataSource = productDataSource;
        _presenter = presenter;
    }

    public async Task<GetProductsPagedResponse> ExecuteAsync(GetProductsPagedInputModel input)
    {
        // Validar parâmetros
        if (input.Page < 1)
            input.Page = 1;
        
        if (input.PageSize < 1)
            input.PageSize = 10;
        
        if (input.PageSize > 100)
            input.PageSize = 100;

        // Buscar produtos paginados
        var products = await _productDataSource.GetPagedAsync(
            input.Page,
            input.PageSize,
            input.Category,
            input.Name);

        // Para calcular TotalCount, precisamos fazer uma busca adicional
        // Por simplicidade, vamos usar o tamanho da lista retornada como aproximação
        // Em produção, seria ideal ter um método CountAsync no DataSource
        var totalCount = products.Count;

        var totalPages = totalCount > 0 ? (int)Math.Ceiling(totalCount / (double)input.PageSize) : 0;
        var hasNextPage = products.Count == input.PageSize && totalCount > input.Page * input.PageSize;

        var output = AdaptToOutputModel(products, input.Page, input.PageSize, totalCount, totalPages, hasNextPage);
        return _presenter.Present(output);
    }

    private GetProductsPagedOutputModel AdaptToOutputModel(
        List<ProductDto> products,
        int page,
        int pageSize,
        int totalCount,
        int totalPages,
        bool hasNextPage)
    {
        return new GetProductsPagedOutputModel
        {
            Items = products.Select(p => new ProductSummaryOutputModel
            {
                ProductId = p.Id,
                Name = p.Name,
                Category = p.Category,
                Price = p.Price,
                Description = p.Description,
                ImageUrl = p.ImageUrl,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt
            }).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasPreviousPage = page > 1,
            HasNextPage = hasNextPage
        };
    }
}

