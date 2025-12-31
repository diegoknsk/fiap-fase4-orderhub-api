# Subtask 08: Adicionar [Authorize] no OrderController

## Objetivo
Adicionar atributos `[Authorize]` nos endpoints do OrderController conforme regras:
- Endpoints Admin: Cognito + Policy Admin
- Endpoints Customer: CustomerBearer + Policy Customer

## Modificações em OrderController.cs

### 1. Adicionar Using Statements

```csharp
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
```

### 2. Adicionar [Authorize] nos Endpoints Admin

```csharp
[Authorize(AuthenticationSchemes = "Cognito", Policy = "Admin")]
[HttpGet]
public async Task<IActionResult> GetPaged(...)

[Authorize(AuthenticationSchemes = "Cognito", Policy = "Admin")]
[HttpGet("{id}")]
public async Task<IActionResult> GetById(Guid id)
```

### 3. Adicionar [Authorize] nos Endpoints Customer

```csharp
[Authorize(AuthenticationSchemes = "CustomerBearer", Policy = "Customer")]
[HttpPost("start")]
public async Task<IActionResult> Start(...)

[Authorize(AuthenticationSchemes = "CustomerBearer", Policy = "Customer")]
[HttpPost("add-product")]
public async Task<IActionResult> AddProduct(...)

[Authorize(AuthenticationSchemes = "CustomerBearer", Policy = "Customer")]
[HttpPut("update-product")]
public async Task<IActionResult> UpdateProduct(...)

[Authorize(AuthenticationSchemes = "CustomerBearer", Policy = "Customer")]
[HttpDelete("remove-product")]
public async Task<IActionResult> RemoveProduct(...)

[Authorize(AuthenticationSchemes = "CustomerBearer", Policy = "Customer")]
[HttpPost("{id}/confirm-selection")]
public async Task<IActionResult> ConfirmSelection(Guid id)
```

## Endpoints por Tipo

### Endpoints Admin (Cognito)
1. **GET `/api/order`** - Listar pedidos paginados
2. **GET `/api/order/{id}`** - Obter pedido por ID

### Endpoints Customer (CustomerBearer)
3. **POST `/api/order/start`** - Iniciar novo pedido
4. **POST `/api/order/add-product`** - Adicionar produto ao pedido
5. **PUT `/api/order/update-product`** - Atualizar produto no pedido
6. **DELETE `/api/order/remove-product`** - Remover produto do pedido
7. **POST `/api/order/{id}/confirm-selection`** - Confirmar seleção do pedido

## Validações
- [ ] Endpoints Admin têm `[Authorize]` com Cognito
- [ ] Endpoints Customer têm `[Authorize]` com CustomerBearer
- [ ] Políticas corretas aplicadas
- [ ] Código compila sem erros

## Observações

- A validação de ownership será implementada na Subtask 09
- Por enquanto, apenas a autenticação/autorização básica é aplicada


