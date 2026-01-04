namespace FastFood.OrderHub.Application.Ports;

/// <summary>
/// Interface para contexto da requisição HTTP
/// </summary>
public interface IRequestContext
{
    /// <summary>
    /// Indica se o usuário autenticado é um administrador (Cognito)
    /// </summary>
    bool IsAdmin { get; }

    /// <summary>
    /// ID do cliente autenticado (CustomerBearer)
    /// </summary>
    string? CustomerId { get; }

    /// <summary>
    /// Obtém o token Bearer do header Authorization da requisição
    /// </summary>
    /// <returns>Token Bearer sem o prefixo "Bearer ", ou null se não estiver presente</returns>
    string? GetBearerToken();
}
