namespace FastFood.OrderHub.Application.Exceptions;

/// <summary>
/// Exceção para erros de negócio que devem ser retornados ao cliente
/// </summary>
public class BusinessException : Exception
{
    public BusinessException(string message) : base(message)
    {
    }

    public BusinessException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
