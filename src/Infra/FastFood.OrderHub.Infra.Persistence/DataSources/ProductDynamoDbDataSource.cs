using FastFood.OrderHub.Application.DTOs;
using FastFood.OrderHub.Application.Ports;
using FastFood.OrderHub.Infra.Persistence.Repositories;

namespace FastFood.OrderHub.Infra.Persistence.DataSources;

/// <summary>
/// DataSource DynamoDB para produtos
/// </summary>
public class ProductDynamoDbDataSource : IProductDataSource
{
    private readonly ProductDynamoDbRepository _repository;

    public ProductDynamoDbDataSource(ProductDynamoDbRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<List<ProductDto>> GetAvailableAsync()
    {
        var allProducts = await _repository.GetAllAsync();
        return allProducts.Where(p => p.IsActive).ToList();
    }

    public async Task<List<ProductDto>> GetByCategoryAsync(int category)
    {
        var products = await _repository.GetByCategoryAsync(category);
        return products.Where(p => p.IsActive).ToList();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _repository.ExistsAsync(id);
    }

    public async Task AddAsync(ProductDto dto)
    {
        dto.IsActive = true;
        dto.CreatedAt = DateTime.UtcNow;
        await _repository.AddAsync(dto);
    }

    public async Task UpdateAsync(ProductDto dto)
    {
        await _repository.UpdateAsync(dto);
    }

    public async Task RemoveAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
    }
}

