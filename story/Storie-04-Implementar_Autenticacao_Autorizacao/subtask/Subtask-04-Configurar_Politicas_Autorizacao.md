# Subtask 04: Configurar Políticas de Autorização

## Objetivo
Configurar políticas de autorização (Admin e Customer) no Program.cs para uso nos atributos `[Authorize]`.

## Modificações em Program.cs

### Adicionar Configuração de Authorization

Após a configuração de autenticação, adicionar:

```csharp
// Configure authorization policies
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
        // policy.RequireClaim("customerId");
    });
});
```

## Detalhamento das Políticas

### Política "Admin"
- **Requisito:** Usuário autenticado
- **Claim obrigatória:** `scope` com valor `"aws.cognito.signin.user.admin"`
- **Uso:** Endpoints de administração (ProductsController, alguns endpoints do OrderController)
- **Esquema:** Cognito

### Política "Customer"
- **Requisito:** Usuário autenticado
- **Uso:** Endpoints de customer (maioria dos endpoints do OrderController)
- **Esquema:** CustomerBearer
- **Observação:** Validação adicional de ownership será feita nos controllers

## Validações
- [ ] Política "Admin" configurada
- [ ] Política "Customer" configurada
- [ ] Política Admin requer claim "scope"
- [ ] Código compila sem erros

## Observações

1. **Scope Claim:**
   - A claim `scope` com valor `"aws.cognito.signin.user.admin"` é adicionada automaticamente pelo Cognito para usuários autenticados
   - Garante que apenas admins autenticados via Cognito podem acessar endpoints Admin

2. **Política Customer:**
   - Apenas requer autenticação
   - Validação adicional de ownership será implementada nos controllers


