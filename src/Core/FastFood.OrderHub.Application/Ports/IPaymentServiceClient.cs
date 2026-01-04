using FastFood.OrderHub.Application.DTOs.Payment;

namespace FastFood.OrderHub.Application.Ports;

/// <summary>
/// Port para integração com serviço de pagamento (PayStream)
/// </summary>
public interface IPaymentServiceClient
{
    /// <summary>
    /// Cria um intent de pagamento no PayStream
    /// </summary>
    /// <param name="request">Dados do pagamento</param>
    /// <param name="bearerToken">Token Bearer do cliente para repassar ao PayStream</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Response com informações do pagamento criado</returns>
    Task<CreatePaymentResponse> CreatePaymentAsync(
        CreatePaymentRequest request, 
        string bearerToken, 
        CancellationToken cancellationToken = default);
}
