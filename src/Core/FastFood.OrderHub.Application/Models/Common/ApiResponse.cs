namespace FastFood.OrderHub.Application.Models.Common;

/// <summary>
/// Modelo padronizado de resposta da API
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public object? Content { get; set; }

    public ApiResponse(object? content, string? message = "Requisição bem-sucedida.", bool success = true)
    {
        Content = content;
        Message = message;
        Success = success;
    }

    public static ApiResponse<T> Ok(T? data, string? message = "Requisição bem-sucedida.")
        => new(data.ToNamedContent(), message, true);

    public static ApiResponse<T> Ok(string? message = "Requisição bem-sucedida.")
        => new(null, message, true);

    public static ApiResponse<T> Fail(string? message)
        => new(null, message, false);
}


