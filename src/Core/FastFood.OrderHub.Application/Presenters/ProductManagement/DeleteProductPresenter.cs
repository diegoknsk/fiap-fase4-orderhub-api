using FastFood.OrderHub.Application.OutputModels.ProductManagement;
using FastFood.OrderHub.Application.Responses.ProductManagement;

namespace FastFood.OrderHub.Application.Presenters.ProductManagement;

/// <summary>
/// Presenter para DeleteProduct
/// </summary>
public class DeleteProductPresenter
{
    public DeleteProductResponse Present(DeleteProductOutputModel output)
    {
        return new DeleteProductResponse
        {
            ProductId = output.ProductId,
            Success = output.Success
        };
    }
}

