using FastFood.OrderHub.Application.DTOs;
using FastFood.OrderHub.Application.Ports;
using FastFood.OrderHub.Infra.Persistence.Repositories;

namespace FastFood.OrderHub.Infra.Persistence.DataSources;

/// <summary>
/// DataSource DynamoDB para pedidos
/// </summary>
public class OrderDynamoDbDataSource : IOrderDataSource
{
    private readonly OrderDynamoDbRepository _repository;

    public OrderDynamoDbDataSource(OrderDynamoDbRepository repository)
    {
        _repository = repository;
    }

    public async Task<OrderDto?> GetByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<List<OrderDto>> GetByCustomerIdAsync(Guid customerId)
    {
        return await _repository.GetByCustomerIdAsync(customerId);
    }

    public async Task<List<OrderDto>> GetPagedAsync(int page, int pageSize, int? status = null)
    {
        var (items, _) = await _repository.GetPagedAsync(page, pageSize, status);
        return items;
    }

    public async Task<List<OrderDto>> GetByStatusAsync(int status)
    {
        return await _repository.GetByStatusAsync(status);
    }

    public async Task<List<OrderDto>> GetByStatusWithoutPreparationAsync(int status)
    {
        return await _repository.GetByStatusWithoutPreparationAsync(status);
    }

    public async Task<Guid> AddAsync(OrderDto dto)
    {
        if (dto.Id == Guid.Empty)
            dto.Id = Guid.NewGuid();

        if (dto.CreatedAt == default)
            dto.CreatedAt = DateTime.UtcNow;

        await _repository.AddAsync(dto);
        return dto.Id;
    }

    public async Task UpdateAsync(OrderDto dto)
    {
        await _repository.UpdateAsync(dto);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _repository.ExistsAsync(id);
    }

    public async Task<bool> ExistsByCodeAsync(string code)
    {
        return await _repository.ExistsByCodeAsync(code);
    }

    public async Task<string> GenerateOrderCodeAsync()
    {
        return await _repository.GenerateOrderCodeAsync();
    }
}

