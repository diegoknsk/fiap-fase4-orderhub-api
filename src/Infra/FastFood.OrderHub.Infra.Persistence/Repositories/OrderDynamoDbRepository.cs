using System.Globalization;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using FastFood.OrderHub.Application.DTOs;
using FastFood.OrderHub.Infra.Persistence.Configurations;

namespace FastFood.OrderHub.Infra.Persistence.Repositories;

/// <summary>
/// Repositório DynamoDB para pedidos
/// </summary>
public class OrderDynamoDbRepository
{
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private readonly string _tableName;
    private const int MAX_ITEM_SIZE_KB = 400;

    public OrderDynamoDbRepository(IAmazonDynamoDB dynamoDbClient)
    {
        _dynamoDbClient = dynamoDbClient;
        _tableName = DynamoDbTableConfiguration.FASTFOOD_ORDERS_TABLE;
    }

    /// <summary>
    /// Obtém pedido por ID
    /// </summary>
    public async Task<OrderDto?> GetByIdAsync(Guid id)
    {
        var request = new GetItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { DynamoDbTableConfiguration.ORDER_ID_ATTRIBUTE, new AttributeValue { S = id.ToString() } }
            }
        };

        var response = await _dynamoDbClient.GetItemAsync(request);

        if (!response.Item.Any())
            return null;

        return MapFromDynamoDb(response.Item);
    }

    /// <summary>
    /// Obtém pedidos por CustomerId usando GSI
    /// </summary>
    public async Task<List<OrderDto>> GetByCustomerIdAsync(Guid customerId)
    {
        var request = new QueryRequest
        {
            TableName = _tableName,
            IndexName = DynamoDbTableConfiguration.ORDER_CUSTOMER_ID_INDEX,
            KeyConditionExpression = $"{DynamoDbTableConfiguration.CUSTOMER_ID_ATTRIBUTE} = :customerId",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":customerId", new AttributeValue { S = customerId.ToString() } }
            }
        };

        var response = await _dynamoDbClient.QueryAsync(request);
        return response.Items.Select(MapFromDynamoDb).ToList();
    }

    /// <summary>
    /// Obtém pedidos paginados (com filtro opcional por status)
    /// </summary>
    public async Task<(List<OrderDto> Items, Dictionary<string, AttributeValue>? LastEvaluatedKey)> GetPagedAsync(
        int page, 
        int pageSize, 
        int? status = null)
    {
        // Para paginação, precisamos fazer Scan ou Query
        // Se status for fornecido, usar GSI Status-CreatedAt-Index
        if (status.HasValue)
        {
            return await GetPagedByStatusAsync(status.Value, page, pageSize);
        }

        // Caso contrário, usar Scan
        var request = new ScanRequest
        {
            TableName = _tableName,
            Limit = pageSize
        };

        // Calcular ExclusiveStartKey baseado na página
        // Nota: DynamoDB não suporta Skip, então precisamos fazer múltiplas chamadas ou usar LastEvaluatedKey
        // Para simplificar, vamos fazer Scan e pular itens manualmente (não ideal, mas funcional)
        var allItems = new List<OrderDto>();
        Dictionary<string, AttributeValue>? lastKey = null;
        int itemsToSkip = (page - 1) * pageSize;
        int itemsToTake = pageSize;
        int itemsSkipped = 0;

        do
        {
            if (lastKey != null)
            {
                request.ExclusiveStartKey = lastKey;
            }

            var response = await _dynamoDbClient.ScanAsync(request);
            
            foreach (var item in response.Items)
            {
                if (itemsSkipped < itemsToSkip)
                {
                    itemsSkipped++;
                    continue;
                }

                if (allItems.Count >= itemsToTake)
                    break;

                allItems.Add(MapFromDynamoDb(item));
            }

            lastKey = response.LastEvaluatedKey;
        } while (allItems.Count < itemsToTake && lastKey != null && lastKey.Any());

        return (allItems, lastKey);
    }

    /// <summary>
    /// Obtém pedidos paginados por status usando GSI
    /// </summary>
    private async Task<(List<OrderDto> Items, Dictionary<string, AttributeValue>? LastEvaluatedKey)> GetPagedByStatusAsync(
        int status, 
        int page, 
        int pageSize)
    {
        var request = new QueryRequest
        {
            TableName = _tableName,
            IndexName = DynamoDbTableConfiguration.ORDER_STATUS_INDEX,
            KeyConditionExpression = $"{DynamoDbTableConfiguration.ORDER_STATUS_ATTRIBUTE} = :status",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":status", new AttributeValue { N = status.ToString() } }
            },
            Limit = pageSize
        };

        // Calcular ExclusiveStartKey baseado na página
        int itemsToSkip = (page - 1) * pageSize;
        int itemsSkipped = 0;
        Dictionary<string, AttributeValue>? lastKey = null;
        var allItems = new List<OrderDto>();

        do
        {
            if (lastKey != null)
            {
                request.ExclusiveStartKey = lastKey;
            }

            var response = await _dynamoDbClient.QueryAsync(request);

            foreach (var item in response.Items)
            {
                if (itemsSkipped < itemsToSkip)
                {
                    itemsSkipped++;
                    continue;
                }

                if (allItems.Count >= pageSize)
                    break;

                allItems.Add(MapFromDynamoDb(item));
            }

            lastKey = response.LastEvaluatedKey;
        } while (allItems.Count < pageSize && lastKey != null && lastKey.Any());

        return (allItems, lastKey);
    }

    /// <summary>
    /// Obtém pedidos por status
    /// </summary>
    public async Task<List<OrderDto>> GetByStatusAsync(int status)
    {
        var request = new QueryRequest
        {
            TableName = _tableName,
            IndexName = DynamoDbTableConfiguration.ORDER_STATUS_INDEX,
            KeyConditionExpression = $"{DynamoDbTableConfiguration.ORDER_STATUS_ATTRIBUTE} = :status",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":status", new AttributeValue { N = status.ToString() } }
            }
        };

        var response = await _dynamoDbClient.QueryAsync(request);
        return response.Items.Select(MapFromDynamoDb).ToList();
    }

    /// <summary>
    /// Obtém pedidos por status sem preparação (filtro adicional)
    /// </summary>
    public async Task<List<OrderDto>> GetByStatusWithoutPreparationAsync(int status)
    {
        var orders = await GetByStatusAsync(status);
        // Filtrar pedidos que não estão em preparação
        return orders.Where(o => o.OrderStatus != (int)Domain.Common.Enums.EnumOrderStatus.InPreparation).ToList();
    }

    /// <summary>
    /// Adiciona um pedido
    /// </summary>
    public async Task AddAsync(OrderDto dto)
    {
        ValidateItemSize(dto);
        var item = MapToDynamoDb(dto);
        var request = new PutItemRequest
        {
            TableName = _tableName,
            Item = item
        };

        await _dynamoDbClient.PutItemAsync(request);
    }

    /// <summary>
    /// Atualiza um pedido
    /// </summary>
    public async Task UpdateAsync(OrderDto dto)
    {
        ValidateItemSize(dto);
        var item = MapToDynamoDb(dto);
        var request = new PutItemRequest
        {
            TableName = _tableName,
            Item = item
        };

        await _dynamoDbClient.PutItemAsync(request);
    }

    /// <summary>
    /// Remove um pedido
    /// </summary>
    public async Task DeleteAsync(Guid id)
    {
        var request = new DeleteItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { DynamoDbTableConfiguration.ORDER_ID_ATTRIBUTE, new AttributeValue { S = id.ToString() } }
            }
        };

        await _dynamoDbClient.DeleteItemAsync(request);
    }

    /// <summary>
    /// Verifica se um pedido existe
    /// </summary>
    public async Task<bool> ExistsAsync(Guid id)
    {
        var order = await GetByIdAsync(id);
        return order != null;
    }

    /// <summary>
    /// Verifica se um código de pedido existe usando GSI
    /// </summary>
    public async Task<bool> ExistsByCodeAsync(string code)
    {
        var request = new QueryRequest
        {
            TableName = _tableName,
            IndexName = DynamoDbTableConfiguration.ORDER_CODE_INDEX,
            KeyConditionExpression = $"{DynamoDbTableConfiguration.CODE_ATTRIBUTE} = :code",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":code", new AttributeValue { S = code } }
            },
            Limit = 1
        };

        var response = await _dynamoDbClient.QueryAsync(request);
        return response.Items.Any();
    }

    /// <summary>
    /// Gera código único para pedido (formato: ORD{yyyyMMdd}{random})
    /// </summary>
    public async Task<string> GenerateOrderCodeAsync()
    {
        string code;
        int attempts = 0;
        const int maxAttempts = 10;

        do
        {
            var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
            var randomPart = new Random().Next(1000, 9999).ToString();
            code = $"ORD{datePart}{randomPart}";
            attempts++;

            if (attempts >= maxAttempts)
                throw new InvalidOperationException("Não foi possível gerar código único para o pedido após várias tentativas.");
        } while (await ExistsByCodeAsync(code));

        return code;
    }

    /// <summary>
    /// Valida se o tamanho do item está dentro do limite (400KB)
    /// </summary>
    private void ValidateItemSize(OrderDto dto)
    {
        // Estimativa aproximada do tamanho (em bytes)
        // DynamoDB limita a 400KB por item
        var estimatedSize = EstimateItemSize(dto);
        if (estimatedSize > MAX_ITEM_SIZE_KB * 1024)
        {
            throw new InvalidOperationException($"O pedido excede o limite de tamanho do DynamoDB ({MAX_ITEM_SIZE_KB}KB). Tamanho estimado: {estimatedSize / 1024}KB");
        }
    }

    /// <summary>
    /// Estima o tamanho do item em bytes
    /// </summary>
    private int EstimateItemSize(OrderDto dto)
    {
        int size = 0;
        
        // Atributos básicos
        size += dto.Id.ToString().Length;
        size += (dto.Code?.Length ?? 0);
        size += (dto.CustomerId?.ToString().Length ?? 0);
        size += dto.CreatedAt.ToString("O").Length;
        size += dto.OrderStatus.ToString().Length;
        size += dto.TotalPrice.ToString().Length;
        size += (dto.OrderSource?.Length ?? 0);

        // Items (OrderedProducts)
        foreach (var item in dto.Items)
        {
            size += item.Id.ToString().Length;
            size += item.ProductId.ToString().Length;
            size += (item.ProductName?.Length ?? 0);
            size += (item.Category?.ToString().Length ?? 0);
            size += item.Quantity.ToString().Length;
            size += item.FinalPrice.ToString().Length;
            size += (item.Observation?.Length ?? 0);

            // CustomIngredients
            foreach (var ingredient in item.CustomIngredients)
            {
                size += ingredient.Id.ToString().Length;
                size += (ingredient.Name?.Length ?? 0);
                size += ingredient.Price.ToString().Length;
                size += ingredient.Quantity.ToString().Length;
            }
        }

        // Adicionar overhead de estrutura DynamoDB (aproximadamente 20%)
        return (int)(size * 1.2);
    }

    /// <summary>
    /// Mapeia OrderDto para atributos DynamoDB
    /// </summary>
    private Dictionary<string, AttributeValue> MapToDynamoDb(OrderDto dto)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            { DynamoDbTableConfiguration.ORDER_ID_ATTRIBUTE, new AttributeValue { S = dto.Id.ToString() } },
            { DynamoDbTableConfiguration.CREATED_AT_ATTRIBUTE, new AttributeValue { S = dto.CreatedAt.ToString("O") } },
            { DynamoDbTableConfiguration.ORDER_STATUS_ATTRIBUTE, new AttributeValue { N = dto.OrderStatus.ToString() } },
            { "TotalPrice", new AttributeValue { N = dto.TotalPrice.ToString("F2", CultureInfo.InvariantCulture) } }
        };

        if (!string.IsNullOrWhiteSpace(dto.Code))
        {
            item[DynamoDbTableConfiguration.CODE_ATTRIBUTE] = new AttributeValue { S = dto.Code };
        }

        if (dto.CustomerId.HasValue)
        {
            item[DynamoDbTableConfiguration.CUSTOMER_ID_ATTRIBUTE] = new AttributeValue { S = dto.CustomerId.Value.ToString() };
        }

        if (!string.IsNullOrWhiteSpace(dto.OrderSource))
        {
            item["OrderSource"] = new AttributeValue { S = dto.OrderSource };
        }

        // Mapear Items (OrderedProducts) como lista de mapas
        if (dto.Items.Any())
        {
            var itemsList = new List<AttributeValue>();
            foreach (var orderedProduct in dto.Items)
            {
                var itemMap = new Dictionary<string, AttributeValue>
                {
                    { "Id", new AttributeValue { S = orderedProduct.Id.ToString() } },
                    { "ProductId", new AttributeValue { S = orderedProduct.ProductId.ToString() } },
                    { "Quantity", new AttributeValue { N = orderedProduct.Quantity.ToString() } },
                    { "FinalPrice", new AttributeValue { N = orderedProduct.FinalPrice.ToString("F2", CultureInfo.InvariantCulture) } }
                };

                if (!string.IsNullOrWhiteSpace(orderedProduct.ProductName))
                    itemMap["ProductName"] = new AttributeValue { S = orderedProduct.ProductName };

                if (orderedProduct.Category.HasValue)
                    itemMap["Category"] = new AttributeValue { N = orderedProduct.Category.Value.ToString() };

                if (!string.IsNullOrWhiteSpace(orderedProduct.Observation))
                    itemMap["Observation"] = new AttributeValue { S = orderedProduct.Observation };

                // CustomIngredients
                if (orderedProduct.CustomIngredients.Any())
                {
                    var ingredientsList = new List<AttributeValue>();
                    foreach (var ingredient in orderedProduct.CustomIngredients)
                    {
                        var ingredientMap = new Dictionary<string, AttributeValue>
                        {
                            { "Id", new AttributeValue { S = ingredient.Id.ToString() } },
                            { "Name", new AttributeValue { S = ingredient.Name ?? string.Empty } },
                            { "Price", new AttributeValue { N = ingredient.Price.ToString("F2", CultureInfo.InvariantCulture) } },
                            { "Quantity", new AttributeValue { N = ingredient.Quantity.ToString() } }
                        };
                        ingredientsList.Add(new AttributeValue { M = ingredientMap });
                    }
                    itemMap["CustomIngredients"] = new AttributeValue { L = ingredientsList };
                }

                itemsList.Add(new AttributeValue { M = itemMap });
            }
            item["Items"] = new AttributeValue { L = itemsList };
        }

        return item;
    }

    /// <summary>
    /// Mapeia atributos DynamoDB para OrderDto
    /// </summary>
    private OrderDto MapFromDynamoDb(Dictionary<string, AttributeValue> item)
    {
        var dto = new OrderDto
        {
            Id = Guid.Parse(item[DynamoDbTableConfiguration.ORDER_ID_ATTRIBUTE].S),
            OrderStatus = int.Parse(item[DynamoDbTableConfiguration.ORDER_STATUS_ATTRIBUTE].N),
            TotalPrice = item.ContainsKey("TotalPrice") ? decimal.Parse(item["TotalPrice"].N, CultureInfo.InvariantCulture) : 0
        };

        if (item.ContainsKey(DynamoDbTableConfiguration.CODE_ATTRIBUTE))
            dto.Code = item[DynamoDbTableConfiguration.CODE_ATTRIBUTE].S;

        if (item.ContainsKey(DynamoDbTableConfiguration.CUSTOMER_ID_ATTRIBUTE))
            dto.CustomerId = Guid.Parse(item[DynamoDbTableConfiguration.CUSTOMER_ID_ATTRIBUTE].S);

        if (item.ContainsKey(DynamoDbTableConfiguration.CREATED_AT_ATTRIBUTE))
            dto.CreatedAt = DateTime.Parse(item[DynamoDbTableConfiguration.CREATED_AT_ATTRIBUTE].S);

        if (item.ContainsKey("OrderSource"))
            dto.OrderSource = item["OrderSource"].S;

        // Mapear Items
        if (item.ContainsKey("Items") && item["Items"].L != null)
        {
            foreach (var itemAttr in item["Items"].L)
            {
                var itemMap = itemAttr.M;
                var orderedProduct = new OrderedProductDto
                {
                    Id = Guid.Parse(itemMap["Id"].S),
                    ProductId = Guid.Parse(itemMap["ProductId"].S),
                    Quantity = int.Parse(itemMap["Quantity"].N),
                    FinalPrice = decimal.Parse(itemMap["FinalPrice"].N, CultureInfo.InvariantCulture)
                };

                if (itemMap.ContainsKey("ProductName"))
                    orderedProduct.ProductName = itemMap["ProductName"].S;

                if (itemMap.ContainsKey("Category"))
                    orderedProduct.Category = int.Parse(itemMap["Category"].N);

                if (itemMap.ContainsKey("Observation"))
                    orderedProduct.Observation = itemMap["Observation"].S;

                // CustomIngredients
                if (itemMap.ContainsKey("CustomIngredients") && itemMap["CustomIngredients"].L != null)
                {
                    foreach (var ingredientAttr in itemMap["CustomIngredients"].L)
                    {
                        var ingredientMap = ingredientAttr.M;
                        orderedProduct.CustomIngredients.Add(new OrderedProductIngredientDto
                        {
                            Id = Guid.Parse(ingredientMap["Id"].S),
                            Name = ingredientMap.ContainsKey("Name") ? ingredientMap["Name"].S : null,
                            Price = ingredientMap.ContainsKey("Price") ? decimal.Parse(ingredientMap["Price"].N, CultureInfo.InvariantCulture) : 0,
                            Quantity = ingredientMap.ContainsKey("Quantity") ? int.Parse(ingredientMap["Quantity"].N) : 0
                        });
                    }
                }

                dto.Items.Add(orderedProduct);
            }
        }

        return dto;
    }
}

