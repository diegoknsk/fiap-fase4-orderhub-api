namespace FastFood.OrderHub.Infra.Configurations;

/// <summary>
/// Opções de configuração para o serviço de pagamento (PayStream)
/// </summary>
public class PaymentServiceOptions
{
    public const string SectionName = "PaymentService";

    /// <summary>
    /// URL base do serviço PayStream
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Timeout em segundos para requisições HTTP
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Habilitar retry simples (1 tentativa adicional)
    /// </summary>
    public bool RetryEnabled { get; set; } = false;

    /// <summary>
    /// Número de tentativas (incluindo a primeira)
    /// </summary>
    public int RetryCount { get; set; } = 1;
}
