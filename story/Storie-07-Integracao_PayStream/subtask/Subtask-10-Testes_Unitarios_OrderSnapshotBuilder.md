# Subtask 10: Testes Unitários OrderSnapshotBuilder

## Objetivo
Criar testes unitários para a classe `OrderSnapshotBuilder`, validando que o snapshot é construído corretamente, não contém PII e inclui todos os campos obrigatórios.

## Arquivo a Criar

### `src/tests/FastFood.OrderHub.Tests.Unit/Services/OrderSnapshotBuilderTests.cs`

```csharp
using FastFood.OrderHub.Application.DTOs.Payment;
using FastFood.OrderHub.Application.Services;
using FastFood.OrderHub.Domain.Common.Enums;
using FastFood.OrderHub.Domain.Entities.OrderManagement;
using FluentAssertions;
using Xunit;

namespace FastFood.OrderHub.Tests.Unit.Services;

public class OrderSnapshotBuilderTests
{
    [Fact]
    public void BuildFromOrder_WhenOrderIsValid_ShouldReturnSnapshotWithAllFields()
    {
        // Arrange
        var order = CreateValidOrder();

        // Act
        var snapshot = OrderSnapshotBuilder.BuildFromOrder(order);

        // Assert
        snapshot.Should().NotBeNull();
        snapshot.Order.Should().NotBeNull();
        snapshot.Pricing.Should().NotBeNull();
        snapshot.Items.Should().NotBeNull();
        snapshot.Version.Should().Be(1);
    }

    [Fact]
    public void BuildFromOrder_WhenOrderIsValid_ShouldNotContainPII()
    {
        // Arrange
        var order = CreateValidOrder();
        order.CustomerId = Guid.NewGuid(); // PII que não deve estar no snapshot

        // Act
        var snapshot = OrderSnapshotBuilder.BuildFromOrder(order);

        // Assert
        var snapshotJson = System.Text.Json.JsonSerializer.Serialize(snapshot);
        snapshotJson.Should().NotContain("customerId");
        snapshotJson.Should().NotContain("CustomerId");
        snapshotJson.Should().NotContain("cpf");
        snapshotJson.Should().NotContain("email");
    }

    [Fact]
    public void BuildFromOrder_WhenOrderIsValid_ShouldHaveCorrectOrderInfo()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderCode = "ORD-001";
        var createdAt = DateTime.UtcNow;
        
        var order = CreateValidOrder();
        order.Id = orderId;
        order.Code = orderCode;
        order.CreatedAt = createdAt;

        // Act
        var snapshot = OrderSnapshotBuilder.BuildFromOrder(order);

        // Assert
        snapshot.Order.OrderId.Should().Be(orderId);
        snapshot.Order.Code.Should().Be(orderCode);
        snapshot.Order.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void BuildFromOrder_WhenOrderIsValid_ShouldHaveCorrectPricingInfo()
    {
        // Arrange
        var totalPrice = 50.00m;
        var order = CreateValidOrder();
        order.TotalPrice = totalPrice;

        // Act
        var snapshot = OrderSnapshotBuilder.BuildFromOrder(order);

        // Assert
        snapshot.Pricing.TotalPrice.Should().Be(totalPrice);
        snapshot.Pricing.Currency.Should().Be("BRL");
    }

    [Fact]
    public void BuildFromOrder_WhenOrderHasItems_ShouldIncludeAllItems()
    {
        // Arrange
        var order = CreateValidOrder();
        order.OrderedProducts.Add(new OrderedProduct
        {
            Id = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            Quantity = 2,
            FinalPrice = 25.00m,
            Observation = "Sem cebola"
        });

        // Act
        var snapshot = OrderSnapshotBuilder.BuildFromOrder(order);

        // Assert
        snapshot.Items.Should().HaveCount(2);
        snapshot.Items[1].Quantity.Should().Be(2);
        snapshot.Items[1].FinalPrice.Should().Be(25.00m);
        snapshot.Items[1].Observation.Should().Be("Sem cebola");
    }

    [Fact]
    public void BuildFromOrder_WhenOrderHasCustomIngredients_ShouldIncludeCustomIngredients()
    {
        // Arrange
        var order = CreateValidOrder();
        var orderedProduct = order.OrderedProducts.First();
        orderedProduct.CustomIngredients.Add(new OrderedProductIngredient
        {
            Id = Guid.NewGuid(),
            Name = "Bacon Extra",
            Price = 5.00m,
            Quantity = 1
        });

        // Act
        var snapshot = OrderSnapshotBuilder.BuildFromOrder(order);

        // Assert
        var item = snapshot.Items.First();
        item.CustomIngredients.Should().HaveCount(1);
        item.CustomIngredients[0].Name.Should().Be("Bacon Extra");
        item.CustomIngredients[0].Price.Should().Be(5.00m);
        item.CustomIngredients[0].Quantity.Should().Be(1);
    }

    [Fact]
    public void BuildFromOrder_WhenOrderCodeIsNull_ShouldUseEmptyString()
    {
        // Arrange
        var order = CreateValidOrder();
        order.Code = null;

        // Act
        var snapshot = OrderSnapshotBuilder.BuildFromOrder(order);

        // Assert
        snapshot.Order.Code.Should().Be(string.Empty);
    }

    [Fact]
    public void BuildFromOrder_WhenOrderIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        Order order = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => OrderSnapshotBuilder.BuildFromOrder(order));
    }

    [Fact]
    public void BuildFromOrder_WhenSnapshotIsBuilt_ShouldNotBeEmptyObject()
    {
        // Arrange
        var order = CreateValidOrder();

        // Act
        var snapshot = OrderSnapshotBuilder.BuildFromOrder(order);

        // Assert
        var snapshotJson = System.Text.Json.JsonSerializer.Serialize(snapshot);
        snapshotJson.Should().NotBe("{}");
        snapshotJson.Should().Contain("order");
        snapshotJson.Should().Contain("pricing");
        snapshotJson.Should().Contain("items");
        snapshotJson.Should().Contain("version");
    }

    private Order CreateValidOrder()
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            Code = "ORD-001",
            CreatedAt = DateTime.UtcNow,
            OrderStatus = EnumOrderStatus.AwaitingPayment,
            TotalPrice = 50.00m,
            CustomerId = Guid.NewGuid()
        };

        order.OrderedProducts.Add(new OrderedProduct
        {
            Id = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            OrderId = order.Id,
            Quantity = 1,
            FinalPrice = 50.00m,
            Product = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Hambúrguer",
                Category = EnumProductCategory.Food
            }
        });

        return order;
    }
}
```

## Validações

- [ ] Teste: BuildFromOrder retorna snapshot com estrutura correta
- [ ] Teste: Snapshot não contém PII
- [ ] Teste: Snapshot tem OrderInfo correto
- [ ] Teste: Snapshot tem PricingInfo correto
- [ ] Teste: Snapshot inclui todos os items
- [ ] Teste: Snapshot inclui customIngredients quando presentes
- [ ] Teste: Snapshot trata null em Code (usa string.Empty)
- [ ] Teste: BuildFromOrder lança ArgumentNullException quando order é null
- [ ] Teste: Snapshot não é objeto vazio
- [ ] Todos os testes passam
- [ ] Código compila sem erros

## Observações

- Usar FluentAssertions para assertions mais legíveis
- Criar helper method `CreateValidOrder()` para reutilização
- Testar edge cases (null, empty, etc.)
- Validar que snapshot é serializável para JSON
