# Subtask 12: Testes Unitários ConfirmOrderSelectionUseCase

## Objetivo
Criar testes unitários para o `ConfirmOrderSelectionUseCase` modificado, validando que a integração com PayStream é chamada corretamente após finalizar o pedido.

## Arquivo a Modificar/Criar

### `src/tests/FastFood.OrderHub.Tests.Unit/UseCases/OrderManagement/ConfirmOrderSelectionUseCaseTests.cs`

Adicionar novos testes para integração com PayStream:

```csharp
using FastFood.OrderHub.Application.DTOs;
using FastFood.OrderHub.Application.DTOs.Payment;
using FastFood.OrderHub.Application.Exceptions;
using FastFood.OrderHub.Application.InputModels.OrderManagement;
using FastFood.OrderHub.Application.Ports;
using FastFood.OrderHub.Application.Presenters.OrderManagement;
using FastFood.OrderHub.Application.UseCases.OrderManagement;
using FastFood.OrderHub.Domain.Common.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace FastFood.OrderHub.Tests.Unit.UseCases.OrderManagement;

public class ConfirmOrderSelectionUseCaseTests
{
    // ... testes existentes ...

    [Fact]
    public async Task ExecuteAsync_WhenOrderIsFinalized_ShouldCallPaymentServiceClient()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderDto = CreateValidOrderDto(orderId);
        
        var orderDataSourceMock = new Mock<IOrderDataSource>();
        orderDataSourceMock.Setup(x => x.GetByIdAsync(orderId)).ReturnsAsync(orderDto);
        orderDataSourceMock.Setup(x => x.UpdateAsync(It.IsAny<OrderDto>())).Returns(Task.CompletedTask);

        var paymentServiceClientMock = new Mock<IPaymentServiceClient>();
        paymentServiceClientMock
            .Setup(x => x.CreatePaymentAsync(
                It.IsAny<CreatePaymentRequest>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CreatePaymentResponse
            {
                PaymentId = Guid.NewGuid(),
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            });

        var requestContextMock = new Mock<IRequestContext>();
        requestContextMock.Setup(x => x.GetBearerToken()).Returns("test-token");

        var loggerMock = new Mock<ILogger<ConfirmOrderSelectionUseCase>>();

        var useCase = new ConfirmOrderSelectionUseCase(
            orderDataSourceMock.Object,
            new ConfirmOrderSelectionPresenter(),
            paymentServiceClientMock.Object,
            requestContextMock.Object,
            loggerMock.Object);

        var input = new ConfirmOrderSelectionInputModel { OrderId = orderId };

        // Act
        var result = await useCase.ExecuteAsync(input);

        // Assert
        paymentServiceClientMock.Verify(
            x => x.CreatePaymentAsync(
                It.Is<CreatePaymentRequest>(r => r.OrderId == orderId),
                It.Is<string>(t => t == "test-token"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPaymentServiceFails_ShouldThrowBusinessException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderDto = CreateValidOrderDto(orderId);
        
        var orderDataSourceMock = new Mock<IOrderDataSource>();
        orderDataSourceMock.Setup(x => x.GetByIdAsync(orderId)).ReturnsAsync(orderDto);
        orderDataSourceMock.Setup(x => x.UpdateAsync(It.IsAny<OrderDto>())).Returns(Task.CompletedTask);

        var paymentServiceClientMock = new Mock<IPaymentServiceClient>();
        paymentServiceClientMock
            .Setup(x => x.CreatePaymentAsync(
                It.IsAny<CreatePaymentRequest>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("PayStream error"));

        var requestContextMock = new Mock<IRequestContext>();
        requestContextMock.Setup(x => x.GetBearerToken()).Returns("test-token");

        var loggerMock = new Mock<ILogger<ConfirmOrderSelectionUseCase>>();

        var useCase = new ConfirmOrderSelectionUseCase(
            orderDataSourceMock.Object,
            new ConfirmOrderSelectionPresenter(),
            paymentServiceClientMock.Object,
            requestContextMock.Object,
            loggerMock.Object);

        var input = new ConfirmOrderSelectionInputModel { OrderId = orderId };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => useCase.ExecuteAsync(input));
        
        exception.Message.Should().Contain("pagamento");
    }

    [Fact]
    public async Task ExecuteAsync_WhenBearerTokenIsMissing_ShouldNotCallPaymentServiceClient()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderDto = CreateValidOrderDto(orderId);
        
        var orderDataSourceMock = new Mock<IOrderDataSource>();
        orderDataSourceMock.Setup(x => x.GetByIdAsync(orderId)).ReturnsAsync(orderDto);
        orderDataSourceMock.Setup(x => x.UpdateAsync(It.IsAny<OrderDto>())).Returns(Task.CompletedTask);

        var paymentServiceClientMock = new Mock<IPaymentServiceClient>();

        var requestContextMock = new Mock<IRequestContext>();
        requestContextMock.Setup(x => x.GetBearerToken()).Returns((string?)null);

        var loggerMock = new Mock<ILogger<ConfirmOrderSelectionUseCase>>();

        var useCase = new ConfirmOrderSelectionUseCase(
            orderDataSourceMock.Object,
            new ConfirmOrderSelectionPresenter(),
            paymentServiceClientMock.Object,
            requestContextMock.Object,
            loggerMock.Object);

        var input = new ConfirmOrderSelectionInputModel { OrderId = orderId };

        // Act
        var result = await useCase.ExecuteAsync(input);

        // Assert
        paymentServiceClientMock.Verify(
            x => x.CreatePaymentAsync(
                It.IsAny<CreatePaymentRequest>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenOrderIsFinalized_ShouldSaveOrderBeforeCallingPayStream()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderDto = CreateValidOrderDto(orderId);
        
        var orderDataSourceMock = new Mock<IOrderDataSource>();
        orderDataSourceMock.Setup(x => x.GetByIdAsync(orderId)).ReturnsAsync(orderDto);
        
        var updateSequence = new List<string>();
        orderDataSourceMock
            .Setup(x => x.UpdateAsync(It.IsAny<OrderDto>()))
            .Callback(() => updateSequence.Add("UpdateOrder"))
            .Returns(Task.CompletedTask);

        var paymentServiceClientMock = new Mock<IPaymentServiceClient>();
        paymentServiceClientMock
            .Setup(x => x.CreatePaymentAsync(
                It.IsAny<CreatePaymentRequest>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Callback(() => updateSequence.Add("CallPayStream"))
            .ReturnsAsync(new CreatePaymentResponse());

        var requestContextMock = new Mock<IRequestContext>();
        requestContextMock.Setup(x => x.GetBearerToken()).Returns("test-token");

        var loggerMock = new Mock<ILogger<ConfirmOrderSelectionUseCase>>();

        var useCase = new ConfirmOrderSelectionUseCase(
            orderDataSourceMock.Object,
            new ConfirmOrderSelectionPresenter(),
            paymentServiceClientMock.Object,
            requestContextMock.Object,
            loggerMock.Object);

        var input = new ConfirmOrderSelectionInputModel { OrderId = orderId };

        // Act
        await useCase.ExecuteAsync(input);

        // Assert
        updateSequence.Should().ContainInOrder("UpdateOrder", "CallPayStream");
    }

    private OrderDto CreateValidOrderDto(Guid orderId)
    {
        return new OrderDto
        {
            Id = orderId,
            Code = "ORD-001",
            CustomerId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            OrderStatus = (int)EnumOrderStatus.Started,
            TotalPrice = 50.00m,
            Items = new List<OrderedProductDto>
            {
                new OrderedProductDto
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.NewGuid(),
                    OrderId = orderId,
                    Quantity = 2,
                    FinalPrice = 25.00m,
                    ProductName = "Hambúrguer",
                    CustomIngredients = new List<OrderedProductIngredientDto>()
                }
            }
        };
    }
}
```

## Validações

- [ ] Teste: ExecuteAsync chama PaymentServiceClient após finalizar pedido
- [ ] Teste: ExecuteAsync passa OrderId e BearerToken corretos
- [ ] Teste: ExecuteAsync lança BusinessException quando PayStream falha
- [ ] Teste: ExecuteAsync não chama PayStream quando token está ausente
- [ ] Teste: ExecuteAsync salva pedido ANTES de chamar PayStream
- [ ] Todos os testes passam
- [ ] Código compila sem erros

## Observações

- Validar ordem de execução (salvar pedido antes de chamar PayStream)
- Mockar todas as dependências
- Verificar que exceções são tratadas corretamente
- Testar edge cases (token ausente, PayStream falha, etc.)
