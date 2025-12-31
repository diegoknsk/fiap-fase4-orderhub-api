using FastFood.OrderHub.Application.DTOs;
using FastFood.OrderHub.Application.InputModels.ProductManagement;
using FastFood.OrderHub.Application.Ports;
using FastFood.OrderHub.Application.Presenters.ProductManagement;
using FastFood.OrderHub.Application.UseCases.ProductManagement;
using Moq;
using Xunit;

namespace FastFood.OrderHub.Tests.Unit.UseCases.ProductManagement;

/// <summary>
/// Testes unitários para GetProductsPagedUseCase
/// </summary>
public class GetProductsPagedUseCaseTests
{
    private readonly Mock<IProductDataSource> _productDataSourceMock;
    private readonly GetProductsPagedPresenter _presenter;
    private readonly GetProductsPagedUseCase _useCase;

    public GetProductsPagedUseCaseTests()
    {
        _productDataSourceMock = new Mock<IProductDataSource>();
        _presenter = new GetProductsPagedPresenter();
        _useCase = new GetProductsPagedUseCase(_productDataSourceMock.Object, _presenter);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Return_Paged_Products_When_Valid_Input()
    {
        // Arrange
        var input = new GetProductsPagedInputModel
        {
            Page = 1,
            PageSize = 10
        };

        var products = new List<ProductDto>
        {
            new ProductDto
            {
                Id = Guid.NewGuid(),
                Name = "Product 1",
                Category = 1,
                Price = 10.50m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new ProductDto
            {
                Id = Guid.NewGuid(),
                Name = "Product 2",
                Category = 2,
                Price = 15.75m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        _productDataSourceMock
            .Setup(x => x.GetPagedAsync(1, 10, null, null))
            .ReturnsAsync(products);

        // Act
        var result = await _useCase.ExecuteAsync(input);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        // HasNextPage será false porque retornamos apenas 2 itens e o pageSize é 10
        Assert.False(result.HasNextPage);
        Assert.False(result.HasPreviousPage);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Apply_Category_Filter_When_Provided()
    {
        // Arrange
        var input = new GetProductsPagedInputModel
        {
            Page = 1,
            PageSize = 10,
            Category = 1
        };

        var products = new List<ProductDto>
        {
            new ProductDto
            {
                Id = Guid.NewGuid(),
                Name = "Product 1",
                Category = 1,
                Price = 10.50m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        _productDataSourceMock
            .Setup(x => x.GetPagedAsync(1, 10, 1, null))
            .ReturnsAsync(products);

        // Act
        var result = await _useCase.ExecuteAsync(input);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.All(result.Items, item => Assert.Equal(1, item.Category));
    }

    [Fact]
    public async Task ExecuteAsync_Should_Apply_Name_Filter_When_Provided()
    {
        // Arrange
        var input = new GetProductsPagedInputModel
        {
            Page = 1,
            PageSize = 10,
            Name = "Test"
        };

        var products = new List<ProductDto>
        {
            new ProductDto
            {
                Id = Guid.NewGuid(),
                Name = "Test Product",
                Category = 1,
                Price = 10.50m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        _productDataSourceMock
            .Setup(x => x.GetPagedAsync(1, 10, null, "Test"))
            .ReturnsAsync(products);

        // Act
        var result = await _useCase.ExecuteAsync(input);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Contains("Test", result.Items[0].Name ?? string.Empty);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Default_Page_To_1_When_Invalid()
    {
        // Arrange
        var input = new GetProductsPagedInputModel
        {
            Page = 0,
            PageSize = 10
        };

        var products = new List<ProductDto>();

        _productDataSourceMock
            .Setup(x => x.GetPagedAsync(1, 10, null, null))
            .ReturnsAsync(products);

        // Act
        var result = await _useCase.ExecuteAsync(input);

        // Assert
        Assert.Equal(1, result.Page);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Limit_PageSize_To_100_When_Exceeds()
    {
        // Arrange
        var input = new GetProductsPagedInputModel
        {
            Page = 1,
            PageSize = 200
        };

        var products = new List<ProductDto>();

        _productDataSourceMock
            .Setup(x => x.GetPagedAsync(1, 100, null, null))
            .ReturnsAsync(products);

        // Act
        var result = await _useCase.ExecuteAsync(input);

        // Assert
        Assert.Equal(100, result.PageSize);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Return_Empty_List_When_No_Products()
    {
        // Arrange
        var input = new GetProductsPagedInputModel
        {
            Page = 1,
            PageSize = 10
        };

        var products = new List<ProductDto>();

        _productDataSourceMock
            .Setup(x => x.GetPagedAsync(1, 10, null, null))
            .ReturnsAsync(products);

        // Act
        var result = await _useCase.ExecuteAsync(input);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.False(result.HasNextPage);
    }
}

