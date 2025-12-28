# Subtask 01: Criar Classes de Configuração (JwtOptions, CognitoOptions)

## Objetivo
Criar classes de configuração para JWT (CustomerBearer) e Cognito, seguindo o padrão do projeto de referência.

## Arquivos a Criar

### 1. `src/InterfacesExternas/FastFood.OrderHub.Api/Config/Auth/JwtOptions.cs`

```csharp
namespace FastFood.OrderHub.Api.Config.Auth;

public sealed record JwtOptions(
    string Issuer,
    string Audience,
    string Secret,
    int ExpirationHours
);
```

**Características:**
- Record type (imutável)
- Propriedades: Issuer, Audience, Secret, ExpirationHours
- Namespace: `FastFood.OrderHub.Api.Config.Auth`

### 2. `src/InterfacesExternas/FastFood.OrderHub.Api/Config/Auth/CognitoOptions.cs`

```csharp
namespace FastFood.OrderHub.Api.Config.Auth;

public sealed class CognitoOptions
{
    public const string SectionName = "Authentication:Cognito";
    
    public string UserPoolId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string Region { get; set; } = "us-east-1";
    public int? ClockSkewMinutes { get; set; } = 5;
    
    public string Authority => $"https://cognito-idp.{Region}.amazonaws.com/{UserPoolId}";
}
```

**Características:**
- Classe sealed
- Constante `SectionName` para referência na configuração
- Propriedade calculada `Authority` baseada em Region e UserPoolId
- Valores padrão: Region = "us-east-1", ClockSkewMinutes = 5

## Validações
- [ ] Classes criadas no namespace correto
- [ ] JwtOptions é um record
- [ ] CognitoOptions tem a propriedade Authority calculada
- [ ] Código compila sem erros

