# Subtask 07: Estender IRequestContext para obter Bearer Token

## Objetivo
Estender a interface `IRequestContext` e sua implementação `RequestContext` para permitir extrair o token Bearer da requisição HTTP atual, necessário para repassar ao PayStream.

## Arquivos a Modificar

### 1. `src/Core/FastFood.OrderHub.Application/Ports/IRequestContext.cs`

Adicionar método `GetBearerToken()`:

```csharp
namespace FastFood.OrderHub.Application.Ports;

/// <summary>
/// Interface para contexto da requisição HTTP
/// </summary>
public interface IRequestContext
{
    /// <summary>
    /// Indica se o usuário autenticado é um administrador (Cognito)
    /// </summary>
    bool IsAdmin { get; }

    /// <summary>
    /// ID do cliente autenticado (CustomerBearer)
    /// </summary>
    string? CustomerId { get; }

    /// <summary>
    /// Obtém o token Bearer da requisição HTTP atual
    /// </summary>
    /// <returns>Token Bearer (sem prefixo "Bearer ") ou null se não estiver presente</returns>
    string? GetBearerToken();
}
```

### 2. `src/Infra/FastFood.OrderHub.Infra/Services/RequestContext.cs`

Implementar método `GetBearerToken()`:

```csharp
using FastFood.OrderHub.Application.Ports;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace FastFood.OrderHub.Infra.Services;

/// <summary>
/// Implementação de IRequestContext que lê claims do HttpContext
/// </summary>
public class RequestContext : IRequestContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RequestContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // ... código existente para IsAdmin e CustomerId ...

    public string? GetBearerToken()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return null;

        // Extrair token do header Authorization
        var authHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(authHeader))
            return null;

        // Remover prefixo "Bearer " se presente (case-insensitive)
        const string bearerPrefix = "Bearer ";
        if (authHeader.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
            return authHeader.Substring(bearerPrefix.Length);

        // Se não tiver prefixo, retornar como está (pode ser apenas o token)
        return authHeader;
    }
}
```

## Validações

- [ ] Método `GetBearerToken()` adicionado em `IRequestContext`
- [ ] Método `GetBearerToken()` implementado em `RequestContext`
- [ ] Método retorna `null` se `HttpContext` for null
- [ ] Método retorna `null` se header `Authorization` não estiver presente
- [ ] Método remove prefixo "Bearer " (case-insensitive)
- [ ] Método retorna token sem prefixo se prefixo estiver presente
- [ ] Método retorna token completo se não tiver prefixo
- [ ] XML documentation presente
- [ ] Código compila sem erros

## Observações

- O método deve ser case-insensitive para o prefixo "Bearer "
- Se o token não tiver prefixo, retorna como está (para flexibilidade)
- Retorna `null` quando token não está presente (não lança exceção)
- O token retornado será usado diretamente no header Authorization do HttpClient
