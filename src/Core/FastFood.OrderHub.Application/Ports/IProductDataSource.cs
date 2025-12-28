using FastFood.OrderHub.Application.DTOs;

namespace FastFood.OrderHub.Application.Ports;

/// <summary>
/// Interface para acesso a dados de produtos
/// </summary>
public interface IProductDataSource
{
    Task<ProductDto?> GetByIdAsync(Guid id);
    Task<List<ProductDto>> GetAvailableAsync();
    Task<List<ProductDto>> GetByCategoryAsync(int category);
    Task<bool> ExistsAsync(Guid id);
    Task AddAsync(ProductDto dto);
    Task UpdateAsync(ProductDto dto);
    Task RemoveAsync(Guid id);
}

