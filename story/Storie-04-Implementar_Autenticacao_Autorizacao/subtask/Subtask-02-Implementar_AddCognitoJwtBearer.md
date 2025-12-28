# Subtask 02: Implementar Extensão AddCognitoJwtBearer

## Objetivo
Criar método de extensão para configurar autenticação JWT Bearer do AWS Cognito, validando tokens Access Token do Cognito.

## Arquivo a Criar

### `src/InterfacesExternas/FastFood.OrderHub.Api/Config/Auth/CognitoAuthenticationConfig.cs`

```csharp
using FastFood.OrderHub.Api.Config.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace FastFood.OrderHub.Api.Config.Auth;

public static class CognitoAuthenticationConfig
{
    public static AuthenticationBuilder AddCognitoJwtBearer(
        this AuthenticationBuilder authBuilder, 
        IConfiguration configuration)
    {
        var services = authBuilder.Services;
        
        // Configurar CognitoOptions
        services.Configure<CognitoOptions>(
            configuration.GetSection(CognitoOptions.SectionName));
        
        var cognito = new CognitoOptions();
        configuration.GetSection(CognitoOptions.SectionName).Bind(cognito);

        // Adicionar JWT Bearer para Cognito
        return authBuilder
            .AddJwtBearer("Cognito", options =>
            {
                options.Authority = cognito.Authority;
                options.RequireHttpsMetadata = false; // Para desenvolvimento local
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = cognito.Authority,
                    ValidateAudience = false, // Cognito Access Token não tem 'aud'
                    ValidAudience = cognito.ClientId,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromMinutes(cognito.ClockSkewMinutes ?? 5),
                    NameClaimType = "username"
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = ctx =>
                    {
                        Console.WriteLine($"Authentication failed: {ctx.Exception?.Message}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = ctx =>
                    {
                        var claims = ctx.Principal?.Claims?.ToDictionary(c => c.Type, c => c.Value);
                        Console.WriteLine($"Token validated. Claims count: {claims?.Count ?? 0}");

                        if (claims is null)
                        {
                            Console.WriteLine("Token sem claims.");
                            ctx.Fail("Token sem claims.");
                            return Task.CompletedTask;
                        }

                        // Validar token_use = "access"
                        if (!claims.TryGetValue("token_use", out var tokenUse) || tokenUse != "access")
                        {
                            Console.WriteLine($"Token use inválido: {tokenUse}");
                            ctx.Fail("Token não é Access Token.");
                            return Task.CompletedTask;
                        }

                        // Validar client_id
                        if (!claims.TryGetValue("client_id", out var clientId) || clientId != cognito.ClientId)
                        {
                            Console.WriteLine($"Client ID inválido. Esperado: {cognito.ClientId}, Recebido: {clientId}");
                            ctx.Fail("client_id inválido para esta API.");
                            return Task.CompletedTask;
                        }

                        Console.WriteLine("Token validado com sucesso!");
                        return Task.CompletedTask;
                    }
                };
            });
    }
}
```

## Validações Importantes

1. **token_use = "access":**
   - Cognito gera dois tipos de token: Access Token e ID Token
   - Apenas Access Token deve ser aceito
   - Validar que `token_use == "access"`

2. **client_id:**
   - Validar que o `client_id` do token corresponde ao configurado
   - Previne uso de tokens de outros clientes Cognito

3. **Authority:**
   - Deve ser construído corretamente: `https://cognito-idp.{Region}.amazonaws.com/{UserPoolId}`
   - Usado para validar a assinatura do token

## Validações
- [ ] Método de extensão criado
- [ ] Validação de `token_use == "access"` implementada
- [ ] Validação de `client_id` implementada
- [ ] Events configurados (OnAuthenticationFailed, OnTokenValidated)
- [ ] Código compila sem erros

