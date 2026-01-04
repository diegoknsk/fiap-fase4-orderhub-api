# Subtask 08: Modificar ConfirmOrderSelectionUseCase

## Objetivo
Modificar o `ConfirmOrderSelectionUseCase` para orquestrar a chamada ao PayStream após finalizar o pedido, seguindo a decisão arquitetural de finalizar o pedido primeiro e depois tentar criar o pagamento.

## Arquivo a Modificar

### `src/Core/FastFood.OrderHub.Application/UseCases/OrderManagement/ConfirmOrderSelectionUseCase.cs`

Adicionar dependências e lógica de integração:

```csharp
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

        // Finalizar seleção usando método de domínio
        order.FinalizeOrderSelection();

        // Converter entidade de domínio de volta para DTO
        orderDto = ConvertToDto(order, orderDto.OrderSource);

        // Salvar Order atualizado (PRIORIDADE: salvar antes de chamar PayStream)
        await _orderDataSource.UpdateAsync(orderDto);

        // Tentar criar pagamento no PayStream (após salvar pedido)
        await TryCreatePaymentInPayStreamAsync(order);

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
                    "Bearer token não encontrado. Pagamento não será criado para pedido {OrderId}",
                    order.Id);
                return;
            }

            // Construir snapshot do pedido
            var snapshot = OrderSnapshotBuilder.BuildFromOrder(order);

            // Validar que snapshot não está vazio
            if (snapshot.Items == null || !snapshot.Items.Any())
            {
                _logger.LogWarning(
                    "Snapshot do pedido está vazio. Pagamento não será criado para pedido {OrderId}",
                    order.Id);
                return;
            }

            // Criar request para PayStream
            var paymentRequest = new CreatePaymentRequest
            {
                OrderId = order.Id,
                TotalAmount = order.TotalPrice,
                OrderSnapshot = snapshot
            };

            _logger.LogInformation(
                "Tentando criar pagamento no PayStream para pedido {OrderId}",
                order.Id);

            // Chamar PayStream
            var paymentResponse = await _paymentServiceClient.CreatePaymentAsync(
                paymentRequest,
                bearerToken);

            _logger.LogInformation(
                "Pagamento criado com sucesso no PayStream. OrderId: {OrderId}, PaymentId: {PaymentId}",
                order.Id,
                paymentResponse.PaymentId);
        }
        catch (HttpRequestException ex)
        {
            // Logar erro mas não quebrar o fluxo (pedido já está finalizado)
            _logger.LogError(
                ex,
                "Erro ao criar pagamento no PayStream para pedido {OrderId}. Pedido permanece finalizado.",
                order.Id);
            
            // Re-throw para que o controller possa retornar 502
            throw new BusinessException(
                "Erro ao iniciar pagamento. O pedido foi finalizado, mas o pagamento não pôde ser criado. Tente novamente mais tarde.",
                ex);
        }
        catch (Exception ex)
        {
            // Logar erro inesperado mas não quebrar o fluxo
            _logger.LogError(
                ex,
                "Erro inesperado ao criar pagamento no PayStream para pedido {OrderId}. Pedido permanece finalizado.",
                order.Id);
            
            // Re-throw para que o controller possa retornar 502
            throw new BusinessException(
                "Erro ao iniciar pagamento. O pedido foi finalizado, mas o pagamento não pôde ser criado. Tente novamente mais tarde.",
                ex);
        }
    }

    // ... métodos privados existentes (ConvertToDomainEntity, ConvertToDto, AdaptToOutputModel) ...
}
```

## Arquivo a Modificar (Registro no DI)

### `src/InterfacesExternas/FastFood.OrderHub.Api/Program.cs`

Adicionar `ILogger` ao registro do UseCase (já deve estar registrado, mas verificar):

```csharp
// Registrar UseCases (OrderManagement)
builder.Services.AddScoped<FastFood.OrderHub.Application.UseCases.OrderManagement.ConfirmOrderSelectionUseCase>();
```

## Validações

- [ ] Dependências adicionadas: `IPaymentServiceClient`, `IRequestContext`, `ILogger`
- [ ] Método `TryCreatePaymentInPayStreamAsync` é privado e assíncrono
- [ ] Pedido é salvo ANTES de chamar PayStream
- [ ] Se token Bearer não estiver presente, loga warning mas não falha
- [ ] Se snapshot estiver vazio, loga warning mas não falha
- [ ] Se PayStream falhar, loga erro mas não quebra o fluxo de finalização
- [ ] Exceção é re-thrown como `BusinessException` para controller tratar
- [ ] Logs são informativos e incluem OrderId
- [ ] Código compila sem erros

## Observações

- **Decisão arquitetural:** Pedido é finalizado primeiro, depois tenta criar pagamento
- Se PayStream falhar, pedido permanece finalizado (status `AwaitingPayment`)
- Controller deve tratar `BusinessException` e retornar 502 Bad Gateway
- Logs devem ser suficientes para debug mas não expor dados sensíveis
