using FastFood.OrderHub.Application.DTOs;

namespace FastFood.OrderHub.Application.Ports;

/// <summary>
/// Interface para acesso a dados de pedidos
/// </summary>
public interface IOrderDataSource
{
    Task<OrderDto?> GetByIdAsync(Guid id);
    Task<List<OrderDto>> GetByCustomerIdAsync(Guid customerId);
    Task<List<OrderDto>> GetPagedAsync(int page, int pageSize, int? status = null);
    Task<List<OrderDto>> GetByStatusAsync(int status);
    Task<List<OrderDto>> GetByStatusWithoutPreparationAsync(int status);
    Task<Guid> AddAsync(OrderDto dto);
    Task UpdateAsync(OrderDto dto);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> ExistsByCodeAsync(string code);
    Task<string> GenerateOrderCodeAsync();
}



