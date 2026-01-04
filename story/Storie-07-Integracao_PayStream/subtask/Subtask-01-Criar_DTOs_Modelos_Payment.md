# Subtask 01: Criar DTOs e Modelos de Payment

## Objetivo
Criar os DTOs e modelos necessários para a integração com PayStream, incluindo `CreatePaymentRequest`, `CreatePaymentResponse` e a estrutura completa de `OrderSnapshot`.

## Arquivos a Criar

### 1. `src/Core/FastFood.OrderHub.Application/DTOs/Payment/CreatePaymentRequest.cs`

```csharp
namespace FastFood.OrderHub.Application.DTOs.Payment;

/// <summary>
/// Request para criar pagamento no PayStream
/// </summary>
public class CreatePaymentRequest
{
    public Guid OrderId { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderSnapshot OrderSnapshot { get; set; } = null!;
}
```

### 2. `src/Core/FastFood.OrderHub.Application/DTOs/Payment/CreatePaymentResponse.cs`

```csharp
namespace FastFood.OrderHub.Application.DTOs.Payment;

/// <summary>
/// Response da criação de pagamento no PayStream
/// </summary>
public class CreatePaymentResponse
{
    public Guid PaymentId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
```

### 3. `src/Core/FastFood.OrderHub.Application/DTOs/Payment/OrderSnapshot.cs`

```csharp
namespace FastFood.OrderHub.Application.DTOs.Payment;

/// <summary>
/// Snapshot híbrido do pedido para envio ao PayStream
/// NÃO contém PII (Personally Identifiable Information)
/// </summary>
public class OrderSnapshot
{
    public OrderInfo Order { get; set; } = null!;
    public PricingInfo Pricing { get; set; } = null!;
    public List<ItemInfo> Items { get; set; } = new();
    public int Version { get; set; } = 1;
}

/// <summary>
/// Informações básicas do pedido
/// </summary>
public class OrderInfo
{
    public Guid OrderId { get; set; }
    public string Code { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Informações de preço
/// </summary>
public class PricingInfo
{
    public decimal TotalPrice { get; set; }
    public string Currency { get; set; } = "BRL";
}

/// <summary>
/// Informações de item do pedido
/// </summary>
public class ItemInfo
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal FinalPrice { get; set; }
    public string? Observation { get; set; }
    public List<CustomIngredientInfo> CustomIngredients { get; set; } = new();
}

/// <summary>
/// Informações de ingrediente customizado
/// </summary>
public class CustomIngredientInfo
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
```

## Validações

- [ ] Todos os arquivos criados no namespace correto: `FastFood.OrderHub.Application.DTOs.Payment`
- [ ] `CreatePaymentRequest` tem propriedades: `OrderId`, `TotalAmount`, `OrderSnapshot`
- [ ] `CreatePaymentResponse` tem propriedades: `PaymentId`, `Status`, `CreatedAt`
- [ ] `OrderSnapshot` tem estrutura completa: `Order`, `Pricing`, `Items`, `Version`
- [ ] `OrderSnapshot` NÃO contém campos de PII (customerId, cpf, email, etc.)
- [ ] Todos os tipos têm XML documentation
- [ ] Código compila sem erros

## Observações

- Os DTOs devem ser serializáveis para JSON (padrão do .NET)
- `OrderSnapshot` deve garantir que nunca seja objeto vazio `{}`
- Valores padrão devem ser definidos onde apropriado (ex: `Currency = "BRL"`, `Version = 1`)
