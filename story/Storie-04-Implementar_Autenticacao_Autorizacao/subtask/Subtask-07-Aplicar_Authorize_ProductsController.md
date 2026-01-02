# Subtask 07: Adicionar [Authorize] no ProductsController

## Objetivo
Adicionar atributos `[Authorize]` em todos os endpoints do ProductsController, exigindo autenticação Cognito com política Admin.

## Modificações em ProductsController.cs

### 1. Adicionar Using Statement

```csharp
using Microsoft.AspNetCore.Authorization;
```

### 2. Adicionar [Authorize] em Todos os Endpoints

```csharp
[Authorize(AuthenticationSchemes = "Cognito", Policy = "Admin")]
[HttpGet]
public async Task<IActionResult> GetPaged(...)

[Authorize(AuthenticationSchemes = "Cognito", Policy = "Admin")]
[HttpGet("{id}")]
public async Task<IActionResult> GetById(Guid id)

[Authorize(AuthenticationSchemes = "Cognito", Policy = "Admin")]
[HttpPost]
public async Task<IActionResult> Create(...)

[Authorize(AuthenticationSchemes = "Cognito", Policy = "Admin")]
[HttpPut("{id}")]
public async Task<IActionResult> Update(Guid id, ...)

[Authorize(AuthenticationSchemes = "Cognito", Policy = "Admin")]
[HttpDelete("{id}")]
public async Task<IActionResult> Delete(Guid id)
```

## Endpoints Afetados

1. **GET `/api/products`** - Listar produtos paginados
2. **GET `/api/products/{id}`** - Obter produto por ID
3. **POST `/api/products`** - Criar produto
4. **PUT `/api/products/{id}`** - Atualizar produto
5. **DELETE `/api/products/{id}`** - Remover produto

## Validações
- [ ] Todos os 5 endpoints têm `[Authorize]`
- [ ] Esquema de autenticação é "Cognito"
- [ ] Política é "Admin"
- [ ] Código compila sem erros

## Testes Manuais

1. **Sem token:**
   - Chamar qualquer endpoint sem token → Deve retornar 401

2. **Com token Customer:**
   - Chamar endpoint com token CustomerBearer → Deve retornar 403

3. **Com token Cognito válido:**
   - Chamar endpoint com token Cognito válido → Deve retornar 200/201 conforme esperado



