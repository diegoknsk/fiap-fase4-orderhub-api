using FastFood.OrderHub.Application.DTOs.Payment;
using FastFood.OrderHub.Domain.Entities.OrderManagement;

namespace FastFood.OrderHub.Application.Services;

/// <summary>
/// Builder para construir OrderSnapshot a partir de uma entidade Order
/// </summary>
public class OrderSnapshotBuilder
{
    /// <summary>
    /// Constrói um OrderSnapshot a partir de uma entidade Order
    /// IMPORTANTE: Não inclui PII (Personally Identifiable Information)
    /// </summary>
    public static OrderSnapshot BuildFromOrder(Order order)
    {
        return new OrderSnapshot
        {
            Order = new OrderInfo
            {
                OrderId = order.Id,
                Code = order.Code ?? string.Empty,
                CreatedAt = order.CreatedAt
            },
            Pricing = new PricingInfo
            {
                TotalPrice = order.TotalPrice,
                Currency = "BRL"
            },
            Items = order.OrderedProducts.Select(op => new ItemInfo
            {
                ProductId = op.ProductId,
                ProductName = op.Product?.Name ?? string.Empty,
                Quantity = op.Quantity,
                FinalPrice = op.FinalPrice,
                Observation = op.Observation,
                CustomIngredients = op.CustomIngredients.Select(ci => new CustomIngredientInfo
                {
                    Name = ci.Name ?? string.Empty,
                    Price = ci.Price,
                    Quantity = ci.Quantity
                }).ToList()
            }).ToList(),
            Version = 1
        };
    }
}
