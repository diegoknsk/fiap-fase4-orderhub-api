# Subtask 13: Validar Compatibilidade com Lambdas

## Objetivo
Validar que a implementação funciona corretamente com tokens gerados pelos lambdas de autenticação (`fiap-fase4-auth-lambda`).

## Pré-requisitos

1. **Lambda Customer rodando:**
   - Endpoint: `POST /api/customer/anonymous`
   - Endpoint: `POST /api/customer/register`
   - Endpoint: `POST /api/customer/identify`

2. **Lambda Admin rodando:**
   - Endpoint: `POST /api/admin/login`

3. **Configurações alinhadas:**
   - `JwtSettings:Secret` deve ser a mesma do Lambda Customer
   - `Cognito:UserPoolId` e `Cognito:ClientId` devem corresponder ao Lambda Admin

## Testes de Compatibilidade

### 1. Teste com Token Customer (CustomerBearer)

#### 1.1. Obter Token do Lambda Customer
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

#### 1.2. Validar Estrutura do Token
- Decodificar token em https://jwt.io
- Verificar claims:
  - `sub`: CustomerId (Guid)
  - `customerId`: CustomerId (Guid)
  - `jti`: JWT ID (Guid)
  - `iat`: Issued At (Unix timestamp)
  - `iss`: Issuer (deve corresponder a `JwtSettings:Issuer`)
  - `aud`: Audience (deve corresponder a `JwtSettings:Audience`)

#### 1.3. Testar Endpoint OrderHub com Token Customer
```bash
# Iniciar pedido
curl -X POST http://orderhub-url/api/order/start \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {token}" \
  -d '{"customerId": "guid-do-token"}'

# Deve retornar 201 Created
```

### 2. Teste com Token Cognito (Admin)

#### 2.1. Obter Token do Lambda Admin
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

#### 2.2. Validar Estrutura do Access Token
- Decodificar Access Token em https://jwt.io
- Verificar claims:
  - `token_use`: "access"
  - `client_id`: Deve corresponder a `Cognito:ClientId`
  - `username`: Username do admin
  - `scope`: Deve conter "aws.cognito.signin.user.admin"

#### 2.3. Testar Endpoint OrderHub com Token Cognito
```bash
# Listar produtos
curl -X GET http://orderhub-url/api/products \
  -H "Authorization: Bearer {accessToken}"

# Deve retornar 200 OK com lista de produtos
```

## Checklist de Validação

### Token Customer
- [ ] Token gerado pelo Lambda Customer é aceito
- [ ] Claims `sub`, `customerId`, `jti`, `iat` estão presentes
- [ ] `iss` corresponde a `JwtSettings:Issuer`
- [ ] `aud` corresponde a `JwtSettings:Audience`
- [ ] Assinatura do token é válida (mesma chave secreta)
- [ ] Token expirado retorna 401
- [ ] Token com assinatura inválida retorna 401

### Token Cognito
- [ ] Access Token gerado pelo Lambda Admin é aceito
- [ ] `token_use` é "access"
- [ ] `client_id` corresponde ao configurado
- [ ] `scope` contém "aws.cognito.signin.user.admin"
- [ ] Token expirado retorna 401
- [ ] Token inválido retorna 401

### Endpoints
- [ ] Endpoints Admin funcionam apenas com token Cognito
- [ ] Endpoints Customer funcionam apenas com token CustomerBearer
- [ ] Tentativa de usar token errado retorna 403
- [ ] Validação de ownership funciona corretamente

## Problemas Comuns e Soluções

### 1. Token Customer não é aceito
**Causa:** Chave secreta diferente entre Lambda Customer e OrderHub
**Solução:** Verificar que `JwtSettings:Secret` é a mesma em ambos os projetos

### 2. Token Cognito retorna 401
**Causa:** `client_id` não corresponde
**Solução:** Verificar que `Cognito:ClientId` corresponde ao usado no Lambda Admin

### 3. Token Cognito retorna "Token não é Access Token"
**Causa:** Usando ID Token ao invés de Access Token
**Solução:** Usar `accessToken` da resposta do Lambda Admin, não `idToken`

### 4. Validação de ownership falha
**Causa:** CustomerId do token não corresponde ao CustomerId do pedido
**Solução:** Verificar que o CustomerId extraído do token está correto

## Validações
- [ ] Todos os testes de compatibilidade passam
- [ ] Tokens do Lambda Customer funcionam
- [ ] Tokens do Lambda Admin funcionam
- [ ] Documentação atualizada com instruções de teste


