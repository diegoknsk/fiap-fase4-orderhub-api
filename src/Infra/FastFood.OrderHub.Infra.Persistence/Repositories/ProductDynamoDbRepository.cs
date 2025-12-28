using System.Globalization;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using FastFood.OrderHub.Application.DTOs;
using FastFood.OrderHub.Infra.Persistence.Configurations;

namespace FastFood.OrderHub.Infra.Persistence.Repositories;

/// <summary>
/// Repositório DynamoDB para produtos
/// </summary>
public class ProductDynamoDbRepository
{
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private readonly string _tableName;

    public ProductDynamoDbRepository(IAmazonDynamoDB dynamoDbClient)
    {
        _dynamoDbClient = dynamoDbClient;
        _tableName = DynamoDbTableConfiguration.FASTFOOD_PRODUCTS_TABLE;
    }

    /// <summary>
    /// Obtém produto por ID
    /// </summary>
    public async Task<ProductDto?> GetByIdAsync(Guid id)
    {
        try
        {
            var request = new GetItemRequest
            {
                TableName = _tableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { DynamoDbTableConfiguration.PRODUCT_ID_ATTRIBUTE, new AttributeValue { S = id.ToString() } }
                }
            };

            var response = await _dynamoDbClient.GetItemAsync(request);

            if (!response.Item.Any())
                return null;

            return MapFromDynamoDb(response.Item);
        }
        catch (AmazonDynamoDBException ex) when (ex.Message.Contains("does not match the schema"))
        {
            // Tentar buscar usando Scan como fallback (para debug)
            // Isso pode ajudar a identificar o formato correto da chave
            throw new InvalidOperationException(
                $"Erro ao buscar produto: A chave '{DynamoDbTableConfiguration.PRODUCT_ID_ATTRIBUTE}' não corresponde ao schema. " +
                $"Verifique se o item foi inserido com a chave primária '{DynamoDbTableConfiguration.PRODUCT_ID_ATTRIBUTE}' (snake_case). " +
                $"ID usado: {id}", ex);
        }
    }

    /// <summary>
    /// Obtém todos os produtos (Scan)
    /// </summary>
    public async Task<List<ProductDto>> GetAllAsync()
    {
        var request = new ScanRequest
        {
            TableName = _tableName
        };

        var response = await _dynamoDbClient.ScanAsync(request);
        return response.Items.Select(MapFromDynamoDb).ToList();
    }

    /// <summary>
    /// Obtém produtos por categoria usando GSI
    /// </summary>
    public async Task<List<ProductDto>> GetByCategoryAsync(int category)
    {
        var request = new QueryRequest
        {
            TableName = _tableName,
            IndexName = DynamoDbTableConfiguration.PRODUCT_CATEGORY_INDEX,
            KeyConditionExpression = $"{DynamoDbTableConfiguration.CATEGORY_ATTRIBUTE} = :category",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":category", new AttributeValue { N = category.ToString() } }
            }
        };

        var response = await _dynamoDbClient.QueryAsync(request);
        return response.Items.Select(MapFromDynamoDb).ToList();
    }

    /// <summary>
    /// Adiciona um produto
    /// </summary>
    public async Task AddAsync(ProductDto dto)
    {
        var item = MapToDynamoDb(dto);
        var request = new PutItemRequest
        {
            TableName = _tableName,
            Item = item
        };

        await _dynamoDbClient.PutItemAsync(request);
    }

    /// <summary>
    /// Atualiza um produto
    /// </summary>
    public async Task UpdateAsync(ProductDto dto)
    {
        var item = MapToDynamoDb(dto);
        var request = new PutItemRequest
        {
            TableName = _tableName,
            Item = item
        };

        await _dynamoDbClient.PutItemAsync(request);
    }

    /// <summary>
    /// Remove um produto (soft delete: IsActive = false)
    /// </summary>
    public async Task DeleteAsync(Guid id)
    {
        var request = new UpdateItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { DynamoDbTableConfiguration.PRODUCT_ID_ATTRIBUTE, new AttributeValue { S = id.ToString() } }
            },
            UpdateExpression = "SET IsActive = :isActive",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":isActive", new AttributeValue { BOOL = false } }
            }
        };

        await _dynamoDbClient.UpdateItemAsync(request);
    }

    /// <summary>
    /// Verifica se um produto existe
    /// </summary>
    public async Task<bool> ExistsAsync(Guid id)
    {
        var product = await GetByIdAsync(id);
        return product != null;
    }

    /// <summary>
    /// Obtém produtos paginados (com filtros opcionais por categoria e nome)
    /// </summary>
    public async Task<List<ProductDto>> GetPagedAsync(int page, int pageSize, int? category = null, string? name = null)
    {
        // Se categoria fornecida, usar GSI
        if (category.HasValue)
        {
            return await GetPagedByCategoryAsync(category.Value, page, pageSize, name);
        }

        // Caso contrário, usar Scan com filtros
        var request = new ScanRequest
        {
            TableName = _tableName,
            Limit = pageSize
        };

        // Adicionar filtros
        var filterExpressions = new List<string>();
        var expressionAttributeValues = new Dictionary<string, AttributeValue>();

        if (!string.IsNullOrWhiteSpace(name))
        {
            filterExpressions.Add("contains(Name, :name)");
            expressionAttributeValues[":name"] = new AttributeValue { S = name };
        }

        if (filterExpressions.Any())
        {
            request.FilterExpression = string.Join(" AND ", filterExpressions);
            request.ExpressionAttributeValues = expressionAttributeValues;
        }

        // Calcular paginação
        var allItems = new List<ProductDto>();
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

        return allItems;
    }

    /// <summary>
    /// Obtém produtos paginados por categoria usando GSI
    /// </summary>
    private async Task<List<ProductDto>> GetPagedByCategoryAsync(int category, int page, int pageSize, string? name = null)
    {
        var request = new QueryRequest
        {
            TableName = _tableName,
            IndexName = DynamoDbTableConfiguration.PRODUCT_CATEGORY_INDEX,
            KeyConditionExpression = $"{DynamoDbTableConfiguration.CATEGORY_ATTRIBUTE} = :category",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":category", new AttributeValue { N = category.ToString() } }
            },
            Limit = pageSize
        };

        // Adicionar filtro por nome se fornecido
        if (!string.IsNullOrWhiteSpace(name))
        {
            request.FilterExpression = "contains(Name, :name)";
            request.ExpressionAttributeValues[":name"] = new AttributeValue { S = name };
        }

        // Calcular paginação
        int itemsToSkip = (page - 1) * pageSize;
        int itemsSkipped = 0;
        Dictionary<string, AttributeValue>? lastKey = null;
        var allItems = new List<ProductDto>();

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

        return allItems;
    }

    /// <summary>
    /// Mapeia ProductDto para atributos DynamoDB
    /// </summary>
    private Dictionary<string, AttributeValue> MapToDynamoDb(ProductDto dto)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            { DynamoDbTableConfiguration.PRODUCT_ID_ATTRIBUTE, new AttributeValue { S = dto.Id.ToString() } },
            { "Name", new AttributeValue { S = dto.Name ?? string.Empty } },
            { DynamoDbTableConfiguration.CATEGORY_ATTRIBUTE, new AttributeValue { N = dto.Category.ToString() } },
            { "Price", new AttributeValue { N = dto.Price.ToString("F2", CultureInfo.InvariantCulture) } },
            { "IsActive", new AttributeValue { BOOL = dto.IsActive } },
            { DynamoDbTableConfiguration.CREATED_AT_ATTRIBUTE, new AttributeValue { S = dto.CreatedAt.ToString("O") } }
        };

        if (!string.IsNullOrWhiteSpace(dto.Description))
            item["Description"] = new AttributeValue { S = dto.Description };

        if (!string.IsNullOrWhiteSpace(dto.ImageUrl))
            item["ImageUrl"] = new AttributeValue { S = dto.ImageUrl };

        // Mapear BaseIngredients como lista de mapas
        if (dto.BaseIngredients.Any())
        {
            var ingredientsList = new List<AttributeValue>();
            foreach (var ingredient in dto.BaseIngredients)
            {
                var ingredientMap = new Dictionary<string, AttributeValue>
                {
                    { "Id", new AttributeValue { S = ingredient.Id.ToString() } },
                    { "Name", new AttributeValue { S = ingredient.Name ?? string.Empty } },
                    { "Price", new AttributeValue { N = ingredient.Price.ToString("F2", CultureInfo.InvariantCulture) } }
                };
                ingredientsList.Add(new AttributeValue { M = ingredientMap });
            }
            item["BaseIngredients"] = new AttributeValue { L = ingredientsList };
        }

        return item;
    }

    /// <summary>
    /// Mapeia atributos DynamoDB para ProductDto
    /// </summary>
    private ProductDto MapFromDynamoDb(Dictionary<string, AttributeValue> item)
    {
        var dto = new ProductDto
        {
            Id = Guid.Parse(item[DynamoDbTableConfiguration.PRODUCT_ID_ATTRIBUTE].S),
            Name = item.ContainsKey("Name") ? item["Name"].S : null,
            Category = item.ContainsKey(DynamoDbTableConfiguration.CATEGORY_ATTRIBUTE) 
                ? int.Parse(item[DynamoDbTableConfiguration.CATEGORY_ATTRIBUTE].N) 
                : 0,
            Price = item.ContainsKey("Price") ? decimal.Parse(item["Price"].N, CultureInfo.InvariantCulture) : 0,
            IsActive = item.ContainsKey("IsActive") ? item["IsActive"].BOOL : true,
            Description = item.ContainsKey("Description") ? item["Description"].S : null,
            ImageUrl = item.ContainsKey("ImageUrl") ? item["ImageUrl"].S : null
        };

        if (item.ContainsKey(DynamoDbTableConfiguration.CREATED_AT_ATTRIBUTE))
        {
            dto.CreatedAt = DateTime.Parse(item[DynamoDbTableConfiguration.CREATED_AT_ATTRIBUTE].S);
        }

        // Mapear BaseIngredients
        if (item.ContainsKey("BaseIngredients") && item["BaseIngredients"].L != null)
        {
            foreach (var ingredientAttr in item["BaseIngredients"].L)
            {
                var ingredientMap = ingredientAttr.M;
                dto.BaseIngredients.Add(new ProductBaseIngredientDto
                {
                    Id = Guid.Parse(ingredientMap["Id"].S),
                    Name = ingredientMap.ContainsKey("Name") ? ingredientMap["Name"].S : null,
                    Price = ingredientMap.ContainsKey("Price") ? decimal.Parse(ingredientMap["Price"].N, CultureInfo.InvariantCulture) : 0,
                    ProductId = dto.Id
                });
            }
        }

        return dto;
    }
}

