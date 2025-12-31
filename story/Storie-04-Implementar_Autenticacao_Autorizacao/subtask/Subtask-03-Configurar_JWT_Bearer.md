# Subtask 03: Configurar Autenticação JWT Bearer no Program.cs

## Objetivo
Configurar autenticação JWT Bearer para tokens de Customer (CustomerBearer scheme) no Program.cs, validando tokens gerados pelo Lambda Customer.

## Modificações em Program.cs

### 1. Adicionar Using Statements

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using FastFood.OrderHub.Api.Config.Auth;
```

### 2. Desabilitar Mapeamento Automático de Claims

**IMPORTANTE:** Adicionar antes de `builder.Services.AddControllers()`:

```csharp
// Critical: Disable automatic claim mapping to avoid role claim remapping
JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
```

### 3. Configurar JWT Options

```csharp
// Configure JWT options
builder.Services.Configure<JwtOptions>("Customer", builder.Configuration.GetSection("JwtSettings"));
```

### 4. Helper Function para TokenValidationParameters

```csharp
// Helper function to build token validation parameters
static TokenValidationParameters BuildJwtParams(IConfiguration cfg, string section)
{
    var issuer = cfg[$"{section}:Issuer"];
    var audience = cfg[$"{section}:Audience"];
    var secret = cfg[$"{section}:Secret"];

    return new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret!)),
        ClockSkew = TimeSpan.FromSeconds(30),
        RoleClaimType = "role",
        NameClaimType = JwtRegisteredClaimNames.Sub
    };
}
```

### 5. Configurar Authentication

Adicionar após configuração de serviços:

```csharp
// Configure authentication with JWT schemes
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer("CustomerBearer", o =>
    {
        o.TokenValidationParameters = BuildJwtParams(builder.Configuration, "JwtSettings");
    })
    .AddCognitoJwtBearer(builder.Configuration);
```

### 6. Adicionar UseAuthentication no Pipeline

No método `app.Build()`, adicionar antes de `app.UseAuthorization()`:

```csharp
app.UseAuthentication();
app.UseAuthorization();
```

## Configuração appsettings.json

```json
{
  "JwtSettings": {
    "Issuer": "FastFood.Auth",
    "Audience": "FastFood.API",
    "Secret": "",
    "ExpirationHours": "24"
  }
}
```

## Variáveis de Ambiente (Alternativa)

- `JWT__ISSUER`
- `JWT__AUDIENCE`
- `JWT__SECRET`
- `JWT__EXPIRATIONHOURS`

## Validações
- [ ] JwtSecurityTokenHandler.DefaultMapInboundClaims = false configurado
- [ ] JWT Bearer scheme "CustomerBearer" configurado
- [ ] TokenValidationParameters configurado corretamente
- [ ] UseAuthentication() adicionado no pipeline
- [ ] Código compila sem erros

## Observações

1. **Chave Secreta:**
   - A chave secreta (`JwtSettings:Secret`) **DEVE** ser a mesma usada no Lambda Customer
   - Se diferentes, os tokens não serão validados

2. **Claims Esperadas:**
   - `sub`: CustomerId (Guid)
   - `customerId`: CustomerId (Guid)
   - `jti`: JWT ID (Guid)
   - `iat`: Issued At (Unix timestamp)

3. **NameClaimType:**
   - Configurado como `JwtRegisteredClaimNames.Sub` para facilitar extração do CustomerId


