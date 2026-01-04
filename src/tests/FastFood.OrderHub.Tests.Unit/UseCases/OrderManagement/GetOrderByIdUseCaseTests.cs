using FastFood.OrderHub.Application.DTOs;
using FastFood.OrderHub.Application.Exceptions;
using FastFood.OrderHub.Application.InputModels.OrderManagement;
using FastFood.OrderHub.Application.Ports;
using FastFood.OrderHub.Application.Presenters.OrderManagement;
using FastFood.OrderHub.Application.UseCases.OrderManagement;
using FastFood.OrderHub.Domain.Common.Enums;
using Moq;
using Xunit;

namespace FastFood.OrderHub.Tests.Unit.UseCases.OrderManagement;

/// <summary>
/// Testes unitários para GetOrderByIdUseCase
/// </summary>
public class GetOrderByIdUseCaseTests
{
    private readonly Mock<IOrderDataSource> _orderDataSourceMock;
    private readonly Mock<IRequestContext> _requestContextMock;
    private readonly GetOrderByIdPresenter _presenter;
    private readonly GetOrderByIdUseCase _useCase;

    public GetOrderByIdUseCaseTests()
    {
        _orderDataSourceMock = new Mock<IOrderDataSource>();
        _requestContextMock = new Mock<IRequestContext>();
        _presenter = new GetOrderByIdPresenter();
        _useCase = new GetOrderByIdUseCase(_orderDataSourceMock.Object, _presenter, _requestContextMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOrderExists_AsAdmin_ShouldReturnOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var orderDto = new OrderDto
        {
            Id = orderId,
            Code = "ORD-001",
            CustomerId = customerId,
            CreatedAt = DateTime.UtcNow,
            OrderStatus = (int)EnumOrderStatus.Started,
            TotalPrice = 50.00m,
            Items = new List<OrderedProductDto>()
        };

        var input = new GetOrderByIdInputModel
        {
            OrderId = orderId
        };

        _requestContextMock.Setup(x => x.IsAdmin).Returns(true);
        _orderDataSourceMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(orderDto);

        // Act
        var result = await _useCase.ExecuteAsync(input);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(orderId, result.OrderId);
        Assert.Equal(orderDto.Code, result.Code);
        Assert.Equal(customerId, result.CustomerId);
        Assert.Equal(orderDto.OrderStatus, result.OrderStatus);
        Assert.Equal(orderDto.TotalPrice, result.TotalPrice);
        Assert.Empty(result.Items);
        _orderDataSourceMock.Verify(x => x.GetByIdAsync(orderId), Times.Once);
        _orderDataSourceMock.Verify(x => x.GetByIdForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOrderDoesNotExist_AsAdmin_ShouldThrowBusinessException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var input = new GetOrderByIdInputModel
        {
            OrderId = orderId
        };

        _requestContextMock.Setup(x => x.IsAdmin).Returns(true);
        _orderDataSourceMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync((OrderDto?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() => _useCase.ExecuteAsync(input));
        Assert.Equal("Pedido não encontrado.", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOrderHasItems_AsAdmin_ShouldReturnOrderWithItems()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
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
                    ProductId = productId,
                    OrderId = orderId,
                    Quantity = 2,
                    FinalPrice = 25.00m,
                    ProductName = "Test Product",
                    Category = 1,
                    CustomIngredients = new List<OrderedProductIngredientDto>()
                }
            }
        };

        var input = new GetOrderByIdInputModel
        {
            OrderId = orderId
        };

        _requestContextMock.Setup(x => x.IsAdmin).Returns(true);
        _orderDataSourceMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(orderDto);

        // Act
        var result = await _useCase.ExecuteAsync(input);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(productId, result.Items[0].ProductId);
        Assert.Equal(2, result.Items[0].Quantity);
        Assert.Equal(25.00m, result.Items[0].FinalPrice);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOrderHasCustomIngredients_AsAdmin_ShouldMapCustomIngredients()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();
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
                    ProductId = productId,
                    OrderId = orderId,
                    Quantity = 2,
                    FinalPrice = 25.00m,
                    ProductName = "Test Product",
                    Category = 1,
                    Observation = "Test observation",
                    CustomIngredients = new List<OrderedProductIngredientDto>
                    {
                        new OrderedProductIngredientDto
                        {
                            Id = ingredientId,
                            Name = "Extra Cheese",
                            Price = 2.50m,
                            Quantity = 2
                        }
                    }
                }
            }
        };

        var input = new GetOrderByIdInputModel
        {
            OrderId = orderId
        };

        _requestContextMock.Setup(x => x.IsAdmin).Returns(true);
        _orderDataSourceMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(orderDto);

        // Act
        var result = await _useCase.ExecuteAsync(input);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Single(result.Items[0].CustomIngredients);
        Assert.Equal(ingredientId, result.Items[0].CustomIngredients[0].Id);
        Assert.Equal("Extra Cheese", result.Items[0].CustomIngredients[0].Name);
        Assert.Equal(2.50m, result.Items[0].CustomIngredients[0].Price);
        Assert.Equal(2, result.Items[0].CustomIngredients[0].Quantity);
    }

    #region Customer Tests

    [Fact]
    public async Task ExecuteAsync_WhenOrderExists_AsCustomer_ShouldReturnOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var orderDto = new OrderDto
        {
            Id = orderId,
            Code = "ORD-001",
            CustomerId = customerId,
            CreatedAt = DateTime.UtcNow,
            OrderStatus = (int)EnumOrderStatus.Started,
            TotalPrice = 50.00m,
            Items = new List<OrderedProductDto>()
        };

        var input = new GetOrderByIdInputModel
        {
            OrderId = orderId
        };

        _requestContextMock.Setup(x => x.IsAdmin).Returns(false);
        _requestContextMock.Setup(x => x.CustomerId).Returns(customerId.ToString());
        _orderDataSourceMock
            .Setup(x => x.GetByIdForCustomerAsync(orderId, customerId))
            .ReturnsAsync(orderDto);

        // Act
        var result = await _useCase.ExecuteAsync(input);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(orderId, result.OrderId);
        Assert.Equal(orderDto.Code, result.Code);
        Assert.Equal(customerId, result.CustomerId);
        Assert.Equal(orderDto.OrderStatus, result.OrderStatus);
        Assert.Equal(orderDto.TotalPrice, result.TotalPrice);
        Assert.Empty(result.Items);
        _orderDataSourceMock.Verify(x => x.GetByIdForCustomerAsync(orderId, customerId), Times.Once);
        _orderDataSourceMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOrderDoesNotExist_AsCustomer_ShouldThrowBusinessException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var input = new GetOrderByIdInputModel
        {
            OrderId = orderId
        };

        _requestContextMock.Setup(x => x.IsAdmin).Returns(false);
        _requestContextMock.Setup(x => x.CustomerId).Returns(customerId.ToString());
        _orderDataSourceMock
            .Setup(x => x.GetByIdForCustomerAsync(orderId, customerId))
            .ReturnsAsync((OrderDto?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() => _useCase.ExecuteAsync(input));
        Assert.Equal("Pedido não encontrado.", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOrderBelongsToDifferentCustomer_AsCustomer_ShouldThrowBusinessException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var differentCustomerId = Guid.NewGuid();
        var input = new GetOrderByIdInputModel
        {
            OrderId = orderId
        };

        _requestContextMock.Setup(x => x.IsAdmin).Returns(false);
        _requestContextMock.Setup(x => x.CustomerId).Returns(customerId.ToString());
        _orderDataSourceMock
            .Setup(x => x.GetByIdForCustomerAsync(orderId, customerId))
            .ReturnsAsync((OrderDto?)null); // Retorna null porque o pedido pertence a outro cliente

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() => _useCase.ExecuteAsync(input));
        Assert.Equal("Pedido não encontrado.", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCustomerIdIsNull_AsCustomer_ShouldThrowBusinessException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var input = new GetOrderByIdInputModel
        {
            OrderId = orderId
        };

        _requestContextMock.Setup(x => x.IsAdmin).Returns(false);
        _requestContextMock.Setup(x => x.CustomerId).Returns((string?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() => _useCase.ExecuteAsync(input));
        Assert.Equal("CustomerId não encontrado no token.", exception.Message);
        _orderDataSourceMock.Verify(x => x.GetByIdForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCustomerIdIsEmpty_AsCustomer_ShouldThrowBusinessException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var input = new GetOrderByIdInputModel
        {
            OrderId = orderId
        };

        _requestContextMock.Setup(x => x.IsAdmin).Returns(false);
        _requestContextMock.Setup(x => x.CustomerId).Returns(string.Empty);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() => _useCase.ExecuteAsync(input));
        Assert.Equal("CustomerId não encontrado no token.", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCustomerIdIsInvalidGuid_AsCustomer_ShouldThrowBusinessException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var input = new GetOrderByIdInputModel
        {
            OrderId = orderId
        };

        _requestContextMock.Setup(x => x.IsAdmin).Returns(false);
        _requestContextMock.Setup(x => x.CustomerId).Returns("invalid-guid");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() => _useCase.ExecuteAsync(input));
        Assert.Equal("CustomerId inválido no token.", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOrderExists_AsCustomer_WithItems_ShouldReturnOrderWithItems()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var orderDto = new OrderDto
        {
            Id = orderId,
            Code = "ORD-001",
            CustomerId = customerId,
            CreatedAt = DateTime.UtcNow,
            OrderStatus = (int)EnumOrderStatus.Started,
            TotalPrice = 50.00m,
            Items = new List<OrderedProductDto>
            {
                new OrderedProductDto
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    OrderId = orderId,
                    Quantity = 2,
                    FinalPrice = 25.00m,
                    ProductName = "Test Product",
                    Category = 1,
                    CustomIngredients = new List<OrderedProductIngredientDto>()
                }
            }
        };

        var input = new GetOrderByIdInputModel
        {
            OrderId = orderId
        };

        _requestContextMock.Setup(x => x.IsAdmin).Returns(false);
        _requestContextMock.Setup(x => x.CustomerId).Returns(customerId.ToString());
        _orderDataSourceMock
            .Setup(x => x.GetByIdForCustomerAsync(orderId, customerId))
            .ReturnsAsync(orderDto);

        // Act
        var result = await _useCase.ExecuteAsync(input);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(productId, result.Items[0].ProductId);
        Assert.Equal(2, result.Items[0].Quantity);
        Assert.Equal(25.00m, result.Items[0].FinalPrice);
    }

    #endregion
}
