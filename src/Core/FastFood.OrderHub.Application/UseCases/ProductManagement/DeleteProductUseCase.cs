using FastFood.OrderHub.Application.InputModels.ProductManagement;
using FastFood.OrderHub.Application.OutputModels.ProductManagement;
using FastFood.OrderHub.Application.Ports;
using FastFood.OrderHub.Application.Presenters.ProductManagement;
using FastFood.OrderHub.Application.Responses.ProductManagement;

namespace FastFood.OrderHub.Application.UseCases.ProductManagement;

/// <summary>
/// UseCase para deletar produto
/// </summary>
public class DeleteProductUseCase
{
    private readonly IProductDataSource _productDataSource;
    private readonly DeleteProductPresenter _presenter;

    public DeleteProductUseCase(
        IProductDataSource productDataSource,
        DeleteProductPresenter presenter)
    {
        _productDataSource = productDataSource;
        _presenter = presenter;
    }

    public async Task<DeleteProductResponse?> ExecuteAsync(DeleteProductInputModel input)
    {
        // Verificar se produto existe
        var exists = await _productDataSource.ExistsAsync(input.ProductId);
        if (!exists)
            return null;

        // Remover produto (soft delete)
        await _productDataSource.RemoveAsync(input.ProductId);

        // Criar OutputModel
        var output = new DeleteProductOutputModel
        {
            ProductId = input.ProductId,
            Success = true
        };

        return _presenter.Present(output);
    }
}


