# Subtask 11: Testar Autenticação Cognito

## Objetivo
Testar que a autenticação Cognito funciona corretamente, validando Access Tokens gerados pelo Lambda Admin.

## Pré-requisitos

1. Lambda Admin rodando e acessível
2. OrderHub API rodando
3. Configuração `Cognito` alinhada entre Lambda Admin e OrderHub
4. Usuário admin criado no Cognito User Pool

## Testes

### 1. Obter Token do Lambda Admin

```bash
# Autenticar admin
curl -X POST http://lambda-admin-url/api/admin/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "senha"
  }'

# Resposta esperada:
# {
#   "accessToken": "eyJraWQiOiJcL0t...",
#   "idToken": "eyJraWQiOiJcL0t...",
#   "expiresIn": 3600,
#   "tokenType": "Bearer"
# }
```

**IMPORTANTE:** Usar `accessToken`, não `idToken`!

### 2. Validar Access Token

Decodificar Access Token em https://jwt.io e verificar:
- [ ] Claim `token_use` = "access"
- [ ] Claim `client_id` corresponde a `Cognito:ClientId`
- [ ] Claim `username` presente
- [ ] Claim `scope` contém "aws.cognito.signin.user.admin"

### 3. Testar Endpoints Admin com Token Válido

```bash
# Listar produtos
curl -X GET http://orderhub-url/api/products \
  -H "Authorization: Bearer {accessToken}"

# Deve retornar 200 OK com lista de produtos
```

### 4. Testar Endpoints sem Token

```bash
curl -X GET http://orderhub-url/api/products

# Deve retornar 401 Unauthorized
```

### 5. Testar Endpoints com Token Expirado

- Aguardar expiração do token ou usar token expirado
- Deve retornar 401 Unauthorized

### 6. Testar Endpoints com ID Token (errado)

```bash
# Usar idToken ao invés de accessToken
curl -X GET http://orderhub-url/api/products \
  -H "Authorization: Bearer {idToken}"

# Deve retornar 401 com mensagem "Token não é Access Token"
```

### 7. Testar Endpoints Customer com Token Cognito

```bash
curl -X POST http://orderhub-url/api/order/start \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {accessToken}" \
  -d '{"customerId": "guid"}'

# Deve retornar 403 Forbidden
```

### 8. Testar com Client ID Diferente

- Se possível, obter token de outro Client ID
- Deve retornar 401 com mensagem "client_id inválido para esta API"

## Checklist

- [ ] Access Token válido é aceito
- [ ] Token ausente retorna 401
- [ ] Token expirado retorna 401
- [ ] ID Token é rejeitado (token_use != "access")
- [ ] Token Cognito não funciona em endpoints Customer (403)
- [ ] Client ID é validado corretamente
- [ ] Scope é validado corretamente
- [ ] Authority é construído corretamente

## Problemas Comuns

1. **"Token não é Access Token":**
   - Causa: Usando ID Token ao invés de Access Token
   - Solução: Usar `accessToken` da resposta do Lambda Admin

2. **"client_id inválido":**
   - Causa: Client ID do token não corresponde ao configurado
   - Solução: Verificar `Cognito:ClientId` na configuração

3. **"Token sem claims":**
   - Causa: Token inválido ou malformado
   - Solução: Verificar que o token está correto



