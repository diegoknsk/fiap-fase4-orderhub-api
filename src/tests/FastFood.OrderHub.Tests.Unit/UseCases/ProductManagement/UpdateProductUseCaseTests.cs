using FastFood.OrderHub.Application.DTOs;
using FastFood.OrderHub.Application.InputModels.ProductManagement;
using FastFood.OrderHub.Application.Ports;
using FastFood.OrderHub.Application.Presenters.ProductManagement;
using FastFood.OrderHub.Application.UseCases.ProductManagement;
using Moq;
using Xunit;

namespace FastFood.OrderHub.Tests.Unit.UseCases.ProductManagement;

/// <summary>
/// Testes unitários para UpdateProductUseCase
/// </summary>
public class UpdateProductUseCaseTests
{
    private readonly Mock<IProductDataSource> _productDataSourceMock;
    private readonly UpdateProductPresenter _presenter;
    private readonly UpdateProductUseCase _useCase;

    public UpdateProductUseCaseTests()
    {
        _productDataSourceMock = new Mock<IProductDataSource>();
        _presenter = new UpdateProductPresenter();
        _useCase = new UpdateProductUseCase(_productDataSourceMock.Object, _presenter);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Update_Product_When_Valid_Input()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var existingProduct = new ProductDto
        {
            Id = productId,
            Name = "Old Name",
            Category = 1,
            Price = 10.50m,
            Description = "Old Description",
            ImageUrl = "https://example.com/old.jpg",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            BaseIngredients = new List<ProductBaseIngredientDto>
            {
                new ProductBaseIngredientDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Old Ingredient",
                    Price = 2.00m,
                    ProductId = productId
                }
            }
        };

        var input = new UpdateProductInputModel
        {
            ProductId = productId,
            Name = "New Name",
            Category = 2,
            Price = 15.75m,
            Description = "New Description",
            ImageUrl = "https://example.com/new.jpg",
            BaseIngredients = new List<UpdateProductBaseIngredientInputModel>
            {
                new UpdateProductBaseIngredientInputModel
                {
                    Id = existingProduct.BaseIngredients[0].Id,
                    Name = "Updated Ingredient",
                    Price = 3.00m
                }
            }
        };

        _productDataSourceMock
            .Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync(existingProduct);

        _productDataSourceMock
            .Setup(x => x.UpdateAsync(It.IsAny<ProductDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.ExecuteAsync(input);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(productId, result.ProductId);
        Assert.Equal(input.Name, result.Name);
        Assert.Equal(input.Category, result.Category);
        Assert.Equal(input.Price, result.Price);
        Assert.Equal(input.Description, result.Description);
        Assert.Equal(input.ImageUrl, result.ImageUrl);
        Assert.Single(result.BaseIngredients);

        _productDataSourceMock.Verify(x => x.GetByIdAsync(productId), Times.Once);
        _productDataSourceMock.Verify(x => x.UpdateAsync(It.IsAny<ProductDto>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Return_Null_When_Product_Not_Found()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var input = new UpdateProductInputModel
        {
            ProductId = productId,
            Name = "New Name",
            Category = 1,
            Price = 10.50m
        };

        _productDataSourceMock
            .Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync((ProductDto?)null);

        // Act
        var result = await _useCase.ExecuteAsync(input);

        // Assert
        Assert.Null(result);
        _productDataSourceMock.Verify(x => x.UpdateAsync(It.IsAny<ProductDto>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Throw_Exception_When_Name_Is_Empty()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var existingProduct = new ProductDto
        {
            Id = productId,
            Name = "Old Name",
            Category = 1,
            Price = 10.50m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var input = new UpdateProductInputModel
        {
            ProductId = productId,
            Name = string.Empty,
            Category = 1,
            Price = 10.50m
        };

        _productDataSourceMock
            .Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync(existingProduct);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _useCase.ExecuteAsync(input));
        _productDataSourceMock.Verify(x => x.UpdateAsync(It.IsAny<ProductDto>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Add_New_Ingredient_When_Id_Not_Provided()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var existingProduct = new ProductDto
        {
            Id = productId,
            Name = "Test Product",
            Category = 1,
            Price = 10.50m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            BaseIngredients = new List<ProductBaseIngredientDto>()
        };

        var input = new UpdateProductInputModel
        {
            ProductId = productId,
            Name = "Test Product",
            Category = 1,
            Price = 10.50m,
            BaseIngredients = new List<UpdateProductBaseIngredientInputModel>
            {
                new UpdateProductBaseIngredientInputModel
                {
                    Id = null,
                    Name = "New Ingredient",
                    Price = 2.00m
                }
            }
        };

        _productDataSourceMock
            .Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync(existingProduct);

        _productDataSourceMock
            .Setup(x => x.UpdateAsync(It.IsAny<ProductDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.ExecuteAsync(input);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.BaseIngredients);
        Assert.Equal("New Ingredient", result.BaseIngredients[0].Name);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Remove_Ingredients_Not_In_Input()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var ingredientId1 = Guid.NewGuid();
        var ingredientId2 = Guid.NewGuid();

        var existingProduct = new ProductDto
        {
            Id = productId,
            Name = "Test Product",
            Category = 1,
            Price = 10.50m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            BaseIngredients = new List<ProductBaseIngredientDto>
            {
                new ProductBaseIngredientDto
                {
                    Id = ingredientId1,
                    Name = "Ingredient 1",
                    Price = 2.00m,
                    ProductId = productId
                },
                new ProductBaseIngredientDto
                {
                    Id = ingredientId2,
                    Name = "Ingredient 2",
                    Price = 3.00m,
                    ProductId = productId
                }
            }
        };

        var input = new UpdateProductInputModel
        {
            ProductId = productId,
            Name = "Test Product",
            Category = 1,
            Price = 10.50m,
            BaseIngredients = new List<UpdateProductBaseIngredientInputModel>
            {
                new UpdateProductBaseIngredientInputModel
                {
                    Id = ingredientId1,
                    Name = "Ingredient 1",
                    Price = 2.00m
                }
            }
        };

        _productDataSourceMock
            .Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync(existingProduct);

        _productDataSourceMock
            .Setup(x => x.UpdateAsync(It.IsAny<ProductDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.ExecuteAsync(input);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.BaseIngredients);
        Assert.Equal(ingredientId1, result.BaseIngredients[0].Id);
    }
}


