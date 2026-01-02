using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using FastFood.OrderHub.Application.DTOs;
using FastFood.OrderHub.Domain.Common.Enums;
using FastFood.OrderHub.Infra.Persistence.Configurations;
using FastFood.OrderHub.Infra.Persistence.Repositories;
using Moq;
using System.Globalization;
using Xunit;

namespace FastFood.OrderHub.Tests.Unit.Repositories;

/// <summary>
/// Testes unitários para OrderDynamoDbRepository
/// </summary>
public class OrderDynamoDbRepositoryTests
{
    private readonly Mock<IAmazonDynamoDB> _dynamoDbMock;
    private readonly OrderDynamoDbRepository _repository;

    public OrderDynamoDbRepositoryTests()
    {
        _dynamoDbMock = new Mock<IAmazonDynamoDB>();
        _repository = new OrderDynamoDbRepository(_dynamoDbMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenOrderExists_ShouldReturnOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var item = CreateOrderItem(orderId, customerId, "ORD-001", (int)EnumOrderStatus.Started, 50.00m);

        _dynamoDbMock
            .Setup(x => x.GetItemAsync(It.IsAny<GetItemRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetItemResponse { Item = item });

        // Act
        var result = await _repository.GetByIdAsync(orderId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(orderId, result.Id);
        Assert.Equal("ORD-001", result.Code);
        Assert.Equal(customerId, result.CustomerId);
        Assert.Equal((int)EnumOrderStatus.Started, result.OrderStatus);
        Assert.Equal(50.00m, result.TotalPrice);
    }

    [Fact]
    public async Task GetByIdAsync_WhenOrderDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        _dynamoDbMock
            .Setup(x => x.GetItemAsync(It.IsAny<GetItemRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetItemResponse { Item = new Dictionary<string, AttributeValue>() });

        // Act
        var result = await _repository.GetByIdAsync(orderId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByCustomerIdAsync_ShouldReturnOrders()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var order1 = CreateOrderItem(Guid.NewGuid(), customerId, "ORD-001", (int)EnumOrderStatus.Started, 50.00m);
        var order2 = CreateOrderItem(Guid.NewGuid(), customerId, "ORD-002", (int)EnumOrderStatus.AwaitingPayment, 75.00m);

        _dynamoDbMock
            .Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QueryResponse
            {
                Items = new List<Dictionary<string, AttributeValue>> { order1, order2 }
            });

        // Act
        var result = await _repository.GetByCustomerIdAsync(customerId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, o => Assert.Equal(customerId, o.CustomerId));
    }

    [Fact]
    public async Task GetByStatusAsync_ShouldReturnOrders()
    {
        // Arrange
        var status = (int)EnumOrderStatus.Started;
        var order1 = CreateOrderItem(Guid.NewGuid(), Guid.NewGuid(), "ORD-001", status, 50.00m);
        var order2 = CreateOrderItem(Guid.NewGuid(), Guid.NewGuid(), "ORD-002", status, 75.00m);

        _dynamoDbMock
            .Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QueryResponse
            {
                Items = new List<Dictionary<string, AttributeValue>> { order1, order2 }
            });

        // Act
        var result = await _repository.GetByStatusAsync(status);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, o => Assert.Equal(status, o.OrderStatus));
    }

    [Fact]
    public async Task GetByStatusWithoutPreparationAsync_ShouldFilterOutInPreparation()
    {
        // Arrange
        var status = (int)EnumOrderStatus.Started;
        var order1 = CreateOrderItem(Guid.NewGuid(), Guid.NewGuid(), "ORD-001", status, 50.00m);
        var order2 = CreateOrderItem(Guid.NewGuid(), Guid.NewGuid(), "ORD-002", (int)EnumOrderStatus.InPreparation, 75.00m);
        var order3 = CreateOrderItem(Guid.NewGuid(), Guid.NewGuid(), "ORD-003", status, 100.00m);

        _dynamoDbMock
            .Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QueryResponse
            {
                Items = new List<Dictionary<string, AttributeValue>> { order1, order2, order3 }
            });

        // Act
        var result = await _repository.GetByStatusWithoutPreparationAsync(status);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, o => Assert.NotEqual((int)EnumOrderStatus.InPreparation, o.OrderStatus));
    }

    [Fact]
    public async Task GetPagedAsync_WithoutStatus_ShouldReturnPagedOrders()
    {
        // Arrange
        var order1 = CreateOrderItem(Guid.NewGuid(), Guid.NewGuid(), "ORD-001", (int)EnumOrderStatus.Started, 50.00m);
        var order2 = CreateOrderItem(Guid.NewGuid(), Guid.NewGuid(), "ORD-002", (int)EnumOrderStatus.Started, 75.00m);

        _dynamoDbMock
            .Setup(x => x.ScanAsync(It.IsAny<ScanRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ScanResponse
            {
                Items = new List<Dictionary<string, AttributeValue>> { order1, order2 }
            });

        // Act
        var (items, lastKey) = await _repository.GetPagedAsync(1, 10, null);

        // Assert
        Assert.NotNull(items);
        Assert.Equal(2, items.Count);
    }

    [Fact]
    public async Task GetPagedAsync_WithStatus_ShouldReturnPagedOrders()
    {
        // Arrange
        var status = (int)EnumOrderStatus.Started;
        var order1 = CreateOrderItem(Guid.NewGuid(), Guid.NewGuid(), "ORD-001", status, 50.00m);

        _dynamoDbMock
            .Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QueryResponse
            {
                Items = new List<Dictionary<string, AttributeValue>> { order1 }
            });

        // Act
        var (items, lastKey) = await _repository.GetPagedAsync(1, 10, status);

        // Assert
        Assert.NotNull(items);
        Assert.Single(items);
        Assert.Equal(status, items[0].OrderStatus);
    }

    [Fact]
    public async Task AddAsync_ShouldCallPutItem()
    {
        // Arrange
        var orderDto = new OrderDto
        {
            Id = Guid.NewGuid(),
            Code = "ORD-001",
            CustomerId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            OrderStatus = (int)EnumOrderStatus.Started,
            TotalPrice = 50.00m,
            Items = new List<OrderedProductDto>()
        };

        PutItemRequest? capturedRequest = null;

        _dynamoDbMock
            .Setup(x => x.PutItemAsync(It.IsAny<PutItemRequest>(), It.IsAny<CancellationToken>()))
            .Callback<PutItemRequest, CancellationToken>((req, ct) => capturedRequest = req)
            .ReturnsAsync(new PutItemResponse());

        // Act
        await _repository.AddAsync(orderDto);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Equal(DynamoDbTableConfiguration.FASTFOOD_ORDERS_TABLE, capturedRequest.TableName);
        Assert.True(capturedRequest.Item.ContainsKey(DynamoDbTableConfiguration.ORDER_ID_ATTRIBUTE));
        Assert.Equal(orderDto.Id.ToString(), capturedRequest.Item[DynamoDbTableConfiguration.ORDER_ID_ATTRIBUTE].S);
    }

    [Fact]
    public async Task UpdateAsync_ShouldCallPutItem()
    {
        // Arrange
        var orderDto = new OrderDto
        {
            Id = Guid.NewGuid(),
            Code = "ORD-001",
            CustomerId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            OrderStatus = (int)EnumOrderStatus.AwaitingPayment,
            TotalPrice = 75.00m,
            Items = new List<OrderedProductDto>()
        };

        PutItemRequest? capturedRequest = null;

        _dynamoDbMock
            .Setup(x => x.PutItemAsync(It.IsAny<PutItemRequest>(), It.IsAny<CancellationToken>()))
            .Callback<PutItemRequest, CancellationToken>((req, ct) => capturedRequest = req)
            .ReturnsAsync(new PutItemResponse());

        // Act
        await _repository.UpdateAsync(orderDto);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Equal(DynamoDbTableConfiguration.FASTFOOD_ORDERS_TABLE, capturedRequest.TableName);
    }

    [Fact]
    public async Task DeleteAsync_ShouldCallDeleteItem()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        DeleteItemRequest? capturedRequest = null;

        _dynamoDbMock
            .Setup(x => x.DeleteItemAsync(It.IsAny<DeleteItemRequest>(), It.IsAny<CancellationToken>()))
            .Callback<DeleteItemRequest, CancellationToken>((req, ct) => capturedRequest = req)
            .ReturnsAsync(new DeleteItemResponse());

        // Act
        await _repository.DeleteAsync(orderId);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Equal(DynamoDbTableConfiguration.FASTFOOD_ORDERS_TABLE, capturedRequest.TableName);
        Assert.Equal(orderId.ToString(), capturedRequest.Key[DynamoDbTableConfiguration.ORDER_ID_ATTRIBUTE].S);
    }

    [Fact]
    public async Task ExistsAsync_WhenOrderExists_ShouldReturnTrue()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var item = CreateOrderItem(orderId, Guid.NewGuid(), "ORD-001", (int)EnumOrderStatus.Started, 50.00m);

        _dynamoDbMock
            .Setup(x => x.GetItemAsync(It.IsAny<GetItemRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetItemResponse { Item = item });

        // Act
        var result = await _repository.ExistsAsync(orderId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsAsync_WhenOrderDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        _dynamoDbMock
            .Setup(x => x.GetItemAsync(It.IsAny<GetItemRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetItemResponse { Item = new Dictionary<string, AttributeValue>() });

        // Act
        var result = await _repository.ExistsAsync(orderId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ExistsByCodeAsync_WhenCodeExists_ShouldReturnTrue()
    {
        // Arrange
        var code = "ORD-001";
        var item = CreateOrderItem(Guid.NewGuid(), Guid.NewGuid(), code, (int)EnumOrderStatus.Started, 50.00m);

        _dynamoDbMock
            .Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QueryResponse
            {
                Items = new List<Dictionary<string, AttributeValue>> { item }
            });

        // Act
        var result = await _repository.ExistsByCodeAsync(code);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsByCodeAsync_WhenCodeDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var code = "ORD-999";

        _dynamoDbMock
            .Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QueryResponse
            {
                Items = new List<Dictionary<string, AttributeValue>>()
            });

        // Act
        var result = await _repository.ExistsByCodeAsync(code);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GenerateOrderCodeAsync_ShouldReturnUniqueCode()
    {
        // Arrange
        _dynamoDbMock
            .SetupSequence(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QueryResponse { Items = new List<Dictionary<string, AttributeValue>>() }) // Primeira tentativa: código não existe
            .ReturnsAsync(new QueryResponse { Items = new List<Dictionary<string, AttributeValue>>() }); // Segunda tentativa: código não existe

        // Act
        var result = await _repository.GenerateOrderCodeAsync();

        // Assert
        Assert.NotNull(result);
        Assert.StartsWith("ORD", result);
        Assert.True(result.Length > 3);
    }

    [Fact]
    public async Task GetByIdAsync_WithItems_ShouldMapItemsCorrectly()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var item = CreateOrderItem(orderId, customerId, "ORD-001", (int)EnumOrderStatus.Started, 50.00m);
        
        var orderedProductId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var itemMap = new Dictionary<string, AttributeValue>
        {
            { "Id", new AttributeValue { S = orderedProductId.ToString() } },
            { "ProductId", new AttributeValue { S = productId.ToString() } },
            { "Quantity", new AttributeValue { N = "2" } },
            { "FinalPrice", new AttributeValue { N = "25.00" } },
            { "ProductName", new AttributeValue { S = "Test Product" } },
            { "Category", new AttributeValue { N = "1" } },
            { "Observation", new AttributeValue { S = "Test observation" } }
        };

        item["Items"] = new AttributeValue
        {
            L = new List<AttributeValue> { new AttributeValue { M = itemMap } }
        };

        _dynamoDbMock
            .Setup(x => x.GetItemAsync(It.IsAny<GetItemRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetItemResponse { Item = item });

        // Act
        var result = await _repository.GetByIdAsync(orderId);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(orderedProductId, result.Items[0].Id);
        Assert.Equal(productId, result.Items[0].ProductId);
        Assert.Equal(2, result.Items[0].Quantity);
        Assert.Equal(25.00m, result.Items[0].FinalPrice);
        Assert.Equal("Test Product", result.Items[0].ProductName);
        Assert.Equal(1, result.Items[0].Category);
        Assert.Equal("Test observation", result.Items[0].Observation);
    }

    [Fact]
    public async Task GetByIdAsync_WithCustomIngredients_ShouldMapCustomIngredients()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var item = CreateOrderItem(orderId, customerId, "ORD-001", (int)EnumOrderStatus.Started, 50.00m);
        
        var orderedProductId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();
        
        var ingredientMap = new Dictionary<string, AttributeValue>
        {
            { "Id", new AttributeValue { S = ingredientId.ToString() } },
            { "Name", new AttributeValue { S = "Extra Cheese" } },
            { "Price", new AttributeValue { N = "2.50" } },
            { "Quantity", new AttributeValue { N = "2" } }
        };

        var itemMap = new Dictionary<string, AttributeValue>
        {
            { "Id", new AttributeValue { S = orderedProductId.ToString() } },
            { "ProductId", new AttributeValue { S = productId.ToString() } },
            { "Quantity", new AttributeValue { N = "1" } },
            { "FinalPrice", new AttributeValue { N = "15.00" } },
            { "CustomIngredients", new AttributeValue { L = new List<AttributeValue> { new AttributeValue { M = ingredientMap } } } }
        };

        item["Items"] = new AttributeValue
        {
            L = new List<AttributeValue> { new AttributeValue { M = itemMap } }
        };

        _dynamoDbMock
            .Setup(x => x.GetItemAsync(It.IsAny<GetItemRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetItemResponse { Item = item });

        // Act
        var result = await _repository.GetByIdAsync(orderId);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Single(result.Items[0].CustomIngredients);
        Assert.Equal(ingredientId, result.Items[0].CustomIngredients[0].Id);
        Assert.Equal("Extra Cheese", result.Items[0].CustomIngredients[0].Name);
        Assert.Equal(2.50m, result.Items[0].CustomIngredients[0].Price);
        Assert.Equal(2, result.Items[0].CustomIngredients[0].Quantity);
    }

    [Fact]
    public async Task AddAsync_WithItems_ShouldMapItemsToDynamoDb()
    {
        // Arrange
        var orderDto = new OrderDto
        {
            Id = Guid.NewGuid(),
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
                    Quantity = 2,
                    FinalPrice = 25.00m,
                    ProductName = "Test Product",
                    Category = 1,
                    Observation = "Test observation",
                    CustomIngredients = new List<OrderedProductIngredientDto>
                    {
                        new OrderedProductIngredientDto
                        {
                            Id = Guid.NewGuid(),
                            Name = "Extra Cheese",
                            Price = 2.50m,
                            Quantity = 2
                        }
                    }
                }
            }
        };

        PutItemRequest? capturedRequest = null;

        _dynamoDbMock
            .Setup(x => x.PutItemAsync(It.IsAny<PutItemRequest>(), It.IsAny<CancellationToken>()))
            .Callback<PutItemRequest, CancellationToken>((req, ct) => capturedRequest = req)
            .ReturnsAsync(new PutItemResponse());

        // Act
        await _repository.AddAsync(orderDto);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.True(capturedRequest.Item.ContainsKey("Items"));
        var items = capturedRequest.Item["Items"].L;
        Assert.Single(items);
        var itemMap = items[0].M;
        Assert.True(itemMap.ContainsKey("Id"));
        Assert.True(itemMap.ContainsKey("ProductId"));
        Assert.True(itemMap.ContainsKey("Quantity"));
        Assert.True(itemMap.ContainsKey("FinalPrice"));
        Assert.True(itemMap.ContainsKey("ProductName"));
        Assert.True(itemMap.ContainsKey("Category"));
        Assert.True(itemMap.ContainsKey("Observation"));
        Assert.True(itemMap.ContainsKey("CustomIngredients"));
    }

    [Fact]
    public async Task AddAsync_WithOrderSource_ShouldMapOrderSource()
    {
        // Arrange
        var orderDto = new OrderDto
        {
            Id = Guid.NewGuid(),
            Code = "ORD-001",
            CustomerId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            OrderStatus = (int)EnumOrderStatus.Started,
            TotalPrice = 50.00m,
            OrderSource = "MobileApp",
            Items = new List<OrderedProductDto>()
        };

        PutItemRequest? capturedRequest = null;

        _dynamoDbMock
            .Setup(x => x.PutItemAsync(It.IsAny<PutItemRequest>(), It.IsAny<CancellationToken>()))
            .Callback<PutItemRequest, CancellationToken>((req, ct) => capturedRequest = req)
            .ReturnsAsync(new PutItemResponse());

        // Act
        await _repository.AddAsync(orderDto);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.True(capturedRequest.Item.ContainsKey("OrderSource"));
        Assert.Equal("MobileApp", capturedRequest.Item["OrderSource"].S);
    }

    [Fact]
    public async Task GetByIdAsync_WithoutOptionalFields_ShouldHandleNulls()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var item = new Dictionary<string, AttributeValue>
        {
            { DynamoDbTableConfiguration.ORDER_ID_ATTRIBUTE, new AttributeValue { S = orderId.ToString() } },
            { DynamoDbTableConfiguration.CREATED_AT_ATTRIBUTE, new AttributeValue { S = DateTime.UtcNow.ToString("O") } },
            { DynamoDbTableConfiguration.ORDER_STATUS_ATTRIBUTE, new AttributeValue { N = ((int)EnumOrderStatus.Started).ToString() } },
            { "TotalPrice", new AttributeValue { N = "0" } }
            // Sem Code, CustomerId, OrderSource
        };

        _dynamoDbMock
            .Setup(x => x.GetItemAsync(It.IsAny<GetItemRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetItemResponse { Item = item });

        // Act
        var result = await _repository.GetByIdAsync(orderId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(orderId, result.Id);
        Assert.Null(result.Code);
        Assert.Null(result.CustomerId);
        Assert.Null(result.OrderSource);
    }

    [Fact]
    public async Task GetPagedAsync_WithPagination_ShouldHandleMultiplePages()
    {
        // Arrange
        var order1 = CreateOrderItem(Guid.NewGuid(), Guid.NewGuid(), "ORD-001", (int)EnumOrderStatus.Started, 50.00m);
        var order2 = CreateOrderItem(Guid.NewGuid(), Guid.NewGuid(), "ORD-002", (int)EnumOrderStatus.Started, 75.00m);
        var lastKey = new Dictionary<string, AttributeValue> { { "key", new AttributeValue { S = "value" } } };

        _dynamoDbMock
            .SetupSequence(x => x.ScanAsync(It.IsAny<ScanRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ScanResponse
            {
                Items = new List<Dictionary<string, AttributeValue>> { order1 },
                LastEvaluatedKey = lastKey
            })
            .ReturnsAsync(new ScanResponse
            {
                Items = new List<Dictionary<string, AttributeValue>> { order2 }
            });

        // Act
        var (items, returnedLastKey) = await _repository.GetPagedAsync(1, 1, null);

        // Assert
        Assert.NotNull(items);
        Assert.Single(items);
    }

    [Fact]
    public async Task GetPagedByStatusAsync_WithPagination_ShouldHandleMultiplePages()
    {
        // Arrange
        var status = (int)EnumOrderStatus.Started;
        var order1 = CreateOrderItem(Guid.NewGuid(), Guid.NewGuid(), "ORD-001", status, 50.00m);
        var order2 = CreateOrderItem(Guid.NewGuid(), Guid.NewGuid(), "ORD-002", status, 75.00m);
        var lastKey = new Dictionary<string, AttributeValue> { { "key", new AttributeValue { S = "value" } } };

        _dynamoDbMock
            .SetupSequence(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QueryResponse
            {
                Items = new List<Dictionary<string, AttributeValue>> { order1 },
                LastEvaluatedKey = lastKey
            })
            .ReturnsAsync(new QueryResponse
            {
                Items = new List<Dictionary<string, AttributeValue>> { order2 }
            });

        // Act
        var (items, returnedLastKey) = await _repository.GetPagedAsync(1, 1, status);

        // Assert
        Assert.NotNull(items);
        Assert.Single(items);
    }

    [Fact]
    public async Task GenerateOrderCodeAsync_WhenCodeExists_ShouldRetry()
    {
        // Arrange
        var existingCode = "ORD202501011234";

        _dynamoDbMock
            .SetupSequence(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QueryResponse
            {
                Items = new List<Dictionary<string, AttributeValue>>
                {
                    CreateOrderItem(Guid.NewGuid(), Guid.NewGuid(), existingCode, (int)EnumOrderStatus.Started, 50.00m)
                }
            })
            .ReturnsAsync(new QueryResponse
            {
                Items = new List<Dictionary<string, AttributeValue>>()
            });

        // Act
        var result = await _repository.GenerateOrderCodeAsync();

        // Assert
        Assert.NotNull(result);
        Assert.StartsWith("ORD", result);
        _dynamoDbMock.Verify(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()), Times.AtLeast(2));
    }

    [Fact]
    public async Task AddAsync_WithOrderSource_ShouldIncludeOrderSourceInItem()
    {
        // Arrange
        var orderDto = new OrderDto
        {
            Id = Guid.NewGuid(),
            Code = "ORD-001",
            CustomerId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            OrderStatus = (int)EnumOrderStatus.Started,
            TotalPrice = 50.00m,
            OrderSource = "WebApp",
            Items = new List<OrderedProductDto>()
        };

        PutItemRequest? capturedRequest = null;

        _dynamoDbMock
            .Setup(x => x.PutItemAsync(It.IsAny<PutItemRequest>(), It.IsAny<CancellationToken>()))
            .Callback<PutItemRequest, CancellationToken>((req, ct) => capturedRequest = req)
            .ReturnsAsync(new PutItemResponse());

        // Act
        await _repository.AddAsync(orderDto);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.True(capturedRequest.Item.ContainsKey("OrderSource"));
        Assert.Equal("WebApp", capturedRequest.Item["OrderSource"].S);
    }

    private Dictionary<string, AttributeValue> CreateOrderItem(Guid id, Guid customerId, string code, int status, decimal totalPrice)
    {
        return new Dictionary<string, AttributeValue>
        {
            { DynamoDbTableConfiguration.ORDER_ID_ATTRIBUTE, new AttributeValue { S = id.ToString() } },
            { DynamoDbTableConfiguration.CODE_ATTRIBUTE, new AttributeValue { S = code } },
            { DynamoDbTableConfiguration.CUSTOMER_ID_ATTRIBUTE, new AttributeValue { S = customerId.ToString() } },
            { DynamoDbTableConfiguration.CREATED_AT_ATTRIBUTE, new AttributeValue { S = DateTime.UtcNow.ToString("O") } },
            { DynamoDbTableConfiguration.ORDER_STATUS_ATTRIBUTE, new AttributeValue { N = status.ToString() } },
            { "TotalPrice", new AttributeValue { N = totalPrice.ToString("F2", CultureInfo.InvariantCulture) } },
            { "Items", new AttributeValue { L = new List<AttributeValue>() } }
        };
    }
}
