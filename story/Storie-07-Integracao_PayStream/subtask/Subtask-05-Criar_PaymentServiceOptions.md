# Subtask 05: Criar PaymentServiceOptions e Configuração

## Objetivo
Criar a classe `PaymentServiceOptions` para configuração do cliente HTTP do PayStream, permitindo configuração via appsettings.json e variáveis de ambiente.

## Arquivo a Criar

### `src/Infra/FastFood.OrderHub.Infra/Configurations/PaymentServiceOptions.cs`

```csharp
namespace FastFood.OrderHub.Infra.Configurations;

/// <summary>
/// Opções de configuração para o serviço de pagamento (PayStream)
/// </summary>
public class PaymentServiceOptions
{
    /// <summary>
    /// URL base do PayStream (ex: "http://paystream-service:8080")
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Timeout em segundos para chamadas HTTP (padrão: 30)
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Habilitar retry simples em caso de falha (padrão: false)
    /// </summary>
    public bool RetryEnabled { get; set; } = false;

    /// <summary>
    /// Número de tentativas em caso de retry habilitado (padrão: 1)
    /// </summary>
    public int RetryCount { get; set; } = 1;
}
```

## Arquivo a Modificar

### `src/InterfacesExternas/FastFood.OrderHub.Api/appsettings.json`

Adicionar seção `PaymentService`:

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

### `src/InterfacesExternas/FastFood.OrderHub.Api/appsettings.Development.json`

Adicionar seção `PaymentService` com valores de desenvolvimento:

```json
{
  "PaymentService": {
    "BaseUrl": "http://localhost:5001",
    "TimeoutSeconds": 30,
    "RetryEnabled": false,
    "RetryCount": 1
  }
}
```

## Validações

- [ ] Classe criada no namespace correto: `FastFood.OrderHub.Infra.Configurations`
- [ ] Classe tem propriedades: `BaseUrl`, `TimeoutSeconds`, `RetryEnabled`, `RetryCount`
- [ ] Valores padrão estão definidos (TimeoutSeconds = 30, RetryEnabled = false, RetryCount = 1)
- [ ] Seção `PaymentService` adicionada em `appsettings.json`
- [ ] Seção `PaymentService` adicionada em `appsettings.Development.json`
- [ ] XML documentation presente em todas as propriedades
- [ ] Código compila sem erros

## Observações

- A classe segue o padrão Options Pattern do .NET
- Valores podem ser sobrescritos via variáveis de ambiente (ex: `PAYMENT_SERVICE__BASE_URL`)
- `BaseUrl` não deve terminar com `/` (será tratado no PaymentServiceClient)
- `TimeoutSeconds` será convertido para `TimeSpan` no HttpClient
