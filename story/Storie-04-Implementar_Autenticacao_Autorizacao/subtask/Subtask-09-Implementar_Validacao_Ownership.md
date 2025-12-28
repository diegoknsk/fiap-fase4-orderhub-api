# Subtask 09: Implementar Validação de Ownership

## Objetivo
Implementar validação de ownership nos endpoints de customer, garantindo que um customer só possa acessar seus próprios pedidos.

## Modificações em OrderController.cs

### 1. Adicionar Método Helper para Validação

```csharp
private async Task<bool> ValidateCustomerOwnsOrder(Guid orderId, Guid customerId)
{
    var input = new GetOrderByIdInputModel { OrderId = orderId };
    var order = await _getOrderByIdUseCase.ExecuteAsync(input);
    
    if (order == null)
        return false;
    
    return order.CustomerId == customerId;
}
```

**Observação:** Se `GetOrderByIdResponse` não tiver `CustomerId`, será necessário ajustar o UseCase ou criar um método específico no DataSource.

### 2. Adicionar Validação nos Endpoints Customer

#### POST `/api/order/start`
```csharp
[Authorize(AuthenticationSchemes = "CustomerBearer", Policy = "Customer")]
[HttpPost("start")]
public async Task<IActionResult> Start([FromBody] StartOrderInputModel input)
{
    // Extrair CustomerId do token
    var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
    if (!Guid.TryParse(sub, out var customerId))
        return Unauthorized(ApiResponse<StartOrderResponse>.Fail("Token inválido: CustomerId não encontrado."));
    
    // Validar que o CustomerId do body corresponde ao do token
    if (input.CustomerId != customerId)
        return Forbid(); // 403 Forbidden
    
    try
    {
        var response = await _startOrderUseCase.ExecuteAsync(input);
        return CreatedAtAction(nameof(GetById), new { id = response.OrderId }, 
            ApiResponse<StartOrderResponse>.Ok(response, "Pedido iniciado com sucesso."));
    }
    catch (ArgumentException ex)
    {
        return BadRequest(ApiResponse<StartOrderResponse>.Fail(ex.Message));
    }
    catch (InvalidOperationException ex)
    {
        return BadRequest(ApiResponse<StartOrderResponse>.Fail(ex.Message));
    }
}
```

#### POST `/api/order/add-product`
```csharp
[Authorize(AuthenticationSchemes = "CustomerBearer", Policy = "Customer")]
[HttpPost("add-product")]
public async Task<IActionResult> AddProduct([FromBody] AddProductToOrderInputModel input)
{
    var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
    if (!Guid.TryParse(sub, out var customerId))
        return Unauthorized();
    
    // Validar ownership
    if (!await ValidateCustomerOwnsOrder(input.OrderId, customerId))
        return Forbid(); // 403 Forbidden
    
    try
    {
        var response = await _addProductToOrderUseCase.ExecuteAsync(input);
        // ... resto do código
    }
    // ... tratamento de exceções
}
```

#### PUT `/api/order/update-product`
```csharp
[Authorize(AuthenticationSchemes = "CustomerBearer", Policy = "Customer")]
[HttpPut("update-product")]
public async Task<IActionResult> UpdateProduct([FromBody] UpdateProductInOrderInputModel input)
{
    var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
    if (!Guid.TryParse(sub, out var customerId))
        return Unauthorized();
    
    if (!await ValidateCustomerOwnsOrder(input.OrderId, customerId))
        return Forbid();
    
    // ... resto do código
}
```

#### DELETE `/api/order/remove-product`
```csharp
[Authorize(AuthenticationSchemes = "CustomerBearer", Policy = "Customer")]
[HttpDelete("remove-product")]
public async Task<IActionResult> RemoveProduct([FromBody] RemoveProductFromOrderInputModel input)
{
    var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
    if (!Guid.TryParse(sub, out var customerId))
        return Unauthorized();
    
    if (!await ValidateCustomerOwnsOrder(input.OrderId, customerId))
        return Forbid();
    
    // ... resto do código
}
```

#### POST `/api/order/{id}/confirm-selection`
```csharp
[Authorize(AuthenticationSchemes = "CustomerBearer", Policy = "Customer")]
[HttpPost("{id}/confirm-selection")]
public async Task<IActionResult> ConfirmSelection(Guid id)
{
    var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
    if (!Guid.TryParse(sub, out var customerId))
        return Unauthorized();
    
    if (!await ValidateCustomerOwnsOrder(id, customerId))
        return Forbid();
    
    // ... resto do código
}
```

## Códigos HTTP

- **401 Unauthorized:** Token ausente, inválido ou expirado
- **403 Forbidden:** Token válido, mas customer não tem permissão para acessar o recurso (ownership)

## Validações
- [ ] Método `ValidateCustomerOwnsOrder` implementado
- [ ] Validação aplicada em todos os endpoints Customer
- [ ] CustomerId extraído do token corretamente
- [ ] Retorna 403 quando customer tenta acessar pedido de outro customer
- [ ] Retorna 401 quando token está ausente ou inválido
- [ ] Código compila sem erros

## Observações

1. **Extração do CustomerId:**
   - Usar `JwtRegisteredClaimNames.Sub` para extrair o CustomerId
   - Validar que é um Guid válido

2. **Validação de Ownership:**
   - Deve ser feita **antes** de executar a operação
   - Retornar `403 Forbidden` (não 401) quando ownership não corresponde

3. **Performance:**
   - A validação faz uma query adicional ao banco
   - Considerar cache se necessário (futuro)

