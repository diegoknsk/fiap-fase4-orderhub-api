using FastFood.OrderHub.Api.Controllers;
using FastFood.OrderHub.Application.DTOs;
using FastFood.OrderHub.Application.InputModels.OrderManagement;
using FastFood.OrderHub.Application.Models.Common;
using FastFood.OrderHub.Application.Ports;
using FastFood.OrderHub.Application.Presenters.OrderManagement;
using FastFood.OrderHub.Application.Responses.OrderManagement;
using FastFood.OrderHub.Application.UseCases.OrderManagement;
using FastFood.OrderHub.Domain.Common.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FastFood.OrderHub.Tests.Unit.Controllers;

/// <summary>
/// Testes unitários para OrderController
/// </summary>
public class OrderControllerTests
{
    private readonly Mock<IOrderDataSource> _orderDataSourceMock;
    private readonly Mock<IProductDataSource> _productDataSourceMock;
    private readonly Mock<IRequestContext> _requestContextMock;
    private readonly GetOrderByIdUseCase _getOrderByIdUseCase;
    private readonly StartOrderUseCase _startOrderUseCase;
    private readonly AddProductToOrderUseCase _addProductToOrderUseCase;
    private readonly UpdateProductInOrderUseCase _updateProductInOrderUseCase;
    private readonly RemoveProductFromOrderUseCase _removeProductFromOrderUseCase;
    private readonly ConfirmOrderSelectionUseCase _confirmOrderSelectionUseCase;
    private readonly GetPagedOrdersUseCase _getPagedOrdersUseCase;
    private readonly OrderController _controller;

    public OrderControllerTests()
    {
        _orderDataSourceMock = new Mock<IOrderDataSource>();
        _productDataSourceMock = new Mock<IProductDataSource>();
        _requestContextMock = new Mock<IRequestContext>();

        // Configurar RequestContext como Admin por padrão para os testes existentes
        _requestContextMock.Setup(x => x.IsAdmin).Returns(true);

        _getOrderByIdUseCase = new GetOrderByIdUseCase(
            _orderDataSourceMock.Object,
            new GetOrderByIdPresenter(),
            _requestContextMock.Object);

        _startOrderUseCase = new StartOrderUseCase(
            _orderDataSourceMock.Object,
            new StartOrderPresenter());

        _addProductToOrderUseCase = new AddProductToOrderUseCase(
            _orderDataSourceMock.Object,
            _productDataSourceMock.Object,
            new AddProductToOrderPresenter());

        _updateProductInOrderUseCase = new UpdateProductInOrderUseCase(
            _orderDataSourceMock.Object,
            _productDataSourceMock.Object,
            new UpdateProductInOrderPresenter());

        _removeProductFromOrderUseCase = new RemoveProductFromOrderUseCase(
            _orderDataSourceMock.Object,
            new RemoveProductFromOrderPresenter());

        var paymentServiceClientMock = new Mock<IPaymentServiceClient>();
        var loggerMock = new Mock<ILogger<ConfirmOrderSelectionUseCase>>();
        
        // Configurar RequestContext para retornar token Bearer
        _requestContextMock.Setup(x => x.GetBearerToken()).Returns("test-bearer-token");
        
        // Mock PaymentServiceClient para retornar sucesso
        paymentServiceClientMock
            .Setup(x => x.CreatePaymentAsync(It.IsAny<FastFood.OrderHub.Application.DTOs.Payment.CreatePaymentRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FastFood.OrderHub.Application.DTOs.Payment.CreatePaymentResponse
            {
                PaymentId = Guid.NewGuid(),
                Status = "Created",
                CreatedAt = DateTime.UtcNow
            });

        _confirmOrderSelectionUseCase = new ConfirmOrderSelectionUseCase(
            _orderDataSourceMock.Object,
            new ConfirmOrderSelectionPresenter(),
            paymentServiceClientMock.Object,
            _requestContextMock.Object,
            loggerMock.Object);

        _getPagedOrdersUseCase = new GetPagedOrdersUseCase(
            _orderDataSourceMock.Object,
            new GetPagedOrdersPresenter());

        _controller = new OrderController(
            _getOrderByIdUseCase,
            _startOrderUseCase,
            _addProductToOrderUseCase,
            _updateProductInOrderUseCase,
            _removeProductFromOrderUseCase,
            _confirmOrderSelectionUseCase,
            _getPagedOrdersUseCase);
    }

    [Fact]
    public async Task GetPaged_WhenValidInput_ShouldReturnOk()
    {
        // Arrange
        _orderDataSourceMock
            .Setup(x => x.GetPagedAsync(1, 10, null))
            .ReturnsAsync(new List<OrderDto>());

        // Act
        var result = await _controller.GetPaged(1, 10, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<GetPagedOrdersResponse>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Content);
        var response = Assert.IsType<GetPagedOrdersResponse>(apiResponse.Content);
        Assert.NotNull(response);
    }

    [Fact]
    public async Task GetById_WhenOrderExists_ShouldReturnOk()
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
            Items = new List<OrderedProductDto>()
        };

        _orderDataSourceMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(orderDto);

        // Act
        var result = await _controller.GetById(orderId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<GetOrderByIdResponse>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Content);
        var response = Assert.IsType<GetOrderByIdResponse>(apiResponse.Content);
        Assert.Equal(orderId, response.OrderId);
    }

    [Fact]
    public async Task GetById_WhenOrderDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        _orderDataSourceMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync((OrderDto?)null);

        // Act
        var result = await _controller.GetById(orderId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<GetOrderByIdResponse>>(notFoundResult.Value);
        Assert.False(apiResponse.Success);
    }

    [Fact]
    public async Task Start_WhenValidInput_ShouldReturnCreated()
    {
        // Arrange
        var input = new StartOrderInputModel
        {
            CustomerId = Guid.NewGuid()
        };

        _orderDataSourceMock
            .Setup(x => x.GenerateOrderCodeAsync())
            .ReturnsAsync("ORD-001");

        _orderDataSourceMock
            .Setup(x => x.AddAsync(It.IsAny<OrderDto>()))
            .ReturnsAsync(Guid.NewGuid());

        // Act
        var result = await _controller.Start(input);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<StartOrderResponse>>(createdResult.Value);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Content);
        var response = Assert.IsType<StartOrderResponse>(apiResponse.Content);
        Assert.NotEqual(Guid.Empty, response.OrderId);
    }

    [Fact]
    public async Task AddProduct_WhenValidInput_ShouldReturnOk()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var input = new AddProductToOrderInputModel
        {
            OrderId = orderId,
            ProductId = productId,
            Quantity = 2
        };

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
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            BaseIngredients = new List<ProductBaseIngredientDto>()
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
        var result = await _controller.AddProduct(input);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<AddProductToOrderResponse>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Content);
        var response = Assert.IsType<AddProductToOrderResponse>(apiResponse.Content);
        Assert.Equal(orderId, response.OrderId);
    }

    [Fact]
    public async Task AddProduct_WhenOrderDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var input = new AddProductToOrderInputModel
        {
            OrderId = orderId,
            ProductId = Guid.NewGuid(),
            Quantity = 2
        };

        _orderDataSourceMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync((OrderDto?)null);

        // Act
        var result = await _controller.AddProduct(input);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<AddProductToOrderResponse>>(notFoundResult.Value);
        Assert.False(apiResponse.Success);
    }

    [Fact]
    public async Task RemoveProduct_WhenValidInput_ShouldReturnOk()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderedProductId = Guid.NewGuid();
        var input = new RemoveProductFromOrderInputModel
        {
            OrderId = orderId,
            OrderedProductId = orderedProductId
        };

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
                    Id = orderedProductId,
                    ProductId = Guid.NewGuid(),
                    OrderId = orderId,
                    Quantity = 2,
                    FinalPrice = 25.00m,
                    CustomIngredients = new List<OrderedProductIngredientDto>()
                }
            }
        };

        _orderDataSourceMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(orderDto);

        _orderDataSourceMock
            .Setup(x => x.UpdateAsync(It.IsAny<OrderDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.RemoveProduct(input);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<RemoveProductFromOrderResponse>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Content);
        var response = Assert.IsType<RemoveProductFromOrderResponse>(apiResponse.Content);
        Assert.Equal(orderId, response.OrderId);
    }

    [Fact]
    public async Task ConfirmSelection_WhenValidInput_ShouldReturnOk()
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

        _orderDataSourceMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(orderDto);

        _orderDataSourceMock
            .Setup(x => x.UpdateAsync(It.IsAny<OrderDto>()))
            .Returns(Task.CompletedTask);

        // O PaymentServiceClient já está mockado no construtor do teste

        // Act
        var result = await _controller.ConfirmSelection(orderId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<ConfirmOrderSelectionResponse>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Content);
        var response = Assert.IsType<ConfirmOrderSelectionResponse>(apiResponse.Content);
        Assert.Equal(orderId, response.OrderId);
        Assert.Equal((int)EnumOrderStatus.AwaitingPayment, response.OrderStatus);
    }

    [Fact]
    public async Task UpdateProduct_WhenValidInput_ShouldReturnOk()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderedProductId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var input = new UpdateProductInOrderInputModel
        {
            OrderId = orderId,
            OrderedProductId = orderedProductId,
            Quantity = 3,
            Observation = "Updated observation"
        };

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
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            BaseIngredients = new List<ProductBaseIngredientDto>()
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
        var result = await _controller.UpdateProduct(input);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<UpdateProductInOrderResponse>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Content);
        var response = Assert.IsType<UpdateProductInOrderResponse>(apiResponse.Content);
        Assert.Equal(orderId, response.OrderId);
    }

    [Fact]
    public async Task UpdateProduct_WhenOrderDoesNotExist_ShouldReturnNotFound()
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
        var result = await _controller.UpdateProduct(input);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<UpdateProductInOrderResponse>>(notFoundResult.Value);
        Assert.False(apiResponse.Success);
    }

    [Fact]
    public async Task RemoveProduct_WhenOrderDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var input = new RemoveProductFromOrderInputModel
        {
            OrderId = orderId,
            OrderedProductId = Guid.NewGuid()
        };

        _orderDataSourceMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync((OrderDto?)null);

        // Act
        var result = await _controller.RemoveProduct(input);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<RemoveProductFromOrderResponse>>(notFoundResult.Value);
        Assert.False(apiResponse.Success);
    }

    [Fact]
    public async Task ConfirmSelection_WhenOrderDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        _orderDataSourceMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync((OrderDto?)null);

        // Act
        var result = await _controller.ConfirmSelection(orderId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<ConfirmOrderSelectionResponse>>(notFoundResult.Value);
        Assert.False(apiResponse.Success);
    }

    [Fact]
    public async Task GetPaged_WithStatus_ShouldReturnOk()
    {
        // Arrange
        var status = (int)EnumOrderStatus.Started;
        _orderDataSourceMock
            .Setup(x => x.GetPagedAsync(1, 10, status))
            .ReturnsAsync(new List<OrderDto>());

        // Act
        var result = await _controller.GetPaged(1, 10, status);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<GetPagedOrdersResponse>>(okResult.Value);
        Assert.True(apiResponse.Success);
    }

    [Fact]
    public async Task Start_WhenArgumentException_ShouldReturnBadRequest()
    {
        // Arrange
        var input = new StartOrderInputModel
        {
            CustomerId = Guid.Empty // Pode causar erro
        };

        _orderDataSourceMock
            .Setup(x => x.GenerateOrderCodeAsync())
            .ReturnsAsync("ORD-001");

        _orderDataSourceMock
            .Setup(x => x.AddAsync(It.IsAny<OrderDto>()))
            .ThrowsAsync(new FastFood.OrderHub.Application.Exceptions.BusinessException("Invalid customer"));

        // Act
        var result = await _controller.Start(input);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<StartOrderResponse>>(badRequestResult.Value);
        Assert.False(apiResponse.Success);
    }

    [Fact]
    public async Task AddProduct_WhenArgumentException_ShouldReturnBadRequest()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var input = new AddProductToOrderInputModel
        {
            OrderId = orderId,
            ProductId = Guid.NewGuid(),
            Quantity = 0 // Quantidade inválida
        };

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

        _orderDataSourceMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(orderDto);

        // Act
        var result = await _controller.AddProduct(input);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<AddProductToOrderResponse>>(badRequestResult.Value);
        Assert.False(apiResponse.Success);
    }

    [Fact]
    public async Task AddProduct_WhenInvalidOperationException_ShouldReturnBadRequest()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var input = new AddProductToOrderInputModel
        {
            OrderId = orderId,
            ProductId = productId,
            Quantity = 2
        };

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
            IsActive = false, // Inativo
            CreatedAt = DateTime.UtcNow,
            BaseIngredients = new List<ProductBaseIngredientDto>()
        };

        _orderDataSourceMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(orderDto);

        _productDataSourceMock
            .Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync(productDto);

        // Act
        var result = await _controller.AddProduct(input);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<AddProductToOrderResponse>>(badRequestResult.Value);
        Assert.False(apiResponse.Success);
    }

    [Fact]
    public async Task UpdateProduct_WhenArgumentException_ShouldReturnBadRequest()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var input = new UpdateProductInOrderInputModel
        {
            OrderId = orderId,
            OrderedProductId = Guid.NewGuid(),
            Quantity = 0 // Quantidade inválida
        };

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
                    Id = Guid.NewGuid(),
                    ProductId = Guid.NewGuid(),
                    OrderId = orderId,
                    Quantity = 1,
                    FinalPrice = 25.00m,
                    CustomIngredients = new List<OrderedProductIngredientDto>()
                }
            }
        };

        _orderDataSourceMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(orderDto);

        // Act
        var result = await _controller.UpdateProduct(input);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<UpdateProductInOrderResponse>>(badRequestResult.Value);
        Assert.False(apiResponse.Success);
    }

    [Fact]
    public async Task ConfirmSelection_WhenInvalidOperationException_ShouldReturnBadRequest()
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
            Items = new List<OrderedProductDto>() // Sem itens
        };

        _orderDataSourceMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(orderDto);

        // Act
        var result = await _controller.ConfirmSelection(orderId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<ConfirmOrderSelectionResponse>>(badRequestResult.Value);
        Assert.False(apiResponse.Success);
    }
}
