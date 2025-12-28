namespace FastFood.OrderHub.Application.Models.Common;

/// <summary>
/// Extensões para objetos
/// </summary>
public static class ObjectExtensions
{
    /// <summary>
    /// Converte um objeto para formato nomeado (retorna o próprio objeto)
    /// </summary>
    public static object? ToNamedContent<T>(this T? obj)
    {
        return obj;
    }
}

