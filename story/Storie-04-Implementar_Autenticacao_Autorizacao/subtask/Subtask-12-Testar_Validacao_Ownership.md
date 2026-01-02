# Subtask 12: Testar Validação de Ownership

## Objetivo
Testar que a validação de ownership funciona corretamente, garantindo que customers só possam acessar seus próprios pedidos.

## Pré-requisitos

1. Lambda Customer rodando
2. OrderHub API rodando
3. Pelo menos 2 customers diferentes com tokens válidos
4. Pelo menos 1 pedido criado por cada customer

## Cenários de Teste

### 1. Customer Acessa Seu Próprio Pedido

```bash
# 1. Obter token do Customer A
TOKEN_A=$(curl -X POST http://lambda-customer-url/api/customer/anonymous | jq -r '.token')

# 2. Criar pedido para Customer A
ORDER_ID=$(curl -X POST http://orderhub-url/api/order/start \
  -H "Authorization: Bearer $TOKEN_A" \
  -H "Content-Type: application/json" \
  -d '{"customerId": "customer-a-id"}' | jq -r '.data.orderId')

# 3. Adicionar produto ao pedido (deve funcionar)
curl -X POST http://orderhub-url/api/order/add-product \
  -H "Authorization: Bearer $TOKEN_A" \
  -H "Content-Type: application/json" \
  -d '{
    "orderId": "'$ORDER_ID'",
    "productId": "product-id",
    "quantity": 1
  }'

# Deve retornar 200 OK
```

### 2. Customer Tenta Acessar Pedido de Outro Customer

```bash
# 1. Obter token do Customer B
TOKEN_B=$(curl -X POST http://lambda-customer-url/api/customer/anonymous | jq -r '.token')

# 2. Tentar adicionar produto ao pedido do Customer A (usando token do Customer B)
curl -X POST http://orderhub-url/api/order/add-product \
  -H "Authorization: Bearer $TOKEN_B" \
  -H "Content-Type: application/json" \
  -d '{
    "orderId": "'$ORDER_ID'",
    "productId": "product-id",
    "quantity": 1
  }'

# Deve retornar 403 Forbidden
```

### 3. Customer Tenta Confirmar Pedido de Outro Customer

```bash
# Tentar confirmar pedido do Customer A usando token do Customer B
curl -X POST http://orderhub-url/api/order/$ORDER_ID/confirm-selection \
  -H "Authorization: Bearer $TOKEN_B"

# Deve retornar 403 Forbidden
```

### 4. Customer Tenta Atualizar Produto em Pedido de Outro Customer

```bash
# Tentar atualizar produto no pedido do Customer A usando token do Customer B
curl -X PUT http://orderhub-url/api/order/update-product \
  -H "Authorization: Bearer $TOKEN_B" \
  -H "Content-Type: application/json" \
  -d '{
    "orderId": "'$ORDER_ID'",
    "orderedProductId": "ordered-product-id",
    "quantity": 2
  }'

# Deve retornar 403 Forbidden
```

### 5. Customer Tenta Remover Produto de Pedido de Outro Customer

```bash
# Tentar remover produto do pedido do Customer A usando token do Customer B
curl -X DELETE http://orderhub-url/api/order/remove-product \
  -H "Authorization: Bearer $TOKEN_B" \
  -H "Content-Type: application/json" \
  -d '{
    "orderId": "'$ORDER_ID'",
    "orderedProductId": "ordered-product-id"
  }'

# Deve retornar 403 Forbidden
```

### 6. CustomerId do Body Diferente do Token

```bash
# Tentar iniciar pedido com CustomerId diferente do token
curl -X POST http://orderhub-url/api/order/start \
  -H "Authorization: Bearer $TOKEN_A" \
  -H "Content-Type: application/json" \
  -d '{"customerId": "outro-customer-id"}'

# Deve retornar 403 Forbidden
```

### 7. Admin Pode Acessar Qualquer Pedido

```bash
# Obter token Admin
ADMIN_TOKEN=$(curl -X POST http://lambda-admin-url/api/admin/login \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "senha"}' | jq -r '.accessToken')

# Admin pode acessar pedido de qualquer customer
curl -X GET http://orderhub-url/api/order/$ORDER_ID \
  -H "Authorization: Bearer $ADMIN_TOKEN"

# Deve retornar 200 OK
```

## Checklist

- [ ] Customer pode acessar seu próprio pedido
- [ ] Customer não pode acessar pedido de outro customer (403)
- [ ] Customer não pode confirmar pedido de outro customer (403)
- [ ] Customer não pode atualizar produto em pedido de outro customer (403)
- [ ] Customer não pode remover produto de pedido de outro customer (403)
- [ ] CustomerId do body deve corresponder ao do token (403 se diferente)
- [ ] Admin pode acessar qualquer pedido
- [ ] Retorna 401 quando token está ausente
- [ ] Retorna 403 (não 401) quando ownership não corresponde

## Observações

1. **Códigos HTTP:**
   - 401 Unauthorized: Token ausente, inválido ou expirado
   - 403 Forbidden: Token válido, mas sem permissão (ownership)

2. **Validação:**
   - Deve ser feita antes de executar a operação
   - Deve consultar o pedido no banco para verificar CustomerId

3. **Performance:**
   - Cada validação faz uma query adicional
   - Considerar cache se necessário (futuro)



