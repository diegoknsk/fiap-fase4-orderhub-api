using FastFood.OrderHub.Application.Ports;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace FastFood.OrderHub.Infra.Services;

/// <summary>
/// Implementação de IRequestContext que lê claims do HttpContext
/// </summary>
public class RequestContext : IRequestContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RequestContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsAdmin
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || user.Identity?.IsAuthenticated != true)
                return false;

            // Verifica se é token Cognito (access token)
            // Token Cognito tem claim "token_use" = "access" e "scope" contém "aws.cognito.signin.user.admin"
            var tokenUse = user.FindFirst("token_use")?.Value;
            var scope = user.FindFirst("scope")?.Value;

            return tokenUse == "access" && 
                   !string.IsNullOrWhiteSpace(scope) && 
                   scope.Contains("aws.cognito.signin.user.admin");
        }
    }

    public string? CustomerId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || user.Identity?.IsAuthenticated != true)
                return null;

            // Tenta obter customerId do claim "customerId" primeiro
            var customerIdClaim = user.FindFirst("customerId")?.Value;
            if (!string.IsNullOrWhiteSpace(customerIdClaim))
                return customerIdClaim;

            // Se não encontrar, tenta obter do claim "sub" (Subject)
            // que é usado no CustomerBearer JWT
            var subClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                          ?? user.FindFirst("sub")?.Value;

            return subClaim;
        }
    }

    public string? GetBearerToken()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return null;

        // Extrair token do header Authorization
        var authHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(authHeader))
            return null;

        // Remover prefixo "Bearer " se presente
        if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return authHeader.Substring(7);

        return authHeader;
    }
}
