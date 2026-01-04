# Subtask 03: Criar Interface IPaymentServiceClient

## Objetivo
Criar a interface `IPaymentServiceClient` que define o contrato para comunicação com o PayStream, seguindo o padrão Port/Adapter da Clean Architecture.

## Arquivo a Criar

### `src/Core/FastFood.OrderHub.Application/Ports/IPaymentServiceClient.cs`

```csharp
using FastFood.OrderHub.Application.DTOs.Payment;

namespace FastFood.OrderHub.Application.Ports;

/// <summary>
/// Port para comunicação com o serviço de pagamento (PayStream)
/// </summary>
public interface IPaymentServiceClient
{
    /// <summary>
    /// Cria um intent de pagamento no PayStream
    /// </summary>
    /// <param name="request">Dados do pagamento a ser criado</param>
    /// <param name="bearerToken">Token Bearer para autenticação (será repassado ao PayStream)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Response com informações do pagamento criado</returns>
    /// <exception cref="HttpRequestException">Quando ocorre erro na comunicação HTTP</exception>
    Task<CreatePaymentResponse> CreatePaymentAsync(
        CreatePaymentRequest request,
        string bearerToken,
        CancellationToken cancellationToken = default);
}
```

## Validações

- [ ] Interface criada no namespace correto: `FastFood.OrderHub.Application.Ports`
- [ ] Interface tem XML documentation completa
- [ ] Método `CreatePaymentAsync` recebe `CreatePaymentRequest` e `bearerToken`
- [ ] Método retorna `Task<CreatePaymentResponse>`
- [ ] Método suporta `CancellationToken`
- [ ] Documentação XML menciona exceções possíveis
- [ ] Código compila sem erros

## Observações

- Esta é a **Port** no padrão Port/Adapter
- A implementação concreta ficará na camada Infrastructure
- O `bearerToken` será repassado diretamente no header Authorization da chamada HTTP
- A interface deve ser registrada no DI container na camada API
