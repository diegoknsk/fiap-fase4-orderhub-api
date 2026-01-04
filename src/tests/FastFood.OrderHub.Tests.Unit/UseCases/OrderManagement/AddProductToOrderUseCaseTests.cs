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
/// Testes unitários para AddProductToOrderUseCase
/// </summary>
public class AddProductToOrderUseCaseTests
{
    private readonly Mock<IOrderDataSource> _orderDataSourceMock;
    private readonly Mock<IProductDataSource> _productDataSourceMock;
    private readonly AddProductToOrderPresenter _presenter;
    private readonly AddProductToOrderUseCase _useCase;

    public AddProductToOrderUseCaseTests()
    {
        _orderDataSourceMock = new Mock<IOrderDataSource>();
        _productDataSourceMock = new Mock<IProductDataSource>();
        _presenter = new AddProductToOrderPresenter();
        _useCase = new AddProductToOrderUseCase(
            _orderDataSourceMock.Object,
            _productDataSourceMock.Object,
            _presenter);
    }

    [Fact]
    public async Task ExecuteAsync_WhenValidInput_ShouldAddProductToOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var orderDto = new OrderDto
        {
            Id = orderId,
            Code = "ORD-001",
            CustomerId = customerId,
            CreatedAt = DateTime.UtcNow,
            OrderStatus = (int)EnumOrderStatus.Started,
            TotalPrice = 0,
            Items = new List<OrderedProductDto>()
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

        var input = new AddProductToOrderInputModel
        {
            OrderId = orderId,
            ProductId = productId,
            Quantity = 2,
            Observation = "Test observation"
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
        Assert.NotEqual(Guid.Empty, result.OrderedProductId);
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
                o.Items[0].ProductId == productId &&
                o.Items[0].Quantity == 2)),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenQuantityIsZero_ShouldThrowBusinessException()
    {
        // Arrange
        var input = new AddProductToOrderInputModel
        {
            OrderId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            Quantity = 0
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() => _useCase.ExecuteAsync(input));
        Assert.Equal("Quantidade deve ser maior que zero.", exception.Message);
        _orderDataSourceMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOrderDoesNotExist_ShouldThrowBusinessException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var input = new AddProductToOrderInputModel
        {
            OrderId = orderId,
            ProductId = Guid.NewGuid(),
            Quantity = 1
        };

        _orderDataSourceMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync((OrderDto?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() => _useCase.ExecuteAsync(input));
        Assert.Equal("Pedido não encontrado.", exception.Message);
        _productDataSourceMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenProductDoesNotExist_ShouldThrowBusinessException()
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
            TotalPrice = 0,
            Items = new List<OrderedProductDto>()
        };

        var input = new AddProductToOrderInputModel
        {
            OrderId = orderId,
            ProductId = productId,
            Quantity = 1
        };

        _orderDataSourceMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(orderDto);

        _productDataSourceMock
            .Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync((ProductDto?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() => _useCase.ExecuteAsync(input));
        Assert.Equal("Produto não encontrado.", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WhenProductIsInactive_ShouldThrowBusinessException()
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
            TotalPrice = 0,
            Items = new List<OrderedProductDto>()
        };

        var productDto = new ProductDto
        {
            Id = productId,
            Name = "Test Product",
            Category = 1,
            Price = 10.50m,
            IsActive = false,
            CreatedAt = DateTime.UtcNow,
            BaseIngredients = new List<ProductBaseIngredientDto>()
        };

        var input = new AddProductToOrderInputModel
        {
            OrderId = orderId,
            ProductId = productId,
            Quantity = 1
        };

        _orderDataSourceMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(orderDto);

        _productDataSourceMock
            .Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync(productDto);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() => _useCase.ExecuteAsync(input));
        Assert.Equal("Produto não está disponível.", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WhenProductHasCustomIngredients_ShouldAddCustomIngredients()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var baseIngredientId = Guid.NewGuid();

        var orderDto = new OrderDto
        {
            Id = orderId,
            Code = "ORD-001",
            CustomerId = customerId,
            CreatedAt = DateTime.UtcNow,
            OrderStatus = (int)EnumOrderStatus.Started,
            TotalPrice = 0,
            Items = new List<OrderedProductDto>()
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

        var input = new AddProductToOrderInputModel
        {
            OrderId = orderId,
            ProductId = productId,
            Quantity = 2,
            Observation = "Test observation",
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
        Assert.Equal(orderId, result.OrderId);
        Assert.NotEqual(Guid.Empty, result.OrderedProductId);
        Assert.True(result.TotalPrice > 0);

        _orderDataSourceMock.Verify(
            x => x.UpdateAsync(It.Is<OrderDto>(o =>
                o.Id == orderId &&
                o.Items.Count == 1 &&
                o.Items[0].ProductId == productId &&
                o.Items[0].Quantity == 2 &&
                o.Items[0].CustomIngredients.Count == 1 &&
                o.Items[0].CustomIngredients[0].ProductBaseIngredientId == baseIngredientId &&
                o.Items[0].CustomIngredients[0].Quantity == 5)),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenProductHasNoCustomIngredients_ShouldUseBaseIngredients()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var baseIngredientId = Guid.NewGuid();

        var orderDto = new OrderDto
        {
            Id = orderId,
            Code = "ORD-001",
            CustomerId = customerId,
            CreatedAt = DateTime.UtcNow,
            OrderStatus = (int)EnumOrderStatus.Started,
            TotalPrice = 0,
            Items = new List<OrderedProductDto>()
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

        var input = new AddProductToOrderInputModel
        {
            OrderId = orderId,
            ProductId = productId,
            Quantity = 2,
            Observation = "Test observation",
            CustomIngredients = new List<CustomIngredientInputModel>() // Sem ingredientes customizados
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
        Assert.NotEqual(Guid.Empty, result.OrderedProductId);
        Assert.True(result.TotalPrice > 0);

        _orderDataSourceMock.Verify(
            x => x.UpdateAsync(It.Is<OrderDto>(o =>
                o.Id == orderId &&
                o.Items.Count == 1 &&
                o.Items[0].ProductId == productId &&
                o.Items[0].Quantity == 2 &&
                o.Items[0].CustomIngredients.Count == 1 &&
                o.Items[0].CustomIngredients[0].ProductBaseIngredientId == baseIngredientId &&
                o.Items[0].CustomIngredients[0].Quantity == 1)), // Quantidade padrão
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCustomIngredientQuantityExceedsMax_ShouldClampToMax()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var baseIngredientId = Guid.NewGuid();

        var orderDto = new OrderDto
        {
            Id = orderId,
            Code = "ORD-001",
            CustomerId = customerId,
            CreatedAt = DateTime.UtcNow,
            OrderStatus = (int)EnumOrderStatus.Started,
            TotalPrice = 0,
            Items = new List<OrderedProductDto>()
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

        var input = new AddProductToOrderInputModel
        {
            OrderId = orderId,
            ProductId = productId,
            Quantity = 1,
            CustomIngredients = new List<CustomIngredientInputModel>
            {
                new CustomIngredientInputModel
                {
                    ProductBaseIngredientId = baseIngredientId,
                    Quantity = 15 // Excede o máximo de 10
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
                o.Items[0].CustomIngredients[0].Quantity == 10)), // Deve ser limitado a 10
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCustomIngredientNotFound_ShouldIgnoreIt()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var baseIngredientId = Guid.NewGuid();
        var nonExistentIngredientId = Guid.NewGuid();

        var orderDto = new OrderDto
        {
            Id = orderId,
            Code = "ORD-001",
            CustomerId = customerId,
            CreatedAt = DateTime.UtcNow,
            OrderStatus = (int)EnumOrderStatus.Started,
            TotalPrice = 0,
            Items = new List<OrderedProductDto>()
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

        var input = new AddProductToOrderInputModel
        {
            OrderId = orderId,
            ProductId = productId,
            Quantity = 1,
            CustomIngredients = new List<CustomIngredientInputModel>
            {
                new CustomIngredientInputModel
                {
                    ProductBaseIngredientId = baseIngredientId,
                    Quantity = 5
                },
                new CustomIngredientInputModel
                {
                    ProductBaseIngredientId = nonExistentIngredientId, // Não existe no produto
                    Quantity = 3
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
                o.Items[0].CustomIngredients.Count == 1 && // Apenas o ingrediente válido
                o.Items[0].CustomIngredients[0].ProductBaseIngredientId == baseIngredientId)),
            Times.Once);
    }
}
