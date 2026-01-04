using FastFood.OrderHub.Application.DTOs;
using FastFood.OrderHub.Application.DTOs.Payment;
using FastFood.OrderHub.Application.Exceptions;
using FastFood.OrderHub.Application.InputModels.OrderManagement;
using FastFood.OrderHub.Application.OutputModels.OrderManagement;
using FastFood.OrderHub.Application.Ports;
using FastFood.OrderHub.Application.Presenters.OrderManagement;
using FastFood.OrderHub.Application.Responses.OrderManagement;
using FastFood.OrderHub.Application.Services;
using FastFood.OrderHub.Domain.Common.Enums;
using FastFood.OrderHub.Domain.Entities.OrderManagement;
using Microsoft.Extensions.Logging;

namespace FastFood.OrderHub.Application.UseCases.OrderManagement;

/// <summary>
/// UseCase para confirmar seleção do pedido
/// </summary>
public class ConfirmOrderSelectionUseCase
{
    private readonly IOrderDataSource _orderDataSource;
    private readonly ConfirmOrderSelectionPresenter _presenter;
    private readonly IPaymentServiceClient _paymentServiceClient;
    private readonly IRequestContext _requestContext;
    private readonly ILogger<ConfirmOrderSelectionUseCase> _logger;

    public ConfirmOrderSelectionUseCase(
        IOrderDataSource orderDataSource,
        ConfirmOrderSelectionPresenter presenter,
        IPaymentServiceClient paymentServiceClient,
        IRequestContext requestContext,
        ILogger<ConfirmOrderSelectionUseCase> logger)
    {
        _orderDataSource = orderDataSource;
        _presenter = presenter;
        _paymentServiceClient = paymentServiceClient;
        _requestContext = requestContext;
        _logger = logger;
    }

    public async Task<ConfirmOrderSelectionResponse> ExecuteAsync(ConfirmOrderSelectionInputModel input)
    {
        // Buscar pedido completo com Items
        var orderDto = await _orderDataSource.GetByIdAsync(input.OrderId);
        if (orderDto == null)
            throw new BusinessException("Pedido não encontrado.");

        // Converter OrderDto para entidade de domínio Order
        var order = ConvertToDomainEntity(orderDto);

        // Validar que o pedido tem itens
        if (!order.OrderedProducts.Any())
            throw new BusinessException("Não é possível confirmar um pedido sem itens.");

        // Validar que o pedido está no status correto
        if (order.OrderStatus != EnumOrderStatus.Started)
            throw new BusinessException("Apenas pedidos com status 'Started' podem ser confirmados.");

        // Guardar status original para possível rollback
        var originalStatus = order.OrderStatus;

        // Finalizar seleção usando método de domínio
        order.FinalizeOrderSelection();

        // Converter entidade de domínio de volta para DTO
        orderDto = ConvertToDto(order, orderDto.OrderSource);

        // Salvar Order atualizado (PRIORIDADE: salvar antes de chamar PayStream)
        await _orderDataSource.UpdateAsync(orderDto);

        // Tentar criar pagamento no PayStream (após salvar pedido)
        try
        {
            await TryCreatePaymentInPayStreamAsync(order);
        }
        catch (BusinessException)
        {
            // Se falhar, reverter status para Started e salvar novamente
            _logger.LogWarning(
                "Revertendo status do pedido {OrderId} de {CurrentStatus} para {OriginalStatus} devido a falha no PayStream",
                order.Id, order.OrderStatus, originalStatus);
            
            order.UpdateStatus(originalStatus);
            orderDto = ConvertToDto(order, orderDto.OrderSource);
            await _orderDataSource.UpdateAsync(orderDto);
            
            // Re-lançar a exceção
            throw;
        }

        var output = AdaptToOutputModel(order);
        return _presenter.Present(output);
    }

    private async Task TryCreatePaymentInPayStreamAsync(Order order)
    {
        try
        {
            var bearerToken = _requestContext.GetBearerToken();
            if (string.IsNullOrWhiteSpace(bearerToken))
            {
                _logger.LogWarning(
                    "Bearer token não encontrado na requisição. Pagamento não será criado no PayStream para pedido {OrderId}",
                    order.Id);
                return;
            }

            // Construir snapshot do pedido (sem PII)
            var snapshot = OrderSnapshotBuilder.BuildFromOrder(order);
            
            // Validar que snapshot não está vazio
            if (snapshot.Items == null || !snapshot.Items.Any())
            {
                _logger.LogWarning(
                    "Snapshot do pedido {OrderId} está vazio ou sem itens. Pagamento não será criado no PayStream",
                    order.Id);
                return;
            }

            var paymentRequest = new CreatePaymentRequest
            {
                OrderId = order.Id,
                TotalAmount = order.TotalPrice,
                OrderSnapshot = snapshot
            };

            await _paymentServiceClient.CreatePaymentAsync(paymentRequest, bearerToken);
            _logger.LogInformation(
                "Pagamento criado com sucesso no PayStream para pedido {OrderId}",
                order.Id);
        }
        catch (HttpRequestException ex)
        {
            // Logar erro detalhadamente
            _logger.LogError(ex,
                "Erro ao criar pagamento no PayStream para pedido {OrderId}. StatusCode: {StatusCode}",
                order.Id, ex.Data.Contains("StatusCode") ? ex.Data["StatusCode"] : "Unknown");
            
            // Lançar exceção para que o UseCase possa reverter o status
            throw new BusinessException("Erro ao iniciar pagamento. Tente novamente mais tarde.");
        }
        catch (Exception ex)
        {
            // Logar erro inesperado
            _logger.LogError(ex,
                "Erro inesperado ao criar pagamento no PayStream para pedido {OrderId}",
                order.Id);
            
            // Lançar exceção para que o UseCase possa reverter o status
            throw new BusinessException("Erro ao iniciar pagamento. Tente novamente mais tarde.");
        }
    }

    private Order ConvertToDomainEntity(OrderDto dto)
    {
        var order = new Order
        {
            Id = dto.Id,
            Code = dto.Code,
            CustomerId = dto.CustomerId,
            CreatedAt = dto.CreatedAt,
            OrderStatus = (EnumOrderStatus)dto.OrderStatus,
            TotalPrice = dto.TotalPrice,
            OrderedProducts = dto.Items.Select(item => new OrderedProduct
            {
                Id = item.Id,
                ProductId = item.ProductId,
                OrderId = item.OrderId,
                Quantity = item.Quantity,
                Observation = item.Observation,
                FinalPrice = item.FinalPrice,
                CustomIngredients = item.CustomIngredients.Select(ci => new OrderedProductIngredient
                {
                    Id = ci.Id,
                    Name = ci.Name,
                    Price = ci.Price,
                    Quantity = ci.Quantity,
                    OrderedProductId = ci.OrderedProductId,
                    ProductBaseIngredientId = ci.ProductBaseIngredientId
                }).ToList()
            }).ToList()
        };

        return order;
    }

    private OrderDto ConvertToDto(Order order, string? orderSource = null)
    {
        return new OrderDto
        {
            Id = order.Id,
            Code = order.Code,
            CustomerId = order.CustomerId,
            CreatedAt = order.CreatedAt,
            OrderStatus = (int)order.OrderStatus,
            TotalPrice = order.TotalPrice,
            OrderSource = orderSource,
            Items = order.OrderedProducts.Select(op => new OrderedProductDto
            {
                Id = op.Id,
                ProductId = op.ProductId,
                OrderId = op.OrderId,
                Quantity = op.Quantity,
                Observation = op.Observation,
                FinalPrice = op.FinalPrice,
                ProductName = op.Product?.Name,
                Category = op.Product != null ? (int)op.Product.Category : null,
                CustomIngredients = op.CustomIngredients.Select(ci => new OrderedProductIngredientDto
                {
                    Id = ci.Id,
                    Name = ci.Name,
                    Price = ci.Price,
                    Quantity = ci.Quantity,
                    OrderedProductId = ci.OrderedProductId,
                    ProductBaseIngredientId = ci.ProductBaseIngredientId
                }).ToList()
            }).ToList()
        };
    }

    private ConfirmOrderSelectionOutputModel AdaptToOutputModel(Order order)
    {
        return new ConfirmOrderSelectionOutputModel
        {
            OrderId = order.Id,
            OrderStatus = (int)order.OrderStatus,
            TotalPrice = order.TotalPrice
        };
    }
}
