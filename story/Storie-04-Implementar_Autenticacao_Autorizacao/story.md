# Storie-04: Implementar Autenticação e Autorização

## Status
- **Estado:** 🔄 Pendente
- **Data de Início:** -
- **Data de Conclusão:** -
- **Prioridade:** Alta

## Descrição
Como desenvolvedor, preciso implementar autenticação e autorização na API OrderHub para proteger os endpoints de Products e Orders, utilizando dois esquemas de autenticação distintos:
1. **Cognito** - Para endpoints de administração (ProductsController e alguns endpoints do OrderController)
2. **CustomerBearer** - Para endpoints de customer (maioria dos endpoints do OrderController)

A solução deve funcionar com os tokens gerados pelos lambdas do projeto `fiap-fase4-auth-lambda`, validando corretamente os tokens JWT emitidos por esses serviços.

## Objetivo Geral
1. Configurar autenticação JWT Bearer para tokens de Customer (gerados pelo Lambda Customer)
2. Configurar autenticação JWT Bearer para tokens do AWS Cognito (gerados pelo Lambda Admin)
3. Criar políticas de autorização (Admin e Customer)
4. Aplicar atributos `[Authorize]` nos controllers conforme regras de negócio
5. Implementar validação de ownership (customer só pode acessar seus próprios pedidos)
6. Configurar Swagger para suportar múltiplos esquemas de autenticação
7. Garantir compatibilidade com tokens gerados pelos lambdas de autenticação

## Contexto e Referências

### Projeto de Referência
- **Projeto Fase3:** `C:\Projetos\Fiap\fiap-fase3-aplicacao\fiap-fastfood\01-InterfacesExternas\FastFood.Api`
- **Lambdas de Autenticação:** `C:\Projetos\Fiap\fiap-fase4-auth-lambda\src\InterfacesExternas`

### Esquemas de Autenticação

#### 1. CustomerBearer (Tokens JWT do Lambda Customer)
- **Fonte:** Lambda `FastFood.Auth.Lambda.Customer`
- **Endpoints que geram tokens:**
  - `POST /api/customer/anonymous` - Cria customer anônimo e retorna token
  - `POST /api/customer/register` - Registra customer por CPF e retorna token
  - `POST /api/customer/identify` - Identifica customer existente por CPF e retorna token
- **Estrutura do Token JWT:**
  - **Claims obrigatórias:**
    - `sub`: CustomerId (Guid) - Subject do token
    - `customerId`: CustomerId (Guid) - ID do customer
    - `jti`: JWT ID (Guid) - Identificador único do token
    - `iat`: Issued At (Unix timestamp) - Data de emissão
  - **Configuração esperada:**
    - `JwtSettings:Issuer` - Emissor do token (ex: "FastFood.Auth")
    - `JwtSettings:Audience` - Audiência do token (ex: "FastFood.API")
    - `JwtSettings:Secret` - Chave secreta para assinatura (deve ser a mesma usada no Lambda Customer)
    - `JwtSettings:ExpirationHours` - Tempo de expiração (ex: 24 horas)

#### 2. Cognito (Tokens do AWS Cognito)
- **Fonte:** Lambda `FastFood.Auth.Lambda.Admin`
- **Endpoint que gera tokens:**
  - `POST /api/admin/login` - Autentica admin via AWS Cognito e retorna AccessToken/IdToken
- **Estrutura do Token:**
  - **Tipo:** Access Token do AWS Cognito
  - **Claims obrigatórias:**
    - `token_use`: Deve ser "access" (não "id")
    - `client_id`: Client ID do Cognito (deve corresponder ao configurado)
    - `username`: Username do admin
    - `scope`: Deve conter "aws.cognito.signin.user.admin"
  - **Configuração esperada:**
    - `Cognito:Region` - Região do Cognito (ex: "us-east-1")
    - `Cognito:UserPoolId` - ID do User Pool do Cognito
    - `Cognito:ClientId` - Client ID do Cognito
    - `Cognito:ClockSkewMinutes` - Tolerância de relógio (opcional, padrão: 5 minutos)
  - **Authority:** `https://cognito-idp.{Region}.amazonaws.com/{UserPoolId}`

## Endpoints e Autorização

### ProductsController
Todos os endpoints devem usar autenticação **Cognito** com política **Admin**:

1. **GET `/api/products`** - Listar produtos paginados
   - `[Authorize(AuthenticationSchemes = "Cognito", Policy = "Admin")]`

2. **GET `/api/products/{id}`** - Obter produto por ID
   - `[Authorize(AuthenticationSchemes = "Cognito", Policy = "Admin")]`

3. **POST `/api/products`** - Criar produto
   - `[Authorize(AuthenticationSchemes = "Cognito", Policy = "Admin")]`

4. **PUT `/api/products/{id}`** - Atualizar produto
   - `[Authorize(AuthenticationSchemes = "Cognito", Policy = "Admin")]`

5. **DELETE `/api/products/{id}`** - Remover produto
   - `[Authorize(AuthenticationSchemes = "Cognito", Policy = "Admin")]`

### OrderController
Endpoints mistos (Admin e Customer):

#### Endpoints Admin (Cognito):
1. **GET `/api/order`** - Listar pedidos paginados
   - `[Authorize(AuthenticationSchemes = "Cognito", Policy = "Admin")]`

2. **GET `/api/order/{id}`** - Obter pedido por ID
   - `[Authorize(AuthenticationSchemes = "Cognito", Policy = "Admin")]`

#### Endpoints Customer (CustomerBearer):
3. **POST `/api/order/start`** - Iniciar novo pedido
   - `[Authorize(AuthenticationSchemes = "CustomerBearer", Policy = "Customer")]`
   - **Validação adicional:** CustomerId do token deve corresponder ao CustomerId do body
   - **Extração do CustomerId:** `User.FindFirstValue(JwtRegisteredClaimNames.Sub)`

4. **POST `/api/order/add-product`** - Adicionar produto ao pedido
   - `[Authorize(AuthenticationSchemes = "CustomerBearer", Policy = "Customer")]`
   - **Validação adicional:** Validar que o pedido pertence ao customer do token

5. **PUT `/api/order/update-product`** - Atualizar produto no pedido
   - `[Authorize(AuthenticationSchemes = "CustomerBearer", Policy = "Customer")]`
   - **Validação adicional:** Validar que o pedido pertence ao customer do token

6. **DELETE `/api/order/remove-product`** - Remover produto do pedido
   - `[Authorize(AuthenticationSchemes = "CustomerBearer", Policy = "Customer")]`
   - **Validação adicional:** Validar que o pedido pertence ao customer do token

7. **POST `/api/order/{id}/confirm-selection`** - Confirmar seleção do pedido
   - `[Authorize(AuthenticationSchemes = "CustomerBearer", Policy = "Customer")]`
   - **Validação adicional:** Validar que o pedido pertence ao customer do token

## Configurações Necessárias

### appsettings.json
```json
{
  "JwtSettings": {
    "Issuer": "FastFood.Auth",
    "Audience": "FastFood.API",
    "Secret": "", // Deve ser a mesma chave usada no Lambda Customer
    "ExpirationHours": "24"
  },
  "Authentication": {
    "Cognito": {
      "Region": "", // Ex: "us-east-1"
      "UserPoolId": "", // ID do User Pool do Cognito
      "ClientId": "", // Client ID do Cognito
      "ClockSkewMinutes": 5
    }
  }
}
```

### Variáveis de Ambiente (Alternativa/Complemento)
- `JWT__ISSUER` - Emissor do token JWT
- `JWT__AUDIENCE` - Audiência do token JWT
- `JWT__SECRET` - Chave secreta do JWT (mesma do Lambda Customer)
- `COGNITO__REGION` - Região do Cognito
- `COGNITO__USERPOOLID` - User Pool ID
- `COGNITO__CLIENTID` - Client ID
- `COGNITO__CLOCKSKEWMINUTES` - Tolerância de relógio (opcional)

## Arquivos a Criar

### 1. Configurações de Autenticação
- `src/InterfacesExternas/FastFood.OrderHub.Api/Config/Auth/JwtOptions.cs`
- `src/InterfacesExternas/FastFood.OrderHub.Api/Config/Auth/CognitoOptions.cs`
- `src/InterfacesExternas/FastFood.OrderHub.Api/Config/Auth/CognitoAuthenticationConfig.cs`
- `src/InterfacesExternas/FastFood.OrderHub.Api/Config/Auth/AuthorizeBySchemeOperationFilter.cs`

### 2. Helpers/Extensions
- `src/InterfacesExternas/FastFood.OrderHub.Api/Extensions/ClaimsPrincipalExtensions.cs` (opcional, para facilitar extração de CustomerId)

## Arquivos a Modificar

### 1. Program.cs
- Adicionar configuração de autenticação JWT Bearer para CustomerBearer
- Adicionar configuração de autenticação Cognito
- Configurar políticas de autorização (Admin e Customer)
- Configurar Swagger com múltiplos esquemas de segurança
- Adicionar `app.UseAuthentication()` antes de `app.UseAuthorization()`

### 2. Controllers
- **ProductsController.cs:** Adicionar `[Authorize]` em todos os endpoints
- **OrderController.cs:** Adicionar `[Authorize]` conforme regras acima
- Implementar validação de ownership nos endpoints de customer

## Detalhamento Técnico

### 1. Configuração JWT Bearer (CustomerBearer)

```csharp
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer("CustomerBearer", options =>
    {
        var jwtSettings = builder.Configuration.GetSection("JwtSettings");
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var secret = jwtSettings["Secret"];

        options.TokenValidationParameters = new TokenValidationParameters
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
    });
```

**Importante:**
- Desabilitar mapeamento automático de claims: `JwtSecurityTokenHandler.DefaultMapInboundClaims = false;`
- Validar que o token contém as claims obrigatórias (`sub`, `customerId`, `jti`, `iat`)

### 2. Configuração Cognito JWT Bearer

```csharp
.AddCognitoJwtBearer(builder.Configuration);
```

**Implementação do método de extensão:**
- Configurar `Authority` baseado em `Region` e `UserPoolId`
- Validar `token_use == "access"`
- Validar `client_id` corresponde ao configurado
- Configurar eventos `OnTokenValidated` para validações adicionais

### 3. Políticas de Autorização

```csharp
builder.Services.AddAuthorization(options =>
{
    // Política para Admin (Cognito)
    options.AddPolicy("Admin", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "aws.cognito.signin.user.admin");
    });
    
    // Política para Customer (JWT Bearer)
    options.AddPolicy("Customer", policy =>
    {
        policy.RequireAuthenticatedUser();
        // Opcional: validar claim específica se necessário
    });
});
```

### 4. Validação de Ownership

Para endpoints de customer, validar que o pedido pertence ao customer do token:

```csharp
private async Task<bool> ValidateCustomerOwnsOrder(Guid orderId, Guid customerId)
{
    var order = await _orderDataSource.GetByIdAsync(orderId);
    return order?.CustomerId == customerId;
}

// No endpoint:
var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
if (!Guid.TryParse(sub, out var customerId)) 
    return Unauthorized();

if (!await ValidateCustomerOwnsOrder(model.OrderId, customerId)) 
    return Forbid();
```

### 5. Configuração Swagger

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
        Description = "Customer token (Bearer {token})"
    });

    // Cognito scheme
    c.AddSecurityDefinition("Cognito", new()
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Bearer do Cognito. Ex: 'Bearer {token}'"
    });

    c.OperationFilter<AuthorizeBySchemeOperationFilter>();
});
```

## Validações e Testes

### Validações de Compatibilidade

1. **Token Customer (CustomerBearer):**
   - Token gerado pelo Lambda Customer deve ser aceito
   - Claims `sub`, `customerId`, `jti`, `iat` devem estar presentes
   - Token expirado deve retornar 401
   - Token com assinatura inválida deve retornar 401
   - Token com issuer/audience incorretos deve retornar 401

2. **Token Cognito (Admin):**
   - Access Token gerado pelo Lambda Admin deve ser aceito
   - `token_use` deve ser "access"
   - `client_id` deve corresponder ao configurado
   - Token expirado deve retornar 401
   - Token inválido deve retornar 401

### Testes Funcionais

1. **Endpoints Products (Admin):**
   - [ ] GET `/api/products` sem token → 401
   - [ ] GET `/api/products` com token Customer → 403
   - [ ] GET `/api/products` com token Cognito válido → 200
   - [ ] POST `/api/products` com token Cognito válido → 201
   - [ ] PUT `/api/products/{id}` com token Cognito válido → 200
   - [ ] DELETE `/api/products/{id}` com token Cognito válido → 200

2. **Endpoints Order (Admin):**
   - [ ] GET `/api/order` sem token → 401
   - [ ] GET `/api/order` com token Customer → 403
   - [ ] GET `/api/order` com token Cognito válido → 200
   - [ ] GET `/api/order/{id}` com token Cognito válido → 200

3. **Endpoints Order (Customer):**
   - [ ] POST `/api/order/start` sem token → 401
   - [ ] POST `/api/order/start` com token Cognito → 403
   - [ ] POST `/api/order/start` com token Customer válido → 201
   - [ ] POST `/api/order/start` com CustomerId do body diferente do token → 403
   - [ ] POST `/api/order/add-product` com token Customer válido → 200
   - [ ] POST `/api/order/add-product` tentando acessar pedido de outro customer → 403
   - [ ] PUT `/api/order/update-product` com token Customer válido → 200
   - [ ] DELETE `/api/order/remove-product` com token Customer válido → 200
   - [ ] POST `/api/order/{id}/confirm-selection` com token Customer válido → 200
   - [ ] POST `/api/order/{id}/confirm-selection` tentando confirmar pedido de outro customer → 403

## Subtasks

### Fase 1: Configuração Base
- [Subtask 01: Criar classes de configuração (JwtOptions, CognitoOptions)](./subtask/Subtask-01-Criar_Classes_Configuracao.md)
- [Subtask 02: Implementar extensão AddCognitoJwtBearer](./subtask/Subtask-02-Implementar_AddCognitoJwtBearer.md)
- [Subtask 03: Configurar autenticação JWT Bearer no Program.cs](./subtask/Subtask-03-Configurar_JWT_Bearer.md)
- [Subtask 04: Configurar políticas de autorização](./subtask/Subtask-04-Configurar_Politicas_Autorizacao.md)

### Fase 2: Swagger e Documentação
- [Subtask 05: Configurar Swagger com múltiplos esquemas de segurança](./subtask/Subtask-05-Configurar_Swagger.md)
- [Subtask 06: Implementar AuthorizeBySchemeOperationFilter](./subtask/Subtask-06-Implementar_OperationFilter.md)

### Fase 3: Aplicar Autorização nos Controllers
- [Subtask 07: Adicionar [Authorize] no ProductsController](./subtask/Subtask-07-Aplicar_Authorize_ProductsController.md)
- [Subtask 08: Adicionar [Authorize] no OrderController](./subtask/Subtask-08-Aplicar_Authorize_OrderController.md)
- [Subtask 09: Implementar validação de ownership](./subtask/Subtask-09-Implementar_Validacao_Ownership.md)

### Fase 4: Testes e Validação
- [Subtask 10: Testar autenticação CustomerBearer](./subtask/Subtask-10-Testar_CustomerBearer.md)
- [Subtask 11: Testar autenticação Cognito](./subtask/Subtask-11-Testar_Cognito.md)
- [Subtask 12: Testar validação de ownership](./subtask/Subtask-12-Testar_Validacao_Ownership.md)
- [Subtask 13: Validar compatibilidade com lambdas](./subtask/Subtask-13-Validar_Compatibilidade_Lambdas.md)

## Parâmetros de Configuração Necessários

### JWT Settings (CustomerBearer)
| Parâmetro | Fonte | Descrição | Exemplo |
|-----------|-------|-----------|---------|
| `JwtSettings:Issuer` | appsettings.json ou `JWT__ISSUER` | Emissor do token JWT | "FastFood.Auth" |
| `JwtSettings:Audience` | appsettings.json ou `JWT__AUDIENCE` | Audiência do token JWT | "FastFood.API" |
| `JwtSettings:Secret` | appsettings.json ou `JWT__SECRET` | Chave secreta para assinatura (deve ser a mesma do Lambda Customer) | "sua-chave-secreta-aqui" |
| `JwtSettings:ExpirationHours` | appsettings.json ou `JWT__EXPIRATIONHOURS` | Tempo de expiração em horas | "24" |

### Cognito Settings
| Parâmetro | Fonte | Descrição | Exemplo |
|-----------|-------|-----------|---------|
| `Authentication:Cognito:Region` | appsettings.json ou `COGNITO__REGION` | Região do AWS Cognito | "us-east-1" |
| `Authentication:Cognito:UserPoolId` | appsettings.json ou `COGNITO__USERPOOLID` | ID do User Pool do Cognito | "us-east-1_XXXXXXXXX" |
| `Authentication:Cognito:ClientId` | appsettings.json ou `COGNITO__CLIENTID` | Client ID do Cognito | "xxxxxxxxxxxxxxxxxxxxx" |
| `Authentication:Cognito:ClockSkewMinutes` | appsettings.json ou `COGNITO__CLOCKSKEWMINUTES` | Tolerância de relógio em minutos (opcional) | "5" |

## Critérios de Aceite

### Funcionais
- [ ] Todos os endpoints de ProductsController requerem autenticação Cognito com política Admin
- [ ] Endpoints de OrderController requerem autenticação conforme especificado (Admin ou Customer)
- [ ] Tokens gerados pelo Lambda Customer são aceitos nos endpoints CustomerBearer
- [ ] Tokens gerados pelo Lambda Admin (Cognito) são aceitos nos endpoints Cognito
- [ ] Validação de ownership funciona corretamente (customer só acessa seus pedidos)
- [ ] Tokens expirados retornam 401 Unauthorized
- [ ] Tokens inválidos retornam 401 Unauthorized
- [ ] Tentativa de acesso com token incorreto retorna 403 Forbidden

### Técnicos
- [ ] Configurações suportam appsettings.json e variáveis de ambiente
- [ ] Swagger exibe corretamente os esquemas de autenticação
- [ ] Swagger permite testar endpoints com ambos os esquemas
- [ ] Código segue padrão arquitetural do projeto
- [ ] Sem vazamento de informações sensíveis em logs de erro

### Qualidade
- [ ] Código compila sem erros
- [ ] Testes funcionais passam
- [ ] Documentação atualizada (README, se necessário)
- [ ] Sem code smells críticos

## Observações Importantes

1. **Chave Secreta JWT:**
   - A chave secreta (`JwtSettings:Secret`) **DEVE** ser a mesma usada no Lambda Customer
   - Se as chaves forem diferentes, os tokens não serão validados
   - Recomenda-se usar variáveis de ambiente ou secrets do Kubernetes para produção

2. **Configuração Cognito:**
   - O `UserPoolId` e `ClientId` devem corresponder exatamente aos usados no Lambda Admin
   - A região deve estar correta para que o Authority seja construído corretamente

3. **Validação de Ownership:**
   - A validação de ownership deve ser feita **antes** de executar a operação
   - Retornar `403 Forbidden` (não 401) quando o customer tenta acessar pedido de outro customer
   - Retornar `401 Unauthorized` quando o token está ausente ou inválido

4. **Swagger:**
   - O Swagger deve permitir selecionar qual esquema usar para cada endpoint
   - O filtro `AuthorizeBySchemeOperationFilter` deve detectar automaticamente qual esquema usar baseado no `[Authorize]`

5. **Compatibilidade com Lambdas:**
   - Testar com tokens reais gerados pelos lambdas
   - Validar que as claims esperadas estão presentes
   - Garantir que a validação de assinatura funciona corretamente

## Referências

- **Projeto Fase3 (Referência):** `C:\Projetos\Fiap\fiap-fase3-aplicacao\fiap-fastfood\01-InterfacesExternas\FastFood.Api`
- **Lambdas de Autenticação:** `C:\Projetos\Fiap\fiap-fase4-auth-lambda\src\InterfacesExternas`
- **Documentação Microsoft:** [ASP.NET Core Authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/)
- **Documentação AWS Cognito:** [AWS Cognito User Pools](https://docs.aws.amazon.com/cognito/latest/developerguide/cognito-user-identity-pools.html)
- **JWT.io:** [JWT Debugger](https://jwt.io/) - Para validar estrutura de tokens


