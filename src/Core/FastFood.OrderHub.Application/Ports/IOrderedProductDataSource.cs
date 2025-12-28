using FastFood.OrderHub.Application.DTOs;

namespace FastFood.OrderHub.Application.Ports;

/// <summary>
/// Interface para acesso a dados de produtos pedidos
/// Nota: Esta interface será removida na migração para DynamoDB, pois Items são parte do Order
/// </summary>
public interface IOrderedProductDataSource
{
    Task<OrderedProductDto?> GetByIdAsync(Guid id);
    Task<List<OrderedProductDto>> GetByOrderIdAsync(Guid orderId);
    Task<Guid> AddAsync(OrderedProductDto dto);
    Task UpdateAsync(OrderedProductDto dto);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}

