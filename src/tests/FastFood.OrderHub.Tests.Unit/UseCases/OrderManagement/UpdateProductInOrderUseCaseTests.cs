using FastFood.OrderHub.Application.DTOs;
using FastFood.OrderHub.Application.InputModels.OrderManagement;
using FastFood.OrderHub.Application.Ports;
using FastFood.OrderHub.Application.Presenters.OrderManagement;
using FastFood.OrderHub.Application.UseCases.OrderManagement;
using FastFood.OrderHub.Domain.Common.Enums;
using Moq;
using Xunit;

namespace FastFood.OrderHub.Tests.Unit.UseCases.OrderManagement;

/// <summary>
/// Testes unitários para UpdateProductInOrderUseCase
/// </summary>
public class UpdateProductInOrderUseCaseTests
{
    private readonly Mock<IOrderDataSource> _orderDataSourceMock;
    private readonly Mock<IProductDataSource> _productDataSourceMock;
    private readonly UpdateProductInOrderPresenter _presenter;
    private readonly UpdateProductInOrderUseCase _useCase;

    public UpdateProductInOrderUseCaseTests()
    {
        _orderDataSourceMock = new Mock<IOrderDataSource>();
        _productDataSourceMock = new Mock<IProductDataSource>();
        _presenter = new UpdateProductInOrderPresenter();
        _useCase = new UpdateProductInOrderUseCase(
            _orderDataSourceMock.Object,
            _productDataSourceMock.Object,
            _presenter);
    }

    [Fact]
    public async Task ExecuteAsync_WhenValidInput_ShouldUpdateProductInOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderedProductId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var orderDto = new OrderDto
        {
            Id = orderId,
            Code = "ORD-001",
            CustomerId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            OrderStatus = (int)EnumOrderStatus.Started,
            TotalPrice = 25.00m,
            Items = new List<OrderedProductDto>
            {
                new OrderedProductDto
                {
                    Id = orderedProductId,
                    ProductId = productId,
                    OrderId = orderId,
                    Quantity = 1,
                    FinalPrice = 25.00m,
                    CustomIngredients = new List<OrderedProductIngredientDto>()
                }
            }
        };

        var productDto = new ProductDto
        {
            Id = productId,
            Name = "Test Product",
            Category = 1,
            Price = 10.50m,
            Description = "Test Description",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            BaseIngredients = new List<ProductBaseIngredientDto>()
        };

        var input = new UpdateProductInOrderInputModel
        {
            OrderId = orderId,
            OrderedProductId = orderedProductId,
            Quantity = 3,
            Observation = "Updated observation"
        };

        _orderDataSourceMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(orderDto);

        _productDataSourceMock
            .Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync(productDto);

        _orderDataSourceMock
            .Setup(x => x.UpdateAsync(It.IsAny<OrderDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.ExecuteAsync(input);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(orderId, result.OrderId);
        Assert.Equal(orderedProductId, result.OrderedProductId);
        Assert.True(result.TotalPrice > 0);

        _orderDataSourceMock.Verify(
            x => x.GetByIdAsync(orderId),
            Times.Once);

        _productDataSourceMock.Verify(
            x => x.GetByIdAsync(productId),
            Times.Once);

        _orderDataSourceMock.Verify(
            x => x.UpdateAsync(It.Is<OrderDto>(o =>
                o.Id == orderId &&
                o.Items.Count == 1 &&
                o.Items[0].Id == orderedProductId &&
                o.Items[0].Quantity == 3)),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenQuantityIsZero_ShouldThrowArgumentException()
    {
        // Arrange
        var input = new UpdateProductInOrderInputModel
        {
            OrderId = Guid.NewGuid(),
            OrderedProductId = Guid.NewGuid(),
            Quantity = 0
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _useCase.ExecuteAsync(input));
        _orderDataSourceMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOrderDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var input = new UpdateProductInOrderInputModel
        {
            OrderId = orderId,
            OrderedProductId = Guid.NewGuid(),
            Quantity = 2
        };

        _orderDataSourceMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync((OrderDto?)null);

        // Act
        var result = await _useCase.ExecuteAsync(input);

        // Assert
        Assert.Null(result);
        _productDataSourceMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOrderedProductDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderedProductId = Guid.NewGuid();

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

        var input = new UpdateProductInOrderInputModel
        {
            OrderId = orderId,
            OrderedProductId = orderedProductId,
            Quantity = 2
        };

        _orderDataSourceMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(orderDto);

        // Act
        var result = await _useCase.ExecuteAsync(input);

        // Assert
        Assert.Null(result);
        _orderDataSourceMock.Verify(x => x.UpdateAsync(It.IsAny<OrderDto>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenProductDoesNotExist_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderedProductId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var orderDto = new OrderDto
        {
            Id = orderId,
            Code = "ORD-001",
            CustomerId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            OrderStatus = (int)EnumOrderStatus.Started,
            TotalPrice = 25.00m,
            Items = new List<OrderedProductDto>
            {
                new OrderedProductDto
                {
                    Id = orderedProductId,
                    ProductId = productId,
                    OrderId = orderId,
                    Quantity = 1,
                    FinalPrice = 25.00m,
                    CustomIngredients = new List<OrderedProductIngredientDto>()
                }
            }
        };

        var input = new UpdateProductInOrderInputModel
        {
            OrderId = orderId,
            OrderedProductId = orderedProductId,
            Quantity = 2
        };

        _orderDataSourceMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(orderDto);

        _productDataSourceMock
            .Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync((ProductDto?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _useCase.ExecuteAsync(input));
    }

    [Fact]
    public async Task ExecuteAsync_WithCustomIngredients_ShouldUpdateCustomIngredients()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderedProductId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var baseIngredientId = Guid.NewGuid();

        var orderDto = new OrderDto
        {
            Id = orderId,
            Code = "ORD-001",
            CustomerId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            OrderStatus = (int)EnumOrderStatus.Started,
            TotalPrice = 25.00m,
            Items = new List<OrderedProductDto>
            {
                new OrderedProductDto
                {
                    Id = orderedProductId,
                    ProductId = productId,
                    OrderId = orderId,
                    Quantity = 1,
                    FinalPrice = 25.00m,
                    CustomIngredients = new List<OrderedProductIngredientDto>()
                }
            }
        };

        var productDto = new ProductDto
        {
            Id = productId,
            Name = "Test Product",
            Category = 1,
            Price = 10.50m,
            Description = "Test Description",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            BaseIngredients = new List<ProductBaseIngredientDto>
            {
                new ProductBaseIngredientDto
                {
                    Id = baseIngredientId,
                    Name = "Base Ingredient",
                    Price = 2.00m,
                    ProductId = productId
                }
            }
        };

        var input = new UpdateProductInOrderInputModel
        {
            OrderId = orderId,
            OrderedProductId = orderedProductId,
            Quantity = 2,
            Observation = "Updated observation",
            CustomIngredients = new List<CustomIngredientInputModel>
            {
                new CustomIngredientInputModel
                {
                    ProductBaseIngredientId = baseIngredientId,
                    Quantity = 5
                }
            }
        };

        _orderDataSourceMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(orderDto);

        _productDataSourceMock
            .Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync(productDto);

        _orderDataSourceMock
            .Setup(x => x.UpdateAsync(It.IsAny<OrderDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.ExecuteAsync(input);

        // Assert
        Assert.NotNull(result);
        _orderDataSourceMock.Verify(
            x => x.UpdateAsync(It.Is<OrderDto>(o =>
                o.Items[0].CustomIngredients.Count == 1 &&
                o.Items[0].CustomIngredients[0].ProductBaseIngredientId == baseIngredientId &&
                o.Items[0].CustomIngredients[0].Quantity == 5)),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenObservationIsUpdated_ShouldUpdateObservation()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderedProductId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var orderDto = new OrderDto
        {
            Id = orderId,
            Code = "ORD-001",
            CustomerId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            OrderStatus = (int)EnumOrderStatus.Started,
            TotalPrice = 25.00m,
            Items = new List<OrderedProductDto>
            {
                new OrderedProductDto
                {
                    Id = orderedProductId,
                    ProductId = productId,
                    OrderId = orderId,
                    Quantity = 1,
                    FinalPrice = 25.00m,
                    Observation = "Original observation",
                    CustomIngredients = new List<OrderedProductIngredientDto>()
                }
            }
        };

        var productDto = new ProductDto
        {
            Id = productId,
            Name = "Test Product",
            Category = 1,
            Price = 10.50m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            BaseIngredients = new List<ProductBaseIngredientDto>()
        };

        var input = new UpdateProductInOrderInputModel
        {
            OrderId = orderId,
            OrderedProductId = orderedProductId,
            Quantity = 2,
            Observation = "Updated observation"
        };

        _orderDataSourceMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(orderDto);

        _productDataSourceMock
            .Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync(productDto);

        _orderDataSourceMock
            .Setup(x => x.UpdateAsync(It.IsAny<OrderDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.ExecuteAsync(input);

        // Assert
        Assert.NotNull(result);
        _orderDataSourceMock.Verify(
            x => x.UpdateAsync(It.Is<OrderDto>(o =>
                o.Items[0].Observation == "Updated observation")),
            Times.Once);
    }
}
