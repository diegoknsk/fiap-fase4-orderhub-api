using FastFood.OrderHub.Application.Exceptions;
using FastFood.OrderHub.Application.InputModels.ProductManagement;
using FastFood.OrderHub.Application.Ports;
using FastFood.OrderHub.Application.Presenters.ProductManagement;
using FastFood.OrderHub.Application.UseCases.ProductManagement;
using Moq;
using Xunit;

namespace FastFood.OrderHub.Tests.Unit.UseCases.ProductManagement;

/// <summary>
/// Testes unitários para DeleteProductUseCase
/// </summary>
public class DeleteProductUseCaseTests
{
    private readonly Mock<IProductDataSource> _productDataSourceMock;
    private readonly DeleteProductPresenter _presenter;
    private readonly DeleteProductUseCase _useCase;

    public DeleteProductUseCaseTests()
    {
        _productDataSourceMock = new Mock<IProductDataSource>();
        _presenter = new DeleteProductPresenter();
        _useCase = new DeleteProductUseCase(_productDataSourceMock.Object, _presenter);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Delete_Product_When_Exists()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var input = new DeleteProductInputModel
        {
            ProductId = productId
        };

        _productDataSourceMock
            .Setup(x => x.ExistsAsync(productId))
            .ReturnsAsync(true);

        _productDataSourceMock
            .Setup(x => x.RemoveAsync(productId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.ExecuteAsync(input);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(productId, result.ProductId);
        Assert.True(result.Success);

        _productDataSourceMock.Verify(x => x.ExistsAsync(productId), Times.Once);
        _productDataSourceMock.Verify(x => x.RemoveAsync(productId), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Throw_BusinessException_When_Product_Not_Found()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var input = new DeleteProductInputModel
        {
            ProductId = productId
        };

        _productDataSourceMock
            .Setup(x => x.ExistsAsync(productId))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() => _useCase.ExecuteAsync(input));
        Assert.Equal("Produto não encontrado.", exception.Message);
        _productDataSourceMock.Verify(x => x.RemoveAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Call_RemoveAsync_Only_When_Product_Exists()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var input = new DeleteProductInputModel
        {
            ProductId = productId
        };

        _productDataSourceMock
            .Setup(x => x.ExistsAsync(productId))
            .ReturnsAsync(true);

        _productDataSourceMock
            .Setup(x => x.RemoveAsync(productId))
            .Returns(Task.CompletedTask);

        // Act
        await _useCase.ExecuteAsync(input);

        // Assert
        _productDataSourceMock.Verify(x => x.ExistsAsync(productId), Times.Once);
        _productDataSourceMock.Verify(x => x.RemoveAsync(productId), Times.Once);
    }
}



