# Subtask 10: Testar Autenticação CustomerBearer

## Objetivo
Testar que a autenticação CustomerBearer funciona corretamente, validando tokens JWT gerados pelo Lambda Customer.

## Pré-requisitos

1. Lambda Customer rodando e acessível
2. OrderHub API rodando
3. Configuração `JwtSettings` alinhada entre Lambda Customer e OrderHub

## Testes

### 1. Obter Token do Lambda Customer

```bash
# Criar customer anônimo
curl -X POST http://lambda-customer-url/api/customer/anonymous \
  -H "Content-Type: application/json"

# Resposta esperada:
# {
#   "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
#   "customerId": "guid",
#   ...
# }
```

### 2. Validar Token

Decodificar token em https://jwt.io e verificar:
- [ ] Claims `sub`, `customerId`, `jti`, `iat` presentes
- [ ] `iss` corresponde a `JwtSettings:Issuer`
- [ ] `aud` corresponde a `JwtSettings:Audience`
- [ ] Assinatura válida

### 3. Testar Endpoints com Token Válido

```bash
# Iniciar pedido
curl -X POST http://orderhub-url/api/order/start \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {token}" \
  -d '{"customerId": "guid-do-token"}'

# Deve retornar 201 Created
```

### 4. Testar Endpoints sem Token

```bash
curl -X POST http://orderhub-url/api/order/start \
  -H "Content-Type: application/json" \
  -d '{"customerId": "guid"}'

# Deve retornar 401 Unauthorized
```

### 5. Testar Endpoints com Token Expirado

- Aguardar expiração do token ou usar token expirado
- Deve retornar 401 Unauthorized

### 6. Testar Endpoints com Token Inválido

```bash
curl -X POST http://orderhub-url/api/order/start \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer token-invalido" \
  -d '{"customerId": "guid"}'

# Deve retornar 401 Unauthorized
```

### 7. Testar Endpoints Admin com Token Customer

```bash
curl -X GET http://orderhub-url/api/products \
  -H "Authorization: Bearer {token-customer}"

# Deve retornar 403 Forbidden
```

## Checklist

- [ ] Token válido é aceito
- [ ] Token ausente retorna 401
- [ ] Token expirado retorna 401
- [ ] Token inválido retorna 401
- [ ] Token Customer não funciona em endpoints Admin (403)
- [ ] Claims do token estão corretas
- [ ] Assinatura do token é validada corretamente



