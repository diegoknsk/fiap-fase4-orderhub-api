using FastFood.OrderHub.Application.OutputModels.OrderManagement;
using FastFood.OrderHub.Application.Responses.OrderManagement;

namespace FastFood.OrderHub.Application.Presenters.OrderManagement;

/// <summary>
/// Presenter para GetOrderById
/// </summary>
public class GetOrderByIdPresenter
{
    public GetOrderByIdResponse Present(GetOrderByIdOutputModel output)
    {
        return new GetOrderByIdResponse
        {
            OrderId = output.OrderId,
            Code = output.Code,
            CustomerId = output.CustomerId,
            CreatedAt = output.CreatedAt,
            OrderStatus = output.OrderStatus,
            TotalPrice = output.TotalPrice,
            Items = output.Items.Select(item => new OrderedProductResponse
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Category = item.Category,
                Quantity = item.Quantity,
                FinalPrice = item.FinalPrice,
                Observation = item.Observation,
                CustomIngredients = item.CustomIngredients.Select(ci => new OrderedProductIngredientResponse
                {
                    Id = ci.Id,
                    Name = ci.Name,
                    Price = ci.Price,
                    Quantity = ci.Quantity
                }).ToList()
            }).ToList()
        };
    }
}

