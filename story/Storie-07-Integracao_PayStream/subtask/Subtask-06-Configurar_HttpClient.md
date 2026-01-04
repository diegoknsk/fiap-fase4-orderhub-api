# Subtask 06: Configurar HttpClient no Program.cs

## Objetivo
Configurar o `HttpClient` para `PaymentServiceClient` via HttpClientFactory no `Program.cs`, seguindo as melhores práticas do .NET.

## Arquivo a Modificar

### `src/InterfacesExternas/FastFood.OrderHub.Api/Program.cs`

Adicionar as seguintes configurações:

```csharp
using FastFood.OrderHub.Application.Ports;
using FastFood.OrderHub.Infra.Configurations;
using FastFood.OrderHub.Infra.Integrations;

// ... código existente ...

// Configurar HttpClient para PaymentServiceClient
builder.Services.AddHttpClient<IPaymentServiceClient, PaymentServiceClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<PaymentServiceOptions>>().Value;
    
    if (string.IsNullOrWhiteSpace(options.BaseUrl))
        throw new InvalidOperationException("PaymentService:BaseUrl não está configurado");

    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("User-Agent", "FastFood.OrderHub/1.0");
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    // Permitir redirects se necessário
    AllowAutoRedirect = false,
    // Usar certificados SSL padrão
    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => 
    {
        // Em desenvolvimento, pode aceitar certificados inválidos
        // Em produção, deve validar corretamente
        if (builder.Environment.IsDevelopment())
            return true;
        return errors == System.Net.Security.SslPolicyErrors.None;
    }
});

// Configurar Options para PaymentService
builder.Services.Configure<PaymentServiceOptions>(
    builder.Configuration.GetSection("PaymentService"));

// Registrar PaymentServiceClient (já registrado via AddHttpClient, mas garantir)
builder.Services.AddScoped<IPaymentServiceClient, PaymentServiceClient>();
```

## Validações

- [ ] `AddHttpClient` é chamado com tipos corretos (`IPaymentServiceClient`, `PaymentServiceClient`)
- [ ] `BaseAddress` é configurado a partir de `PaymentServiceOptions.BaseUrl`
- [ ] `Timeout` é configurado a partir de `PaymentServiceOptions.TimeoutSeconds`
- [ ] Header `Accept: application/json` é adicionado
- [ ] Header `User-Agent` é adicionado
- [ ] `PaymentServiceOptions` é configurado via `Configure<PaymentServiceOptions>`
- [ ] Validação de `BaseUrl` não vazio é feita
- [ ] `HttpClientHandler` é configurado (SSL validation para dev/prod)
- [ ] Código compila sem erros

## Observações

- `AddHttpClient<TClient, TImplementation>` registra automaticamente o tipo no DI
- HttpClientFactory gerencia o ciclo de vida do HttpClient (evita socket exhaustion)
- SSL validation deve ser mais rigorosa em produção
- `User-Agent` ajuda no debugging e monitoramento
