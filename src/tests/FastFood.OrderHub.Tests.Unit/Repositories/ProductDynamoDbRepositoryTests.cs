using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using FastFood.OrderHub.Application.DTOs;
using FastFood.OrderHub.Infra.Persistence.Configurations;
using FastFood.OrderHub.Infra.Persistence.Repositories;
using Moq;
using System.Globalization;
using Xunit;

namespace FastFood.OrderHub.Tests.Unit.Repositories;

/// <summary>
/// Testes unitários para ProductDynamoDbRepository
/// </summary>
public class ProductDynamoDbRepositoryTests
{
    private readonly Mock<IAmazonDynamoDB> _dynamoDbMock;
    private readonly ProductDynamoDbRepository _repository;

    public ProductDynamoDbRepositoryTests()
    {
        _dynamoDbMock = new Mock<IAmazonDynamoDB>();
        _repository = new ProductDynamoDbRepository(_dynamoDbMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Product_When_Exists()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var item = CreateProductItem(productId, "Test Product", 1, 10.50m, true);

        _dynamoDbMock
            .Setup(x => x.GetItemAsync(It.IsAny<GetItemRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetItemResponse { Item = item });

        // Act
        var result = await _repository.GetByIdAsync(productId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(productId, result.Id);
        Assert.Equal("Test Product", result.Name);
        Assert.Equal(1, result.Category);
        Assert.Equal(10.50m, result.Price);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Null_When_Not_Exists()
    {
        // Arrange
        var productId = Guid.NewGuid();

        _dynamoDbMock
            .Setup(x => x.GetItemAsync(It.IsAny<GetItemRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetItemResponse { Item = new Dictionary<string, AttributeValue>() });

        // Act
        var result = await _repository.GetByIdAsync(productId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Throw_When_Schema_Mismatch()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var exception = new AmazonDynamoDBException("does not match the schema");

        _dynamoDbMock
            .Setup(x => x.GetItemAsync(It.IsAny<GetItemRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.GetByIdAsync(productId));
        Assert.Contains("Erro ao buscar produto", ex.Message);
        Assert.Contains(productId.ToString(), ex.Message);
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_All_Products()
    {
        // Arrange
        var product1 = CreateProductItem(Guid.NewGuid(), "Product 1", 1, 10.00m, true);
        var product2 = CreateProductItem(Guid.NewGuid(), "Product 2", 2, 20.00m, true);

        _dynamoDbMock
            .Setup(x => x.ScanAsync(It.IsAny<ScanRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ScanResponse
            {
                Items = new List<Dictionary<string, AttributeValue>> { product1, product2 }
            });

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.Name == "Product 1");
        Assert.Contains(result, p => p.Name == "Product 2");
    }

    [Fact]
    public async Task GetByCategoryAsync_Should_Return_Products_By_Category()
    {
        // Arrange
        var category = 1;
        var product1 = CreateProductItem(Guid.NewGuid(), "Product 1", category, 10.00m, true);
        var product2 = CreateProductItem(Guid.NewGuid(), "Product 2", category, 20.00m, true);

        _dynamoDbMock
            .Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QueryResponse
            {
                Items = new List<Dictionary<string, AttributeValue>> { product1, product2 }
            });

        // Act
        var result = await _repository.GetByCategoryAsync(category);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, p => Assert.Equal(category, p.Category));
    }

    [Fact]
    public async Task AddAsync_Should_Call_PutItem_With_Correct_Item()
    {
        // Arrange
        var product = new ProductDto
        {
            Id = Guid.NewGuid(),
            Name = "New Product",
            Category = 1,
            Price = 10.50m,
            Description = "Test Description",
            ImageUrl = "https://example.com/image.jpg",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        PutItemRequest? capturedRequest = null;

        _dynamoDbMock
            .Setup(x => x.PutItemAsync(It.IsAny<PutItemRequest>(), It.IsAny<CancellationToken>()))
            .Callback<PutItemRequest, CancellationToken>((req, ct) => capturedRequest = req)
            .ReturnsAsync(new PutItemResponse());

        // Act
        await _repository.AddAsync(product);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Equal(DynamoDbTableConfiguration.FASTFOOD_PRODUCTS_TABLE, capturedRequest.TableName);
        Assert.True(capturedRequest.Item.ContainsKey(DynamoDbTableConfiguration.PRODUCT_ID_ATTRIBUTE));
        Assert.Equal(product.Id.ToString(), capturedRequest.Item[DynamoDbTableConfiguration.PRODUCT_ID_ATTRIBUTE].S);
        Assert.Equal(product.Name, capturedRequest.Item["Name"].S);
        Assert.Equal(product.Category.ToString(), capturedRequest.Item[DynamoDbTableConfiguration.CATEGORY_ATTRIBUTE].N);
    }

    [Fact]
    public async Task UpdateAsync_Should_Call_PutItem_With_Correct_Item()
    {
        // Arrange
        var product = new ProductDto
        {
            Id = Guid.NewGuid(),
            Name = "Updated Product",
            Category = 1,
            Price = 15.00m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        PutItemRequest? capturedRequest = null;

        _dynamoDbMock
            .Setup(x => x.PutItemAsync(It.IsAny<PutItemRequest>(), It.IsAny<CancellationToken>()))
            .Callback<PutItemRequest, CancellationToken>((req, ct) => capturedRequest = req)
            .ReturnsAsync(new PutItemResponse());

        // Act
        await _repository.UpdateAsync(product);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Equal(DynamoDbTableConfiguration.FASTFOOD_PRODUCTS_TABLE, capturedRequest.TableName);
        Assert.Equal(product.Id.ToString(), capturedRequest.Item[DynamoDbTableConfiguration.PRODUCT_ID_ATTRIBUTE].S);
    }

    [Fact]
    public async Task DeleteAsync_Should_Call_UpdateItem_With_IsActive_False()
    {
        // Arrange
        var productId = Guid.NewGuid();

        UpdateItemRequest? capturedRequest = null;

        _dynamoDbMock
            .Setup(x => x.UpdateItemAsync(It.IsAny<UpdateItemRequest>(), It.IsAny<CancellationToken>()))
            .Callback<UpdateItemRequest, CancellationToken>((req, ct) => capturedRequest = req)
            .ReturnsAsync(new UpdateItemResponse());

        // Act
        await _repository.DeleteAsync(productId);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Equal(DynamoDbTableConfiguration.FASTFOOD_PRODUCTS_TABLE, capturedRequest.TableName);
        Assert.Equal(productId.ToString(), capturedRequest.Key[DynamoDbTableConfiguration.PRODUCT_ID_ATTRIBUTE].S);
        Assert.Equal("SET IsActive = :isActive", capturedRequest.UpdateExpression);
        Assert.False(capturedRequest.ExpressionAttributeValues[":isActive"].BOOL);
    }

    [Fact]
    public async Task ExistsAsync_Should_Return_True_When_Product_Exists()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var item = CreateProductItem(productId, "Test Product", 1, 10.50m, true);

        _dynamoDbMock
            .Setup(x => x.GetItemAsync(It.IsAny<GetItemRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetItemResponse { Item = item });

        // Act
        var result = await _repository.ExistsAsync(productId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsAsync_Should_Return_False_When_Product_Not_Exists()
    {
        // Arrange
        var productId = Guid.NewGuid();

        _dynamoDbMock
            .Setup(x => x.GetItemAsync(It.IsAny<GetItemRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetItemResponse { Item = new Dictionary<string, AttributeValue>() });

        // Act
        var result = await _repository.ExistsAsync(productId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetPagedAsync_Should_Return_Paged_Products_Without_Filters()
    {
        // Arrange
        var product1 = CreateProductItem(Guid.NewGuid(), "Product 1", 1, 10.00m, true);
        var product2 = CreateProductItem(Guid.NewGuid(), "Product 2", 2, 20.00m, true);

        _dynamoDbMock
            .Setup(x => x.ScanAsync(It.IsAny<ScanRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ScanResponse
            {
                Items = new List<Dictionary<string, AttributeValue>> { product1, product2 }
            });

        // Act
        var result = await _repository.GetPagedAsync(1, 10);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetPagedAsync_Should_Return_Paged_Products_With_Category_Filter()
    {
        // Arrange
        var category = 1;
        var product1 = CreateProductItem(Guid.NewGuid(), "Product 1", category, 10.00m, true);

        _dynamoDbMock
            .Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QueryResponse
            {
                Items = new List<Dictionary<string, AttributeValue>> { product1 }
            });

        // Act
        var result = await _repository.GetPagedAsync(1, 10, category);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(category, result[0].Category);
    }

    [Fact]
    public async Task GetPagedAsync_Should_Return_Paged_Products_With_Name_Filter()
    {
        // Arrange
        var name = "Test";
        var product1 = CreateProductItem(Guid.NewGuid(), "Test Product", 1, 10.00m, true);

        _dynamoDbMock
            .Setup(x => x.ScanAsync(It.IsAny<ScanRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ScanResponse
            {
                Items = new List<Dictionary<string, AttributeValue>> { product1 }
            });

        // Act
        var result = await _repository.GetPagedAsync(1, 10, null, name);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Contains("Test", result[0].Name);
    }

    [Fact]
    public async Task GetPagedAsync_Should_Handle_Pagination()
    {
        // Arrange
        var product1 = CreateProductItem(Guid.NewGuid(), "Product 1", 1, 10.00m, true);
        var product2 = CreateProductItem(Guid.NewGuid(), "Product 2", 2, 20.00m, true);

        _dynamoDbMock
            .SetupSequence(x => x.ScanAsync(It.IsAny<ScanRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ScanResponse
            {
                Items = new List<Dictionary<string, AttributeValue>> { product1 },
                LastEvaluatedKey = new Dictionary<string, AttributeValue> { { "key", new AttributeValue { S = "value" } } }
            })
            .ReturnsAsync(new ScanResponse
            {
                Items = new List<Dictionary<string, AttributeValue>> { product2 }
            });

        // Act
        var result = await _repository.GetPagedAsync(1, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
    }

    [Fact]
    public async Task GetPagedAsync_Should_Map_Product_With_BaseIngredients()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();
        var item = CreateProductItem(productId, "Product with Ingredients", 1, 10.00m, true);
        
        var ingredientMap = new Dictionary<string, AttributeValue>
        {
            { "Id", new AttributeValue { S = ingredientId.ToString() } },
            { "Name", new AttributeValue { S = "Ingredient 1" } },
            { "Price", new AttributeValue { N = "2.00" } }
        };
        
        item["BaseIngredients"] = new AttributeValue
        {
            L = new List<AttributeValue> { new AttributeValue { M = ingredientMap } }
        };

        _dynamoDbMock
            .Setup(x => x.ScanAsync(It.IsAny<ScanRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ScanResponse
            {
                Items = new List<Dictionary<string, AttributeValue>> { item }
            });

        // Act
        var result = await _repository.GetPagedAsync(1, 10);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Single(result[0].BaseIngredients);
        Assert.Equal(ingredientId, result[0].BaseIngredients[0].Id);
        Assert.Equal("Ingredient 1", result[0].BaseIngredients[0].Name);
        Assert.Equal(2.00m, result[0].BaseIngredients[0].Price);
        Assert.Equal(productId, result[0].BaseIngredients[0].ProductId);
    }

    private Dictionary<string, AttributeValue> CreateProductItem(Guid id, string name, int category, decimal price, bool isActive)
    {
        return new Dictionary<string, AttributeValue>
        {
            { DynamoDbTableConfiguration.PRODUCT_ID_ATTRIBUTE, new AttributeValue { S = id.ToString() } },
            { "Name", new AttributeValue { S = name } },
            { DynamoDbTableConfiguration.CATEGORY_ATTRIBUTE, new AttributeValue { N = category.ToString() } },
            { "Price", new AttributeValue { N = price.ToString("F2", CultureInfo.InvariantCulture) } },
            { "IsActive", new AttributeValue { BOOL = isActive } },
            { DynamoDbTableConfiguration.CREATED_AT_ATTRIBUTE, new AttributeValue { S = DateTime.UtcNow.ToString("O") } },
            { "BaseIngredients", new AttributeValue { L = new List<AttributeValue>() } }
        };
    }
}
