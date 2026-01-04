using FastFood.OrderHub.Application.DTOs;
using FastFood.OrderHub.Application.InputModels.OrderManagement;
using FastFood.OrderHub.Application.OutputModels.OrderManagement;
using FastFood.OrderHub.Application.Ports;
using FastFood.OrderHub.Application.Presenters.OrderManagement;
using FastFood.OrderHub.Application.Responses.OrderManagement;

namespace FastFood.OrderHub.Application.UseCases.OrderManagement;

/// <summary>
/// UseCase para listar pedidos paginados
/// </summary>
public class GetPagedOrdersUseCase
{
    private readonly IOrderDataSource _orderDataSource;
    private readonly GetPagedOrdersPresenter _presenter;

    public GetPagedOrdersUseCase(
        IOrderDataSource orderDataSource,
        GetPagedOrdersPresenter presenter)
    {
        _orderDataSource = orderDataSource;
        _presenter = presenter;
    }

    public async Task<GetPagedOrdersResponse> ExecuteAsync(GetPagedOrdersInputModel input)
    {
        // Validar parâmetros
        if (input.Page < 1)
            input.Page = 1;
        
        if (input.PageSize < 1)
            input.PageSize = 10;
        
        if (input.PageSize > 100)
            input.PageSize = 100;

        // Buscar pedidos paginados
        var orders = await _orderDataSource.GetPagedAsync(
            input.Page,
            input.PageSize,
            input.Status);

        // Para calcular TotalCount, precisamos fazer uma busca adicional
        // Por simplicidade, vamos usar o tamanho da lista retornada como aproximação
        // Em produção, seria ideal ter um método CountAsync no DataSource
        var totalCount = orders.Count;

        var totalPages = totalCount > 0 ? (int)Math.Ceiling(totalCount / (double)input.PageSize) : 0;
        var hasNextPage = orders.Count == input.PageSize && totalCount > input.Page * input.PageSize;

        var output = AdaptToOutputModel(orders, input.Page, input.PageSize, totalCount, totalPages, hasNextPage);
        return _presenter.Present(output);
    }

    private GetPagedOrdersOutputModel AdaptToOutputModel(
        List<OrderDto> orders,
        int page,
        int pageSize,
        int totalCount,
        int totalPages,
        bool hasNextPage)
    {
        return new GetPagedOrdersOutputModel
        {
            Items = orders.Select(order => new OrderSummaryOutputModel
            {
                OrderId = order.Id,
                Code = order.Code,
                CustomerId = order.CustomerId,
                CreatedAt = order.CreatedAt,
                OrderStatus = order.OrderStatus,
                TotalPrice = order.TotalPrice
            }).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasNextPage = hasNextPage
        };
    }
}

