using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FastFood.OrderHub.Application.DTOs.Payment;
using FastFood.OrderHub.Application.Ports;
using FastFood.OrderHub.Infra.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FastFood.OrderHub.Infra.Integrations;

/// <summary>
/// Cliente HTTP para integração com serviço de pagamento PayStream
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
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<CreatePaymentResponse> CreatePaymentAsync(
        CreatePaymentRequest request,
        string bearerToken,
        CancellationToken cancellationToken = default)
    {
        var attempt = 0;
        var maxAttempts = _options.RetryEnabled ? _options.RetryCount : 1;

        while (attempt < maxAttempts)
        {
            attempt++;

            try
            {
                _logger.LogInformation(
                    "Tentando criar pagamento no PayStream para pedido {OrderId} (tentativa {Attempt}/{MaxAttempts})",
                    request.OrderId, attempt, maxAttempts);

                // Converter request para o formato esperado pelo PayStream (orderSnapshot como string JSON)
                var payStreamRequest = request.ToPayStreamRequest();

                // Criar HttpRequestMessage com header Authorization
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/Payment/create")
                {
                    Content = JsonContent.Create(payStreamRequest, options: JsonOptions)
                };
                httpRequest.Headers.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);

                // Fazer chamada HTTP POST
                var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

                // Tratar resposta
                if (response.IsSuccessStatusCode)
                {
                    var paymentResponse = await response.Content.ReadFromJsonAsync<CreatePaymentResponse>(
                        JsonOptions,
                        cancellationToken);

                    if (paymentResponse == null)
                    {
                        _logger.LogError(
                            "Resposta do PayStream para pedido {OrderId} está vazia ou inválida",
                            request.OrderId);
                        throw new HttpRequestException(
                            $"Resposta inválida do PayStream para pedido {request.OrderId}");
                    }

                    _logger.LogInformation(
                        "Pagamento criado com sucesso no PayStream. PaymentId: {PaymentId}, OrderId: {OrderId}",
                        paymentResponse.PaymentId, request.OrderId);

                    return paymentResponse;
                }

                // Tratar erros HTTP
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning(
                    "PayStream retornou erro HTTP {StatusCode} para pedido {OrderId}. Conteúdo: {ErrorContent}",
                    response.StatusCode, request.OrderId, errorContent);

                // Se for erro 4xx (exceto 401), não tentar novamente
                if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.NotFound or HttpStatusCode.Forbidden)
                {
                    throw new HttpRequestException(
                        $"Erro ao criar pagamento no PayStream: {response.StatusCode}. {errorContent}",
                        null,
                        response.StatusCode);
                }

                // Se for erro 401, não tentar novamente
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new HttpRequestException(
                        $"Token inválido ou expirado. PayStream retornou 401 Unauthorized para pedido {request.OrderId}",
                        null,
                        response.StatusCode);
                }

                // Para outros erros (5xx, timeout, etc.), tentar novamente se retry estiver habilitado
                if (attempt < maxAttempts)
                {
                    _logger.LogWarning(
                        "Tentando novamente criar pagamento para pedido {OrderId} após erro {StatusCode}",
                        request.OrderId, response.StatusCode);
                    await Task.Delay(1000, cancellationToken); // Aguardar 1 segundo antes de tentar novamente
                    continue;
                }

                throw new HttpRequestException(
                    $"Erro ao criar pagamento no PayStream após {maxAttempts} tentativa(s): {response.StatusCode}. {errorContent}",
                    null,
                    response.StatusCode);
            }
            catch (HttpRequestException)
            {
                // Re-throw HttpRequestException (já tratada acima)
                throw;
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                _logger.LogError(ex, "Timeout ao criar pagamento no PayStream para pedido {OrderId}", request.OrderId);
                
                if (attempt < maxAttempts)
                {
                    _logger.LogWarning(
                        "Tentando novamente criar pagamento para pedido {OrderId} após timeout",
                        request.OrderId);
                    await Task.Delay(1000, cancellationToken);
                    continue;
                }

                throw new HttpRequestException(
                    $"Timeout ao criar pagamento no PayStream para pedido {request.OrderId} após {maxAttempts} tentativa(s)",
                    ex,
                    HttpStatusCode.RequestTimeout);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Erro inesperado ao criar pagamento no PayStream para pedido {OrderId}",
                    request.OrderId);

                if (attempt < maxAttempts)
                {
                    _logger.LogWarning(
                        "Tentando novamente criar pagamento para pedido {OrderId} após erro inesperado",
                        request.OrderId);
                    await Task.Delay(1000, cancellationToken);
                    continue;
                }

                throw new HttpRequestException(
                    $"Erro inesperado ao criar pagamento no PayStream para pedido {request.OrderId}",
                    ex);
            }
        }

        // Não deveria chegar aqui, mas por segurança
        throw new HttpRequestException(
            $"Falha ao criar pagamento no PayStream para pedido {request.OrderId} após {maxAttempts} tentativa(s)");
    }
}
