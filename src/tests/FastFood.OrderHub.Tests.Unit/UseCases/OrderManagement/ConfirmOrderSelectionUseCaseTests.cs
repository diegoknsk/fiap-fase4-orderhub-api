using FastFood.OrderHub.Application.DTOs;
using FastFood.OrderHub.Application.DTOs.Payment;
using FastFood.OrderHub.Application.Exceptions;
using FastFood.OrderHub.Application.InputModels.OrderManagement;
using FastFood.OrderHub.Application.Ports;
using FastFood.OrderHub.Application.Presenters.OrderManagement;
using FastFood.OrderHub.Application.UseCases.OrderManagement;
using FastFood.OrderHub.Domain.Common.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using Xunit;

namespace FastFood.OrderHub.Tests.Unit.UseCases.OrderManagement;

/// <summary>
/// Testes unitários para ConfirmOrderSelectionUseCase
/// </summary>
public class ConfirmOrderSelectionUseCaseTests
{
    private readonly Mock<IOrderDataSource> _orderDataSourceMock;
    private readonly Mock<IPaymentServiceClient> _paymentServiceClientMock;
    private readonly Mock<IRequestContext> _requestContextMock;
    private readonly Mock<ILogger<ConfirmOrderSelectionUseCase>> _loggerMock;
    private readonly ConfirmOrderSelectionPresenter _presenter;
    private readonly ConfirmOrderSelectionUseCase _useCase;

    public ConfirmOrderSelectionUseCaseTests()
    {
        _orderDataSourceMock = new Mock<IOrderDataSource>();
        _paymentServiceClientMock = new Mock<IPaymentServiceClient>();
        _requestContextMock = new Mock<IRequestContext>();
        _loggerMock = new Mock<ILogger<ConfirmOrderSelectionUseCase>>();
        _presenter = new ConfirmOrderSelectionPresenter();
        
        // Configurar RequestContext para retornar token Bearer
        _requestContextMock.Setup(x => x.GetBearerToken()).Returns("test-bearer-token");
        
        _useCase = new ConfirmOrderSelectionUseCase(
            _orderDataSourceMock.Object, 
            _presenter,
            _paymentServiceClientMock.Object,
            _requestContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenValidOrder_ShouldConfirmOrderSelection()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderDto = new OrderDto
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
                    CustomIngredients = new List<OrderedProductIngredientDto>()
                }
            }
        };

        var input = new ConfirmOrderSelectionInputModel
        {
            OrderId = orderId
        };

        _orderDataSourceMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(orderDto);

        _orderDataSourceMock
            .Setup(x => x.UpdateAsync(It.IsAny<OrderDto>()))
            .Returns(Task.CompletedTask);

        // Mock PaymentServiceClient para retornar sucesso
        _paymentServiceClientMock
            .Setup(x => x.CreatePaymentAsync(It.IsAny<CreatePaymentRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CreatePaymentResponse
            {
                PaymentId = Guid.NewGuid(),
                Status = "Created",
                CreatedAt = DateTime.UtcNow
            });

        // Act
        var result = await _useCase.ExecuteAsync(input);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(orderId, result.OrderId);
        Assert.Equal((int)EnumOrderStatus.AwaitingPayment, result.OrderStatus);
        Assert.Equal(50.00m, result.TotalPrice);

        _orderDataSourceMock.Verify(
            x => x.GetByIdAsync(orderId),
            Times.Once);

        // Verificar que foi salvo com status AwaitingPayment
        _orderDataSourceMock.Verify(
            x => x.UpdateAsync(It.Is<OrderDto>(o =>
                o.Id == orderId &&
                o.OrderStatus == (int)EnumOrderStatus.AwaitingPayment)),
            Times.Once);

        // Verificar que PaymentServiceClient foi chamado
        _paymentServiceClientMock.Verify(
            x => x.CreatePaymentAsync(
                It.IsAny<CreatePaymentRequest>(),
                "test-bearer-token",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPaymentServiceFails_ShouldRevertOrderStatusToStarted()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderDto = new OrderDto
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
                    CustomIngredients = new List<OrderedProductIngredientDto>()
                }
            }
        };

        var input = new ConfirmOrderSelectionInputModel
        {
            OrderId = orderId
        };

        _orderDataSourceMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(orderDto);

        _orderDataSourceMock
            .Setup(x => x.UpdateAsync(It.IsAny<OrderDto>()))
            .Returns(Task.CompletedTask);

        // Mock PaymentServiceClient para lançar exceção
        _paymentServiceClientMock
            .Setup(x => x.CreatePaymentAsync(It.IsAny<CreatePaymentRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new System.Net.Http.HttpRequestException("Erro ao criar pagamento no PayStream: BadRequest.", null, HttpStatusCode.BadRequest));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() => _useCase.ExecuteAsync(input));
        Assert.Contains("iniciar pagamento", exception.Message);

        // Verificar que foi salvo duas vezes:
        // 1. Com status AwaitingPayment (antes de chamar PayStream)
        // 2. Com status Started (após falha do PayStream - rollback)
        _orderDataSourceMock.Verify(
            x => x.UpdateAsync(It.Is<OrderDto>(o =>
                o.Id == orderId &&
                o.OrderStatus == (int)EnumOrderStatus.AwaitingPayment)),
            Times.Once);

        _orderDataSourceMock.Verify(
            x => x.UpdateAsync(It.Is<OrderDto>(o =>
                o.Id == orderId &&
                o.OrderStatus == (int)EnumOrderStatus.Started)),
            Times.Once);

        // Verificar que PaymentServiceClient foi chamado
        _paymentServiceClientMock.Verify(
            x => x.CreatePaymentAsync(
                It.IsAny<CreatePaymentRequest>(),
                "test-bearer-token",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPaymentServiceThrowsHttpRequestException_ShouldRevertOrderStatus()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderDto = new OrderDto
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
                    CustomIngredients = new List<OrderedProductIngredientDto>()
                }
            }
        };

        var input = new ConfirmOrderSelectionInputModel
        {
            OrderId = orderId
        };

        _orderDataSourceMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(orderDto);

        _orderDataSourceMock
            .Setup(x => x.UpdateAsync(It.IsAny<OrderDto>()))
            .Returns(Task.CompletedTask);

        // Mock PaymentServiceClient para lançar HttpRequestException
        _paymentServiceClientMock
            .Setup(x => x.CreatePaymentAsync(It.IsAny<CreatePaymentRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new System.Net.Http.HttpRequestException("Timeout", null, HttpStatusCode.RequestTimeout));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() => _useCase.ExecuteAsync(input));
        Assert.Contains("iniciar pagamento", exception.Message);

        // Verificar rollback
        _orderDataSourceMock.Verify(
            x => x.UpdateAsync(It.Is<OrderDto>(o =>
                o.Id == orderId &&
                o.OrderStatus == (int)EnumOrderStatus.Started)),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOrderDoesNotExist_ShouldThrowBusinessException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var input = new ConfirmOrderSelectionInputModel
        {
            OrderId = orderId
        };

        _orderDataSourceMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync((OrderDto?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() => _useCase.ExecuteAsync(input));
        Assert.Equal("Pedido não encontrado.", exception.Message);
        _orderDataSourceMock.Verify(x => x.UpdateAsync(It.IsAny<OrderDto>()), Times.Never);
        _paymentServiceClientMock.Verify(x => x.CreatePaymentAsync(It.IsAny<CreatePaymentRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOrderHasNoItems_ShouldThrowBusinessException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderDto = new OrderDto
        {
            Id = orderId,
            Code = "ORD-001",
            CustomerId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            OrderStatus = (int)EnumOrderStatus.Started,
            TotalPrice = 0,
            Items = new List<OrderedProductDto>()
        };

        var input = new ConfirmOrderSelectionInputModel
        {
            OrderId = orderId
        };

        _orderDataSourceMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(orderDto);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() => _useCase.ExecuteAsync(input));
        Assert.Equal("Não é possível confirmar um pedido sem itens.", exception.Message);
        _orderDataSourceMock.Verify(x => x.UpdateAsync(It.IsAny<OrderDto>()), Times.Never);
        _paymentServiceClientMock.Verify(x => x.CreatePaymentAsync(It.IsAny<CreatePaymentRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOrderStatusIsNotStarted_ShouldThrowBusinessException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderDto = new OrderDto
        {
            Id = orderId,
            Code = "ORD-001",
            CustomerId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            OrderStatus = (int)EnumOrderStatus.AwaitingPayment,
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
                    CustomIngredients = new List<OrderedProductIngredientDto>()
                }
            }
        };

        var input = new ConfirmOrderSelectionInputModel
        {
            OrderId = orderId
        };

        _orderDataSourceMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(orderDto);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() => _useCase.ExecuteAsync(input));
        Assert.Equal("Apenas pedidos com status 'Started' podem ser confirmados.", exception.Message);
        _orderDataSourceMock.Verify(x => x.UpdateAsync(It.IsAny<OrderDto>()), Times.Never);
        _paymentServiceClientMock.Verify(x => x.CreatePaymentAsync(It.IsAny<CreatePaymentRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
