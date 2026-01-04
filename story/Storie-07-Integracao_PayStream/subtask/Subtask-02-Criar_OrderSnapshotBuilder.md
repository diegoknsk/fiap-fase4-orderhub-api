# Subtask 02: Criar OrderSnapshotBuilder

## Objetivo
Criar a classe `OrderSnapshotBuilder` responsável por construir o snapshot híbrido do pedido a partir da entidade de domínio `Order`, garantindo que não contenha PII e que todos os campos obrigatórios estejam presentes.

## Arquivo a Criar

### `src/Core/FastFood.OrderHub.Application/Services/OrderSnapshotBuilder.cs`

```csharp
using FastFood.OrderHub.Application.DTOs.Payment;
using FastFood.OrderHub.Domain.Entities.OrderManagement;

namespace FastFood.OrderHub.Application.Services;

/// <summary>
/// Builder para criar OrderSnapshot a partir de uma entidade Order
/// </summary>
public static class OrderSnapshotBuilder
{
    /// <summary>
    /// Constrói um OrderSnapshot a partir de uma entidade Order
    /// </summary>
    /// <param name="order">Entidade Order do domínio</param>
    /// <returns>OrderSnapshot pronto para envio ao PayStream</returns>
    public static OrderSnapshot BuildFromOrder(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

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
                    Name = ci.Name,
                    Price = ci.Price,
                    Quantity = ci.Quantity
                }).ToList()
            }).ToList(),
            Version = 1
        };
    }
}
```

## Validações

- [ ] Classe criada no namespace correto: `FastFood.OrderHub.Application.Services`
- [ ] Método `BuildFromOrder` é estático
- [ ] Método valida se `order` é null (lança `ArgumentNullException`)
- [ ] Snapshot contém todos os campos obrigatórios
- [ ] Snapshot NÃO contém PII (customerId, cpf, email, etc.)
- [ ] CustomIngredients são incluídos quando presentes
- [ ] Observation é incluída quando presente
- [ ] Version sempre é 1
- [ ] Currency sempre é "BRL"
- [ ] Código compila sem erros

## Observações

- A classe é `static` pois não mantém estado
- O método deve tratar valores null de forma segura (ex: `order.Code ?? string.Empty`)
- `Product?.Name` usa null-conditional operator para evitar NullReferenceException
- CustomIngredients sempre retorna uma lista (mesmo que vazia)
