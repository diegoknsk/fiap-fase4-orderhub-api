using FastFood.OrderHub.Api.Controllers;
using FastFood.OrderHub.Application.InputModels.ProductManagement;
using FastFood.OrderHub.Application.Ports;
using FastFood.OrderHub.Application.Presenters.ProductManagement;
using FastFood.OrderHub.Application.Responses.ProductManagement;
using FastFood.OrderHub.Application.UseCases.ProductManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace FastFood.OrderHub.Tests.Unit.Controllers;

/// <summary>
/// Testes unitários para ProductsController
/// </summary>
public class ProductsControllerTests
{
    private readonly Mock<IProductDataSource> _productDataSourceMock;
    private readonly GetProductByIdUseCase _getProductByIdUseCase;
    private readonly GetProductsPagedUseCase _getProductsPagedUseCase;
    private readonly CreateProductUseCase _createProductUseCase;
    private readonly UpdateProductUseCase _updateProductUseCase;
    private readonly DeleteProductUseCase _deleteProductUseCase;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _productDataSourceMock = new Mock<IProductDataSource>();
        
        _getProductByIdUseCase = new GetProductByIdUseCase(
            _productDataSourceMock.Object,
            new GetProductByIdPresenter());
        
        _getProductsPagedUseCase = new GetProductsPagedUseCase(
            _productDataSourceMock.Object,
            new GetProductsPagedPresenter());
        
        _createProductUseCase = new CreateProductUseCase(
            _productDataSourceMock.Object,
            new CreateProductPresenter());
        
        _updateProductUseCase = new UpdateProductUseCase(
            _productDataSourceMock.Object,
            new UpdateProductPresenter());
        
        _deleteProductUseCase = new DeleteProductUseCase(
            _productDataSourceMock.Object,
            new DeleteProductPresenter());

        _controller = new ProductsController(
            _getProductByIdUseCase,
            _getProductsPagedUseCase,
            _createProductUseCase,
            _updateProductUseCase,
            _deleteProductUseCase);
    }

    [Fact]
    public async Task GetPaged_Should_Return_Ok_With_Products()
    {
        // Arrange
        var response = new GetProductsPagedResponse
        {
            Items = new List<ProductSummaryResponse>
            {
                new ProductSummaryResponse
                {
                    ProductId = Guid.NewGuid(),
                    Name = "Product 1",
                    Category = 1,
                    Price = 10.50m
                }
            },
            Page = 1,
            PageSize = 10,
            TotalCount = 1,
            TotalPages = 1,
            HasPreviousPage = false,
            HasNextPage = false
        };

        _productDataSourceMock
            .Setup(x => x.GetPagedAsync(1, 10, null, null))
            .ReturnsAsync(new List<Application.DTOs.ProductDto>
            {
                new Application.DTOs.ProductDto
                {
                    Id = response.Items[0].ProductId,
                    Name = response.Items[0].Name,
                    Category = response.Items[0].Category,
                    Price = response.Items[0].Price,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            });

        // Act
        var result = await _controller.GetPaged(1, 10, null, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedResponse = Assert.IsType<GetProductsPagedResponse>(okResult.Value);
        Assert.Single(returnedResponse.Items);
    }

    [Fact]
    public async Task GetById_Should_Return_Ok_When_Product_Exists()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var response = new GetProductByIdResponse
        {
            ProductId = productId,
            Name = "Test Product",
            Category = 1,
            Price = 10.50m
        };

        _productDataSourceMock
            .Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync(new Application.DTOs.ProductDto
            {
                Id = productId,
                Name = response.Name,
                Category = response.Category,
                Price = response.Price,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

        // Act
        var result = await _controller.GetById(productId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedResponse = Assert.IsType<GetProductByIdResponse>(okResult.Value);
        Assert.Equal(productId, returnedResponse.ProductId);
    }

    [Fact]
    public async Task GetById_Should_Return_NotFound_When_Product_Not_Exists()
    {
        // Arrange
        var productId = Guid.NewGuid();

        _productDataSourceMock
            .Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync((Application.DTOs.ProductDto?)null);

        // Act
        var result = await _controller.GetById(productId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_Should_Return_Created_When_Valid_Input()
    {
        // Arrange
        var input = new CreateProductInputModel
        {
            Name = "Test Product",
            Category = 1,
            Price = 10.50m
        };

        var response = new CreateProductResponse
        {
            ProductId = Guid.NewGuid(),
            Name = input.Name,
            Category = input.Category,
            Price = input.Price
        };

        _productDataSourceMock
            .Setup(x => x.AddAsync(It.IsAny<Application.DTOs.ProductDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Create(input);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var returnedResponse = Assert.IsType<CreateProductResponse>(createdResult.Value);
        Assert.NotEqual(Guid.Empty, returnedResponse.ProductId);
        Assert.Equal(input.Name, returnedResponse.Name);
    }

    [Fact]
    public async Task Create_Should_Return_BadRequest_When_ArgumentException_Thrown()
    {
        // Arrange
        var input = new CreateProductInputModel
        {
            Name = string.Empty,
            Category = 1,
            Price = 10.50m
        };

        // Não precisa mockar, o UseCase vai lançar a exceção naturalmente

        // Act
        var result = await _controller.Create(input);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task Update_Should_Return_Ok_When_Product_Exists()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var input = new UpdateProductInputModel
        {
            ProductId = productId,
            Name = "Updated Product",
            Category = 1,
            Price = 15.75m
        };

        var response = new UpdateProductResponse
        {
            ProductId = productId,
            Name = input.Name,
            Category = input.Category,
            Price = input.Price
        };

        _productDataSourceMock
            .Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync(new Application.DTOs.ProductDto
            {
                Id = productId,
                Name = "Old Name",
                Category = 1,
                Price = 10.50m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

        _productDataSourceMock
            .Setup(x => x.UpdateAsync(It.IsAny<Application.DTOs.ProductDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Update(productId, input);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedResponse = Assert.IsType<UpdateProductResponse>(okResult.Value);
        Assert.Equal(productId, returnedResponse.ProductId);
        Assert.Equal(input.Name, returnedResponse.Name);
    }

    [Fact]
    public async Task Update_Should_Return_NotFound_When_Product_Not_Exists()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var input = new UpdateProductInputModel
        {
            ProductId = productId,
            Name = "Updated Product",
            Category = 1,
            Price = 15.75m
        };

        _productDataSourceMock
            .Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync((Application.DTOs.ProductDto?)null);

        // Act
        var result = await _controller.Update(productId, input);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Update_Should_Return_BadRequest_When_ArgumentException_Thrown()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var input = new UpdateProductInputModel
        {
            ProductId = productId,
            Name = string.Empty,
            Category = 1,
            Price = 10.50m
        };

        _productDataSourceMock
            .Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync(new Application.DTOs.ProductDto
            {
                Id = productId,
                Name = "Old Name",
                Category = 1,
                Price = 10.50m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        // O UseCase vai lançar a exceção naturalmente

        // Act
        var result = await _controller.Update(productId, input);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task Delete_Should_Return_Ok_When_Product_Exists()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var response = new DeleteProductResponse
        {
            ProductId = productId,
            Success = true
        };

        _productDataSourceMock
            .Setup(x => x.ExistsAsync(productId))
            .ReturnsAsync(true);

        _productDataSourceMock
            .Setup(x => x.RemoveAsync(productId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(productId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedResponse = Assert.IsType<DeleteProductResponse>(okResult.Value);
        Assert.Equal(productId, returnedResponse.ProductId);
        Assert.True(returnedResponse.Success);
        
        _productDataSourceMock.Verify(x => x.ExistsAsync(productId), Times.Once);
        _productDataSourceMock.Verify(x => x.RemoveAsync(productId), Times.Once);
    }

    [Fact]
    public async Task Delete_Should_Return_NotFound_When_Product_Not_Exists()
    {
        // Arrange
        var productId = Guid.NewGuid();

        _productDataSourceMock
            .Setup(x => x.ExistsAsync(productId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(productId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}

