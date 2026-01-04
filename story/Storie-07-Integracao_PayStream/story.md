# Storie-07: Integração com PayStream para Iniciar Pagamento

## Status
- **Estado:** 🔄 Pendente
- **Data de Início:** -
- **Data de Conclusão:** -
- **Prioridade:** Alta

## Descrição
Como desenvolvedor, preciso integrar o microserviço OrderHub com o PayStream para iniciar o fluxo de pagamento após o cliente finalizar a seleção do pedido (confirm-selection). Quando um pedido é finalizado, o OrderHub deve chamar a API do PayStream para criar o intent de pagamento, repassando o token de autorização do cliente e um snapshot híbrido do pedido.

## Objetivo Geral
1. Após o cliente finalizar o pedido (confirm-selection), iniciar automaticamente o fluxo de pagamento no PayStream
2. Criar integração HTTP síncrona entre OrderHub e PayStream seguindo padrão Clean Architecture
3. Repassar o token Bearer do cliente para o PayStream na chamada HTTP
4. Gerar snapshot híbrido do pedido (sem PII) para envio ao PayStream
5. Implementar tratamento de erros e resiliência básica
6. Garantir que falhas na integração não quebrem o fluxo de finalização do pedido

## Contexto e Referências

### Endpoint PayStream
- **URL:** `POST {{PAYSTREAM_BASE_URL}}/api/Payment/create`
- **Autenticação:** Bearer Token (repassado do OrderHub)
- **Content-Type:** `application/json`
- **Body:**
  ```json
  {
    "orderId": "{{orderId}}",
    "totalAmount": {{totalAmount}},
    "orderSnapshot": {{orderSnapshotJson}}
  }
  ```

### Estrutura do orderSnapshot (Híbrido)
O snapshot deve ser um objeto JSON com a seguinte estrutura:
```json
{
  "order": {
    "orderId": "guid",
    "code": "ORD-001",
    "createdAt": "2024-01-01T00:00:00Z"
  },
  "pricing": {
    "totalPrice": 50.00,
    "currency": "BRL"
  },
  "items": [
    {
      "productId": "guid",
      "productName": "Hambúrguer",
      "quantity": 2,
      "finalPrice": 25.00,
      "observation": "Sem cebola",
      "customIngredients": [
        {
          "name": "Bacon Extra",
          "price": 5.00,
          "quantity": 1
        }
      ]
    }
  ],
  "version": 1
}
```

**Importante:**
- NÃO incluir PII (Personally Identifiable Information): `customerId`, `cpf`, `email`, etc.
- O campo `orderSnapshot` NÃO pode ser `{}` (objeto vazio)
- Todos os campos obrigatórios devem estar presentes

## Design / Arquitetura

### Decisão de Comportamento: Opção A (Finaliza e Tenta Pagamento)
**Escolhida:** Opção A - Finaliza pedido e tenta criar pagamento; se falhar, pedido fica finalizado mas sem pagamento.

**Justificativa:**
- Garante que o pedido não fica em estado inconsistente (nem finalizado, nem cancelado)
- Permite recuperação posterior via endpoint manual ou job assíncrono
- Melhor experiência do usuário (pedido finalizado mesmo se houver problema temporário no PayStream)
- Alinhado com padrão de resiliência: falha em serviço externo não deve bloquear operação principal

**Comportamento:**
1. Cliente chama `POST /api/order/{id}/confirm-selection`
2. OrderHub finaliza o pedido (status → `AwaitingPayment`)
3. OrderHub salva o pedido finalizado no DynamoDB
4. OrderHub tenta criar pagamento no PayStream
5. **Se sucesso:** Retorna 200 OK com informações do pedido
6. **Se falha:** 
   - Loga o erro detalhadamente
   - Retorna 502 Bad Gateway com mensagem genérica
   - Pedido permanece finalizado (status `AwaitingPayment`)
   - Pagamento pode ser criado posteriormente via endpoint manual ou job

### Estrutura de Camadas

#### Application Layer
- **Port:** `IPaymentServiceClient` (ou `IPaymentGatewayPort`)
  - Método: `Task<CreatePaymentResponse> CreatePaymentAsync(CreatePaymentRequest request, string bearerToken)`
- **UseCase:** Modificar `ConfirmOrderSelectionUseCase` para orquestrar a chamada ao PayStream após finalizar o pedido
- **DTOs/Models:**
  - `CreatePaymentRequest` - Request para PayStream
  - `CreatePaymentResponse` - Response do PayStream
  - `OrderSnapshot` - Modelo do snapshot híbrido
  - `OrderSnapshotBuilder` - Builder para construir snapshot a partir de Order

#### Infrastructure Layer
- **Implementação:** `PaymentServiceClient` em `Infra/Integrations/`
  - Implementa `IPaymentServiceClient`
  - Usa `HttpClient` injetado via DI
  - Configuração de URL e timeout via `IOptions<PaymentServiceOptions>`
  - Tratamento de erros HTTP (retry simples opcional)

#### API Layer (Borda)
- **Program.cs:** Configurar `HttpClient` para `PaymentServiceClient`
- **appsettings.json:** Adicionar seção `PaymentService` com `BaseUrl` e `TimeoutSeconds`

## Configurações Necessárias

### appsettings.json
```json
{
  "PaymentService": {
    "BaseUrl": "http://paystream-service:8080",
    "TimeoutSeconds": 30,
    "RetryEnabled": false,
    "RetryCount": 1
  }
}
```

### Variáveis de Ambiente (Alternativa/Complemento)
- `PAYMENT_SERVICE__BASE_URL` - URL base do PayStream
- `PAYMENT_SERVICE__TIMEOUT_SECONDS` - Timeout em segundos (padrão: 30)
- `PAYMENT_SERVICE__RETRY_ENABLED` - Habilitar retry simples (padrão: false)
- `PAYMENT_SERVICE__RETRY_COUNT` - Número de tentativas (padrão: 1)

## Arquivos a Criar

### 1. Application Layer (Ports e DTOs)
- `src/Core/FastFood.OrderHub.Application/Ports/IPaymentServiceClient.cs`
- `src/Core/FastFood.OrderHub.Application/DTOs/Payment/CreatePaymentRequest.cs`
- `src/Core/FastFood.OrderHub.Application/DTOs/Payment/CreatePaymentResponse.cs`
- `src/Core/FastFood.OrderHub.Application/DTOs/Payment/OrderSnapshot.cs`
- `src/Core/FastFood.OrderHub.Application/Services/OrderSnapshotBuilder.cs`

### 2. Infrastructure Layer (Implementação)
- `src/Infra/FastFood.OrderHub.Infra/Integrations/PaymentServiceClient.cs`
- `src/Infra/FastFood.OrderHub.Infra/Configurations/PaymentServiceOptions.cs`

### 3. Extensão IRequestContext (para obter Bearer Token)
- `src/Core/FastFood.OrderHub.Application/Ports/IRequestContext.cs` (adicionar método `GetBearerToken()`)
- `src/Infra/FastFood.OrderHub.Infra/Services/RequestContext.cs` (implementar método)

## Arquivos a Modificar

### 1. Application Layer
- `src/Core/FastFood.OrderHub.Application/UseCases/OrderManagement/ConfirmOrderSelectionUseCase.cs`
  - Adicionar dependência de `IPaymentServiceClient`
  - Adicionar dependência de `IRequestContext` (para obter Bearer Token)
  - Após finalizar e salvar pedido, chamar PayStream
  - Tratar exceções e logar erros

### 2. API Layer
- `src/InterfacesExternas/FastFood.OrderHub.Api/Program.cs`
  - Configurar `HttpClient` para `PaymentServiceClient`
  - Registrar `IPaymentServiceClient` com implementação `PaymentServiceClient`
  - Configurar `IOptions<PaymentServiceOptions>`

- `src/InterfacesExternas/FastFood.OrderHub.Api/appsettings.json`
  - Adicionar seção `PaymentService`

- `src/InterfacesExternas/FastFood.OrderHub.Api/Controllers/OrderController.cs`
  - Ajustar tratamento de erro no endpoint `ConfirmSelection` para retornar 502 quando PayStream falhar

## Detalhamento Técnico

### 1. Interface IPaymentServiceClient

```csharp
namespace FastFood.OrderHub.Application.Ports;

public interface IPaymentServiceClient
{
    Task<CreatePaymentResponse> CreatePaymentAsync(
        CreatePaymentRequest request, 
        string bearerToken, 
        CancellationToken cancellationToken = default);
}
```

### 2. DTOs de Payment

**CreatePaymentRequest:**
```csharp
public class CreatePaymentRequest
{
    public Guid OrderId { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderSnapshot OrderSnapshot { get; set; }
}
```

**CreatePaymentResponse:**
```csharp
public class CreatePaymentResponse
{
    public Guid PaymentId { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

**OrderSnapshot:**
```csharp
public class OrderSnapshot
{
    public OrderInfo Order { get; set; }
    public PricingInfo Pricing { get; set; }
    public List<ItemInfo> Items { get; set; }
    public int Version { get; set; }
}

public class OrderInfo
{
    public Guid OrderId { get; set; }
    public string Code { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PricingInfo
{
    public decimal TotalPrice { get; set; }
    public string Currency { get; set; } = "BRL";
}

public class ItemInfo
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal FinalPrice { get; set; }
    public string? Observation { get; set; }
    public List<CustomIngredientInfo> CustomIngredients { get; set; } = new();
}

public class CustomIngredientInfo
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
```

### 3. OrderSnapshotBuilder

```csharp
public class OrderSnapshotBuilder
{
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

### 4. PaymentServiceClient (Infra)

```csharp
public class PaymentServiceClient : IPaymentServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly PaymentServiceOptions _options;
    private readonly ILogger<PaymentServiceClient> _logger;

    public PaymentServiceClient(
        HttpClient httpClient,
        IOptions<PaymentServiceOptions> options,
        ILogger<PaymentServiceClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<CreatePaymentResponse> CreatePaymentAsync(
        CreatePaymentRequest request,
        string bearerToken,
        CancellationToken cancellationToken = default)
    {
        // Implementação com HttpClient
        // Repassar bearerToken no header Authorization
        // Serializar request para JSON
        // Tratar erros HTTP (400, 401, 500, etc.)
        // Logar erros
        // Retornar CreatePaymentResponse ou lançar exceção
    }
}
```

### 5. Configuração HttpClient no Program.cs

```csharp
// Configurar HttpClient para PaymentServiceClient
builder.Services.AddHttpClient<IPaymentServiceClient, PaymentServiceClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<PaymentServiceOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    // Configurações adicionais se necessário
});

// Configurar Options
builder.Services.Configure<PaymentServiceOptions>(
    builder.Configuration.GetSection("PaymentService"));

// Registrar PaymentServiceClient
builder.Services.AddScoped<IPaymentServiceClient, PaymentServiceClient>();
```

### 6. Modificação no ConfirmOrderSelectionUseCase

```csharp
public class ConfirmOrderSelectionUseCase
{
    private readonly IOrderDataSource _orderDataSource;
    private readonly ConfirmOrderSelectionPresenter _presenter;
    private readonly IPaymentServiceClient _paymentServiceClient;
    private readonly IRequestContext _requestContext;
    private readonly ILogger<ConfirmOrderSelectionUseCase> _logger;

    public async Task<ConfirmOrderSelectionResponse> ExecuteAsync(ConfirmOrderSelectionInputModel input)
    {
        // ... código existente para finalizar pedido ...
        
        // Após salvar pedido finalizado:
        order.FinalizeOrderSelection();
        orderDto = ConvertToDto(order, orderDto.OrderSource);
        await _orderDataSource.UpdateAsync(orderDto);

        // Tentar criar pagamento no PayStream
        try
        {
            var bearerToken = _requestContext.GetBearerToken();
            if (string.IsNullOrWhiteSpace(bearerToken))
            {
                _logger.LogWarning("Bearer token não encontrado. Pagamento não será criado.");
            }
            else
            {
                var snapshot = OrderSnapshotBuilder.BuildFromOrder(order);
                var paymentRequest = new CreatePaymentRequest
                {
                    OrderId = order.Id,
                    TotalAmount = order.TotalPrice,
                    OrderSnapshot = snapshot
                };

                await _paymentServiceClient.CreatePaymentAsync(paymentRequest, bearerToken);
                _logger.LogInformation("Pagamento criado com sucesso para pedido {OrderId}", order.Id);
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Erro ao criar pagamento no PayStream para pedido {OrderId}", order.Id);
            throw new BusinessException("Erro ao iniciar pagamento. Tente novamente mais tarde.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao criar pagamento no PayStream para pedido {OrderId}", order.Id);
            throw new BusinessException("Erro ao iniciar pagamento. Tente novamente mais tarde.");
        }

        var output = AdaptToOutputModel(order);
        return _presenter.Present(output);
    }
}
```

### 7. Extensão IRequestContext para obter Bearer Token

**IRequestContext.cs:**
```csharp
public interface IRequestContext
{
    bool IsAdmin { get; }
    string? CustomerId { get; }
    string? GetBearerToken(); // Novo método
}
```

**RequestContext.cs:**
```csharp
public string? GetBearerToken()
{
    var httpContext = _httpContextAccessor.HttpContext;
    if (httpContext == null)
        return null;

    // Extrair token do header Authorization
    var authHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();
    if (string.IsNullOrWhiteSpace(authHeader))
        return null;

    // Remover prefixo "Bearer " se presente
    if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        return authHeader.Substring(7);

    return authHeader;
}
```

## Validações e Testes

### Validações Funcionais

1. **Fluxo de Sucesso:**
   - [ ] Cliente finaliza pedido → Pedido é salvo com status `AwaitingPayment`
   - [ ] PayStream é chamado com token Bearer correto
   - [ ] PayStream é chamado com orderSnapshot válido (não vazio, sem PII)
   - [ ] Resposta 200 OK é retornada ao cliente

2. **Fluxo de Falha (PayStream indisponível):**
   - [ ] Cliente finaliza pedido → Pedido é salvo com status `AwaitingPayment`
   - [ ] PayStream retorna erro (500, timeout, etc.)
   - [ ] Erro é logado detalhadamente
   - [ ] Cliente recebe 502 Bad Gateway
   - [ ] Pedido permanece finalizado (status `AwaitingPayment`)

3. **Validação de Token:**
   - [ ] Token Bearer é extraído corretamente do header Authorization
   - [ ] Token é repassado no header Authorization da chamada ao PayStream
   - [ ] Se token não estiver presente, loga warning mas não falha

4. **Validação de Snapshot:**
   - [ ] Snapshot contém todos os campos obrigatórios
   - [ ] Snapshot NÃO contém PII (customerId, cpf, email)
   - [ ] Snapshot não é objeto vazio `{}`
   - [ ] CustomIngredients são incluídos quando presentes
   - [ ] Observation é incluída quando presente

### Testes Unitários

1. **OrderSnapshotBuilder:**
   - [ ] Teste: BuildFromOrder retorna snapshot com estrutura correta
   - [ ] Teste: Snapshot não contém PII
   - [ ] Teste: Snapshot inclui customIngredients quando presentes
   - [ ] Teste: Snapshot tem version = 1

2. **PaymentServiceClient:**
   - [ ] Teste: CreatePaymentAsync faz chamada HTTP correta
   - [ ] Teste: Token Bearer é incluído no header Authorization
   - [ ] Teste: Request body está correto (JSON serializado)
   - [ ] Teste: Trata erro 400 (Bad Request)
   - [ ] Teste: Trata erro 401 (Unauthorized)
   - [ ] Teste: Trata erro 500 (Internal Server Error)
   - [ ] Teste: Trata timeout
   - [ ] Teste: Trata exceção de rede

3. **ConfirmOrderSelectionUseCase:**
   - [ ] Teste: Após finalizar pedido, chama PaymentServiceClient
   - [ ] Teste: Se PaymentServiceClient falhar, loga erro mas não quebra
   - [ ] Teste: Se token não estiver presente, loga warning mas continua
   - [ ] Teste: Snapshot é construído corretamente antes de enviar

4. **RequestContext:**
   - [ ] Teste: GetBearerToken extrai token do header Authorization
   - [ ] Teste: GetBearerToken remove prefixo "Bearer " se presente
   - [ ] Teste: GetBearerToken retorna null se header não estiver presente

### Testes BDD (SpecFlow)

1. **Cenário: Finalizar pedido com sucesso e criar pagamento**
   ```
   Given existe um pedido com ID "order-123" no status "Started"
   And o pedido tem itens
   When o cliente finaliza o pedido "order-123"
   Then o pedido deve ter status "AwaitingPayment"
   And o PayStream deve ser chamado com o token Bearer
   And o PayStream deve receber orderSnapshot válido
   And a resposta deve ser 200 OK
   ```

2. **Cenário: Finalizar pedido mas PayStream está indisponível**
   ```
   Given existe um pedido com ID "order-123" no status "Started"
   And o PayStream está retornando erro 500
   When o cliente finaliza o pedido "order-123"
   Then o pedido deve ter status "AwaitingPayment"
   And o erro deve ser logado
   And a resposta deve ser 502 Bad Gateway
   ```

3. **Cenário: Finalizar pedido sem token Bearer**
   ```
   Given existe um pedido com ID "order-123" no status "Started"
   And a requisição não tem token Bearer
   When o cliente finaliza o pedido "order-123"
   Then o pedido deve ter status "AwaitingPayment"
   And um warning deve ser logado sobre token ausente
   And a resposta deve ser 200 OK
   ```

## Subtasks

### Fase 1: Estrutura Base e DTOs
- [Subtask 01: Criar DTOs e modelos de Payment](./subtask/Subtask-01-Criar_DTOs_Modelos_Payment.md)
- [Subtask 02: Criar OrderSnapshotBuilder](./subtask/Subtask-02-Criar_OrderSnapshotBuilder.md)
- [Subtask 03: Criar interface IPaymentServiceClient](./subtask/Subtask-03-Criar_Interface_IPaymentServiceClient.md)

### Fase 2: Implementação da Integração
- [Subtask 04: Implementar PaymentServiceClient](./subtask/Subtask-04-Implementar_PaymentServiceClient.md)
- [Subtask 05: Criar PaymentServiceOptions e configuração](./subtask/Subtask-05-Criar_PaymentServiceOptions.md)
- [Subtask 06: Configurar HttpClient no Program.cs](./subtask/Subtask-06-Configurar_HttpClient.md)

### Fase 3: Integração no UseCase
- [Subtask 07: Estender IRequestContext para obter Bearer Token](./subtask/Subtask-07-Estender_IRequestContext.md)
- [Subtask 08: Modificar ConfirmOrderSelectionUseCase](./subtask/Subtask-08-Modificar_ConfirmOrderSelectionUseCase.md)
- [Subtask 09: Ajustar tratamento de erros no OrderController](./subtask/Subtask-09-Ajustar_Tratamento_Erros.md)

### Fase 4: Testes
- [Subtask 10: Testes unitários OrderSnapshotBuilder](./subtask/Subtask-10-Testes_Unitarios_OrderSnapshotBuilder.md)
- [Subtask 11: Testes unitários PaymentServiceClient](./subtask/Subtask-11-Testes_Unitarios_PaymentServiceClient.md)
- [Subtask 12: Testes unitários ConfirmOrderSelectionUseCase](./subtask/Subtask-12-Testes_Unitarios_ConfirmOrderSelectionUseCase.md)
- [Subtask 13: Testes BDD integração PayStream](./subtask/Subtask-13-Testes_BDD_Integracao_PayStream.md)

## Parâmetros de Configuração Necessários

| Parâmetro | Fonte | Descrição | Exemplo |
|-----------|-------|-----------|---------|
| `PaymentService:BaseUrl` | appsettings.json ou `PAYMENT_SERVICE__BASE_URL` | URL base do PayStream | "http://paystream-service:8080" |
| `PaymentService:TimeoutSeconds` | appsettings.json ou `PAYMENT_SERVICE__TIMEOUT_SECONDS` | Timeout em segundos | "30" |
| `PaymentService:RetryEnabled` | appsettings.json ou `PAYMENT_SERVICE__RETRY_ENABLED` | Habilitar retry simples | "false" |
| `PaymentService:RetryCount` | appsettings.json ou `PAYMENT_SERVICE__RETRY_COUNT` | Número de tentativas | "1" |

## Critérios de Aceite

### Funcionais
- [ ] Após finalizar pedido (confirm-selection), PayStream é chamado automaticamente
- [ ] Token Bearer do cliente é repassado corretamente para PayStream
- [ ] orderSnapshot é enviado com estrutura híbrida correta (não vazio, sem PII)
- [ ] Se PayStream falhar, pedido permanece finalizado (status `AwaitingPayment`)
- [ ] Se PayStream falhar, erro é logado e cliente recebe 502 Bad Gateway
- [ ] Se token Bearer não estiver presente, warning é logado mas fluxo continua
- [ ] Pedido é finalizado mesmo se PayStream estiver indisponível

### Técnicos
- [ ] Integração segue padrão Clean Architecture (Port/Adapter)
- [ ] HttpClient é configurado via DI apenas na borda (API)
- [ ] Configurações suportam appsettings.json e variáveis de ambiente
- [ ] Código segue padrão arquitetural do projeto (~80% Clean Architecture)
- [ ] Logs contêm informações suficientes para debug (sem expor dados sensíveis)
- [ ] Tratamento de erros HTTP é robusto (400, 401, 500, timeout)

### Qualidade
- [ ] Código compila sem erros
- [ ] Testes unitários passam (cobertura mínima: 80%)
- [ ] Testes BDD passam
- [ ] Sem code smells críticos
- [ ] Documentação atualizada (se necessário)

## Observações Importantes

1. **Token Bearer:**
   - O token deve ser extraído do header `Authorization` da requisição original
   - O token deve ser repassado exatamente como recebido (com prefixo "Bearer " ou sem)
   - Se token não estiver presente, não deve falhar o fluxo (apenas logar warning)

2. **OrderSnapshot:**
   - **NUNCA** incluir PII: `customerId`, `cpf`, `email`, `phone`, etc.
   - **SEMPRE** incluir todos os campos obrigatórios da estrutura híbrida
   - **NUNCA** enviar objeto vazio `{}`
   - Validar que snapshot está correto antes de enviar

3. **Resiliência:**
   - Falha no PayStream não deve impedir finalização do pedido
   - Erros devem ser logados detalhadamente para debug
   - Cliente deve receber resposta apropriada (502 para falha externa)
   - Considerar implementar retry simples (1 tentativa) se configurado

4. **Arquitetura:**
   - Orquestração fica na Application (UseCase)
   - Chamada HTTP fica na Infra (PaymentServiceClient)
   - Configuração de HttpClient fica na borda (Program.cs)
   - Seguir padrão Port/Adapter (IPaymentServiceClient como Port)

5. **Testes:**
   - Mockar HttpClient nos testes unitários
   - Usar HttpMockServer ou similar para testes de integração
   - Testar todos os cenários de erro (400, 401, 500, timeout)
   - Validar estrutura do snapshot nos testes

## Referências

- **Documentação Microsoft:** [HttpClient Factory](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests)
- **Clean Architecture:** Padrão Port/Adapter para integrações externas
- **Projeto PayStream:** Estrutura de referência para Clean Architecture
