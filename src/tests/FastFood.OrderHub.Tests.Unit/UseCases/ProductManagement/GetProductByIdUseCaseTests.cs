using FastFood.OrderHub.Application.DTOs;
using FastFood.OrderHub.Application.Exceptions;
using FastFood.OrderHub.Application.InputModels.ProductManagement;
using FastFood.OrderHub.Application.Ports;
using FastFood.OrderHub.Application.Presenters.ProductManagement;
using FastFood.OrderHub.Application.UseCases.ProductManagement;
using Moq;
using Xunit;

namespace FastFood.OrderHub.Tests.Unit.UseCases.ProductManagement;

/// <summary>
/// Testes unitários para GetProductByIdUseCase
/// </summary>
public class GetProductByIdUseCaseTests
{
    private readonly Mock<IProductDataSource> _productDataSourceMock;
    private readonly GetProductByIdPresenter _presenter;
    private readonly GetProductByIdUseCase _useCase;

    public GetProductByIdUseCaseTests()
    {
        _productDataSourceMock = new Mock<IProductDataSource>();
        _presenter = new GetProductByIdPresenter();
        _useCase = new GetProductByIdUseCase(_productDataSourceMock.Object, _presenter);
    }

    [Fact]
    public async Task ExecuteAsync_WhenProductExists_ShouldReturnProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var productDto = new ProductDto
        {
            Id = productId,
            Name = "Test Product",
            Category = 1,
            Price = 10.50m,
            Description = "Test Description",
            ImageUrl = "https://example.com/image.jpg",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            BaseIngredients = new List<ProductBaseIngredientDto>()
        };

        var input = new GetProductByIdInputModel
        {
            ProductId = productId
        };

        _productDataSourceMock
            .Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync(productDto);

        // Act
        var result = await _useCase.ExecuteAsync(input);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(productId, result.ProductId);
        Assert.Equal(productDto.Name, result.Name);
        Assert.Equal(productDto.Category, result.Category);
        Assert.Equal(productDto.Price, result.Price);
        Assert.Equal(productDto.Description, result.Description);
        Assert.Equal(productDto.ImageUrl, result.ImageUrl);
        Assert.Equal(productDto.IsActive, result.IsActive);
        Assert.Empty(result.BaseIngredients);
    }

    [Fact]
    public async Task ExecuteAsync_WhenProductDoesNotExist_ShouldThrowBusinessException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var input = new GetProductByIdInputModel
        {
            ProductId = productId
        };

        _productDataSourceMock
            .Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync((ProductDto?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() => _useCase.ExecuteAsync(input));
        Assert.Equal("Produto não encontrado.", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WhenProductHasBaseIngredients_ShouldReturnProductWithIngredients()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();
        var productDto = new ProductDto
        {
            Id = productId,
            Name = "Test Product",
            Category = 1,
            Price = 10.50m,
            Description = "Test Description",
            ImageUrl = "https://example.com/image.jpg",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            BaseIngredients = new List<ProductBaseIngredientDto>
            {
                new ProductBaseIngredientDto
                {
                    Id = ingredientId,
                    ProductId = productId,
                    Name = "Ingredient 1",
                    Price = 2.00m
                }
            }
        };

        var input = new GetProductByIdInputModel
        {
            ProductId = productId
        };

        _productDataSourceMock
            .Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync(productDto);

        // Act
        var result = await _useCase.ExecuteAsync(input);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.BaseIngredients);
        Assert.Equal(ingredientId, result.BaseIngredients[0].Id);
        Assert.Equal("Ingredient 1", result.BaseIngredients[0].Name);
        Assert.Equal(2.00m, result.BaseIngredients[0].Price);
    }
}
