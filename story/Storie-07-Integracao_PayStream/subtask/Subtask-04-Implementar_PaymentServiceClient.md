# Subtask 04: Implementar PaymentServiceClient

## Objetivo
Implementar a classe `PaymentServiceClient` que realiza a chamada HTTP ao PayStream, seguindo o padrão Adapter da Clean Architecture.

## Arquivo a Criar

### `src/Infra/FastFood.OrderHub.Infra/Integrations/PaymentServiceClient.cs`

```csharp
using FastFood.OrderHub.Application.DTOs.Payment;
using FastFood.OrderHub.Application.Ports;
using FastFood.OrderHub.Infra.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace FastFood.OrderHub.Infra.Integrations;

/// <summary>
/// Implementação do cliente HTTP para comunicação com PayStream
/// </summary>
public class PaymentServiceClient : IPaymentServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly PaymentServiceOptions _options;
    private readonly ILogger<PaymentServiceClient> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public PaymentServiceClient(
        HttpClient httpClient,
        IOptions<PaymentServiceOptions> options,
        ILogger<PaymentServiceClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CreatePaymentResponse> CreatePaymentAsync(
        CreatePaymentRequest request,
        string bearerToken,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(bearerToken))
            throw new ArgumentException("Bearer token não pode ser vazio", nameof(bearerToken));

        try
        {
            // Configurar header Authorization
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);

            // Construir URL
            var url = $"{_options.BaseUrl.TrimEnd('/')}/api/Payment/create";

            _logger.LogInformation(
                "Chamando PayStream para criar pagamento. OrderId: {OrderId}, URL: {Url}",
                request.OrderId,
                url);

            // Fazer chamada HTTP
            var response = await _httpClient.PostAsJsonAsync(
                url,
                request,
                JsonOptions,
                cancellationToken);

            // Verificar se foi sucesso
            if (response.IsSuccessStatusCode)
            {
                var paymentResponse = await response.Content.ReadFromJsonAsync<CreatePaymentResponse>(
                    JsonOptions,
                    cancellationToken);

                if (paymentResponse == null)
                {
                    _logger.LogError("Resposta do PayStream é null para OrderId: {OrderId}", request.OrderId);
                    throw new HttpRequestException("Resposta inválida do PayStream");
                }

                _logger.LogInformation(
                    "Pagamento criado com sucesso. OrderId: {OrderId}, PaymentId: {PaymentId}",
                    request.OrderId,
                    paymentResponse.PaymentId);

                return paymentResponse;
            }

            // Tratar erros HTTP
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "Erro ao criar pagamento no PayStream. OrderId: {OrderId}, StatusCode: {StatusCode}, Response: {Response}",
                request.OrderId,
                response.StatusCode,
                errorContent);

            throw new HttpRequestException(
                $"Erro ao criar pagamento no PayStream. StatusCode: {response.StatusCode}",
                null,
                response.StatusCode);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Timeout ao criar pagamento no PayStream. OrderId: {OrderId}", request.OrderId);
            throw new HttpRequestException("Timeout ao comunicar com PayStream", ex);
        }
        catch (HttpRequestException)
        {
            // Re-throw para manter a exceção original
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao criar pagamento no PayStream. OrderId: {OrderId}", request.OrderId);
            throw new HttpRequestException("Erro inesperado ao comunicar com PayStream", ex);
        }
        finally
        {
            // Limpar header Authorization para evitar vazamento entre requisições
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }
}
```

## Validações

- [ ] Classe criada no namespace correto: `FastFood.OrderHub.Infra.Integrations`
- [ ] Classe implementa `IPaymentServiceClient`
- [ ] Construtor valida dependências (não null)
- [ ] Método `CreatePaymentAsync` valida `request` e `bearerToken`
- [ ] Token Bearer é adicionado no header Authorization
- [ ] URL é construída corretamente (BaseUrl + /api/Payment/create)
- [ ] Request é serializado como JSON com camelCase
- [ ] Response é deserializado corretamente
- [ ] Erros HTTP são tratados e logados
- [ ] Timeout é tratado especificamente
- [ ] Header Authorization é limpo no finally
- [ ] Logs são informativos e não expõem dados sensíveis
- [ ] Código compila sem erros

## Observações

- Esta é a **Adapter** no padrão Port/Adapter
- `HttpClient` é injetado via DI (configurado no Program.cs)
- `JsonSerializerOptions` usa camelCase para compatibilidade com PayStream
- Header Authorization é limpo no `finally` para evitar vazamento entre requisições
- Logs devem ser informativos mas não expor dados sensíveis (ex: não logar token completo)
