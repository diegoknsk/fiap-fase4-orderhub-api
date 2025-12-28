using FastFood.OrderHub.Application.DTOs;
using FastFood.OrderHub.Application.InputModels.ProductManagement;
using FastFood.OrderHub.Application.Ports;
using FastFood.OrderHub.Application.Presenters.ProductManagement;
using FastFood.OrderHub.Application.UseCases.ProductManagement;
using Moq;
using Xunit;

namespace FastFood.OrderHub.Tests.Unit.UseCases.ProductManagement;

/// <summary>
/// Testes unitários para CreateProductUseCase
/// </summary>
public class CreateProductUseCaseTests
{
    private readonly Mock<IProductDataSource> _productDataSourceMock;
    private readonly CreateProductPresenter _presenter;
    private readonly CreateProductUseCase _useCase;

    public CreateProductUseCaseTests()
    {
        _productDataSourceMock = new Mock<IProductDataSource>();
        _presenter = new CreateProductPresenter();
        _useCase = new CreateProductUseCase(_productDataSourceMock.Object, _presenter);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Create_Product_When_Valid_Input()
    {
        // Arrange
        var input = new CreateProductInputModel
        {
            Name = "Test Product",
            Category = 1,
            Price = 10.50m,
            Description = "Test Description",
            ImageUrl = "https://example.com/image.jpg",
            BaseIngredients = new List<CreateProductBaseIngredientInputModel>
            {
                new CreateProductBaseIngredientInputModel
                {
                    Name = "Ingredient 1",
                    Price = 2.00m
                }
            }
        };

        _productDataSourceMock
            .Setup(x => x.AddAsync(It.IsAny<ProductDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.ExecuteAsync(input);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.ProductId);
        Assert.Equal(input.Name, result.Name);
        Assert.Equal(input.Category, result.Category);
        Assert.Equal(input.Price, result.Price);
        Assert.Equal(input.Description, result.Description);
        Assert.Equal(input.ImageUrl, result.ImageUrl);
        Assert.True(result.IsActive);
        Assert.Single(result.BaseIngredients);

        _productDataSourceMock.Verify(
            x => x.AddAsync(It.Is<ProductDto>(p =>
                !string.IsNullOrWhiteSpace(p.Name) &&
                p.Price > 0 &&
                p.IsActive &&
                p.BaseIngredients.Count == 1)),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Throw_Exception_When_Name_Is_Empty()
    {
        // Arrange
        var input = new CreateProductInputModel
        {
            Name = string.Empty,
            Category = 1,
            Price = 10.50m
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _useCase.ExecuteAsync(input));
        _productDataSourceMock.Verify(x => x.AddAsync(It.IsAny<ProductDto>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Throw_Exception_When_Price_Is_Zero()
    {
        // Arrange
        var input = new CreateProductInputModel
        {
            Name = "Test Product",
            Category = 1,
            Price = 0
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _useCase.ExecuteAsync(input));
        _productDataSourceMock.Verify(x => x.AddAsync(It.IsAny<ProductDto>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Throw_Exception_When_Price_Is_Negative()
    {
        // Arrange
        var input = new CreateProductInputModel
        {
            Name = "Test Product",
            Category = 1,
            Price = -10.50m
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _useCase.ExecuteAsync(input));
        _productDataSourceMock.Verify(x => x.AddAsync(It.IsAny<ProductDto>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Create_Product_Without_Ingredients()
    {
        // Arrange
        var input = new CreateProductInputModel
        {
            Name = "Test Product",
            Category = 1,
            Price = 10.50m,
            BaseIngredients = new List<CreateProductBaseIngredientInputModel>()
        };

        _productDataSourceMock
            .Setup(x => x.AddAsync(It.IsAny<ProductDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.ExecuteAsync(input);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.BaseIngredients);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Set_ProductId_On_Ingredients()
    {
        // Arrange
        var input = new CreateProductInputModel
        {
            Name = "Test Product",
            Category = 1,
            Price = 10.50m,
            BaseIngredients = new List<CreateProductBaseIngredientInputModel>
            {
                new CreateProductBaseIngredientInputModel
                {
                    Name = "Ingredient 1",
                    Price = 2.00m
                }
            }
        };

        Guid? capturedProductId = null;

        _productDataSourceMock
            .Setup(x => x.AddAsync(It.IsAny<ProductDto>()))
            .Callback<ProductDto>(dto =>
            {
                capturedProductId = dto.Id;
                Assert.All(dto.BaseIngredients, ing => Assert.Equal(dto.Id, ing.ProductId));
            })
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.ExecuteAsync(input);

        // Assert
        Assert.NotNull(capturedProductId);
        Assert.Equal(capturedProductId, result.ProductId);
    }
}

