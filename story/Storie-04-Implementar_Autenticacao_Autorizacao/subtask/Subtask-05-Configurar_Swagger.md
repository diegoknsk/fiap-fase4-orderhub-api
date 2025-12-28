# Subtask 05: Configurar Swagger com Múltiplos Esquemas de Segurança

## Objetivo
Configurar Swagger para suportar múltiplos esquemas de autenticação (CustomerBearer e Cognito), permitindo testar endpoints com ambos os tipos de token.

## Modificações em Program.cs

### 1. Adicionar Using Statement

```csharp
using Microsoft.OpenApi.Models;
```

### 2. Modificar AddSwaggerGen

Substituir o `builder.Services.AddSwaggerGen()` atual por:

```csharp
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "FastFood.OrderHub.Api", Version = "v1" });

    // CustomerBearer scheme
    c.AddSecurityDefinition("CustomerBearer", new()
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Customer token (Bearer {token}). Token gerado pelo Lambda Customer."
    });

    // Cognito scheme
    c.AddSecurityDefinition("Cognito", new()
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Bearer do Cognito. Ex: 'Bearer {token}'. Token gerado pelo Lambda Admin."
    });

    c.OperationFilter<AuthorizeBySchemeOperationFilter>();
});
```

## Arquivo a Criar

### `src/InterfacesExternas/FastFood.OrderHub.Api/Config/Auth/AuthorizeBySchemeOperationFilter.cs`

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FastFood.OrderHub.Api.Config.Auth;

public class AuthorizeBySchemeOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var authAttrs = context.MethodInfo
            .GetCustomAttributes(true).OfType<AuthorizeAttribute>()
            .Concat(context.MethodInfo.DeclaringType?.GetCustomAttributes(true).OfType<AuthorizeAttribute>() ?? Enumerable.Empty<AuthorizeAttribute>())
            .ToArray();

        if (!authAttrs.Any()) return;

        operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
        operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Forbidden" });

        var schemes = authAttrs
            .SelectMany(a => (a.AuthenticationSchemes ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (schemes.Length == 0) return;

        operation.Security ??= new List<OpenApiSecurityRequirement>();
        foreach (var scheme in schemes)
        {
            var reference = new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = scheme }
            };
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [reference] = Array.Empty<string>()
            });
        }
    }
}
```

## Funcionamento

1. **Detecção Automática:**
   - O filtro detecta automaticamente os atributos `[Authorize]` nos métodos e classes
   - Extrai os esquemas de autenticação do atributo `AuthenticationSchemes`

2. **Múltiplos Esquemas:**
   - Se um endpoint tiver `[Authorize(AuthenticationSchemes = "Cognito", Policy = "Admin")]`, apenas o esquema "Cognito" será adicionado ao Swagger
   - Se tiver `[Authorize(AuthenticationSchemes = "CustomerBearer", Policy = "Customer")]`, apenas "CustomerBearer" será adicionado

3. **UI do Swagger:**
   - No Swagger UI, cada endpoint mostrará um botão "Authorize" com o esquema correto
   - O usuário pode inserir o token no formato: `Bearer {token}`

## Validações
- [ ] Swagger configurado com dois esquemas de segurança
- [ ] AuthorizeBySchemeOperationFilter implementado
- [ ] Filtro registrado no AddSwaggerGen
- [ ] Swagger UI exibe botões de autorização corretos
- [ ] Código compila sem erros

## Teste Manual

1. Abrir Swagger UI (`/swagger`)
2. Verificar que endpoints com `[Authorize]` mostram botão "Authorize"
3. Clicar em "Authorize" e verificar que o esquema correto está disponível
4. Inserir token e testar endpoint

