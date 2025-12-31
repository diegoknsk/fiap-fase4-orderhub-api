using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using FastFood.OrderHub.Infra.Persistence.Configurations;
using Microsoft.Extensions.Configuration;

namespace FastFood.OrderHub.Migrator;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Iniciando migração do DynamoDB...");

        // Configurar DynamoDB client
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables() // Converte DynamoDb__AccessKey para DynamoDb:AccessKey automaticamente
            .Build();

        var dynamoDbConfig = configuration.GetSection("DynamoDb").Get<DynamoDbConfiguration>();
        
        if (dynamoDbConfig == null)
        {
            Console.WriteLine("ERRO: Configuração do DynamoDB não encontrada!");
            Console.WriteLine("Verificando variáveis de ambiente...");
            Console.WriteLine($"DynamoDb__AccessKey presente: {!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DynamoDb__AccessKey"))}");
            Console.WriteLine($"DynamoDb__SecretKey presente: {!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DynamoDb__SecretKey"))}");
            Console.WriteLine($"DynamoDb__SessionToken presente: {!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DynamoDb__SessionToken"))}");
            Console.WriteLine($"DynamoDb__Region: {Environment.GetEnvironmentVariable("DynamoDb__Region") ?? "não definido"}");
            Environment.Exit(1);
        }

        // Validar credenciais
        if (string.IsNullOrWhiteSpace(dynamoDbConfig.AccessKey) || string.IsNullOrWhiteSpace(dynamoDbConfig.SecretKey))
        {
            Console.WriteLine("ERRO: AccessKey ou SecretKey do DynamoDB não configurados!");
            Environment.Exit(1);
        }

        Console.WriteLine($"Configuração do DynamoDB carregada:");
        Console.WriteLine($"  Region: {dynamoDbConfig.Region}");
        Console.WriteLine($"  AccessKey: {dynamoDbConfig.AccessKey.Substring(0, Math.Min(10, dynamoDbConfig.AccessKey.Length))}...");
        Console.WriteLine($"  SessionToken presente: {!string.IsNullOrWhiteSpace(dynamoDbConfig.SessionToken)}");

        var client = dynamoDbConfig.CreateDynamoDbClient();

        try
        {
            // Criar tabelas
            await CreateProductTableAsync(client);
            await CreateOrderTableAsync(client);

            // Carregar dados iniciais
            await SeedInitialProductsAsync(client);

            Console.WriteLine("Migração concluída com sucesso!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro durante a migração: {ex.Message}");
            Environment.Exit(1);
        }
    }

    static async Task CreateProductTableAsync(IAmazonDynamoDB client)
    {
        var tableName = DynamoDbTableConfiguration.FASTFOOD_PRODUCTS_TABLE;

        // Verificar se a tabela já existe
        TableDescription? existingTable = null;
        try
        {
            var describeRequest = new DescribeTableRequest { TableName = tableName };
            var response = await client.DescribeTableAsync(describeRequest);
            existingTable = response.Table;
            Console.WriteLine($"Tabela {tableName} já existe.");
        }
        catch (ResourceNotFoundException)
        {
            // Tabela não existe, continuar com a criação
        }

        // Se tabela existe, verificar e criar GSIs faltantes
        if (existingTable != null)
        {
            await EnsureProductTableIndexesAsync(client, tableName, existingTable);
            return;
        }

        Console.WriteLine($"Criando tabela {tableName}...");

        var request = new CreateTableRequest
        {
            TableName = tableName,
            AttributeDefinitions = new List<AttributeDefinition>
            {
                new AttributeDefinition
                {
                    AttributeName = DynamoDbTableConfiguration.PRODUCT_ID_ATTRIBUTE,
                    AttributeType = ScalarAttributeType.S
                },
                new AttributeDefinition
                {
                    AttributeName = DynamoDbTableConfiguration.CATEGORY_ATTRIBUTE,
                    AttributeType = ScalarAttributeType.N
                },
                new AttributeDefinition
                {
                    AttributeName = DynamoDbTableConfiguration.NAME_ATTRIBUTE,
                    AttributeType = ScalarAttributeType.S
                }
            },
            KeySchema = new List<KeySchemaElement>
            {
                new KeySchemaElement
                {
                    AttributeName = DynamoDbTableConfiguration.PRODUCT_ID_ATTRIBUTE,
                    KeyType = KeyType.HASH
                }
            },
            GlobalSecondaryIndexes = new List<GlobalSecondaryIndex>
            {
                new GlobalSecondaryIndex
                {
                    IndexName = DynamoDbTableConfiguration.PRODUCT_CATEGORY_INDEX,
                    KeySchema = new List<KeySchemaElement>
                    {
                        new KeySchemaElement
                        {
                            AttributeName = DynamoDbTableConfiguration.CATEGORY_ATTRIBUTE,
                            KeyType = KeyType.HASH
                        },
                        new KeySchemaElement
                        {
                            AttributeName = DynamoDbTableConfiguration.NAME_ATTRIBUTE,
                            KeyType = KeyType.RANGE
                        }
                    },
                    Projection = new Projection { ProjectionType = ProjectionType.ALL }
                }
            },
            BillingMode = BillingMode.PAY_PER_REQUEST
        };

        await client.CreateTableAsync(request);
        Console.WriteLine($"Tabela {tableName} criada com sucesso!");

        // Aguardar tabela ficar ativa
        await WaitForTableToBeActiveAsync(client, tableName);
    }

    static async Task EnsureProductTableIndexesAsync(IAmazonDynamoDB client, string tableName, TableDescription existingTable)
    {
        var existingIndexNames = existingTable.GlobalSecondaryIndexes?.Select(gsi => gsi.IndexName).ToList() ?? new List<string>();
        var requiredIndexName = DynamoDbTableConfiguration.PRODUCT_CATEGORY_INDEX;

        if (!existingIndexNames.Contains(requiredIndexName))
        {
            Console.WriteLine($"Criando GSI {requiredIndexName} na tabela {tableName}...");
            
            var isOnDemand = existingTable.BillingModeSummary?.BillingMode == BillingMode.PAY_PER_REQUEST;
            
            var createIndexAction = new CreateGlobalSecondaryIndexAction
            {
                IndexName = requiredIndexName,
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement
                    {
                        AttributeName = DynamoDbTableConfiguration.CATEGORY_ATTRIBUTE,
                        KeyType = KeyType.HASH
                    },
                    new KeySchemaElement
                    {
                        AttributeName = DynamoDbTableConfiguration.NAME_ATTRIBUTE,
                        KeyType = KeyType.RANGE
                    }
                },
                Projection = new Projection { ProjectionType = ProjectionType.ALL }
            };

            // Só adiciona ProvisionedThroughput se não for PAY_PER_REQUEST
            if (!isOnDemand)
            {
                createIndexAction.ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = 5,
                    WriteCapacityUnits = 5
                };
            }

            var updateRequest = new UpdateTableRequest
            {
                TableName = tableName,
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition
                    {
                        AttributeName = DynamoDbTableConfiguration.CATEGORY_ATTRIBUTE,
                        AttributeType = ScalarAttributeType.N
                    },
                    new AttributeDefinition
                    {
                        AttributeName = DynamoDbTableConfiguration.NAME_ATTRIBUTE,
                        AttributeType = ScalarAttributeType.S
                    }
                },
                GlobalSecondaryIndexUpdates = new List<GlobalSecondaryIndexUpdate>
                {
                    new GlobalSecondaryIndexUpdate
                    {
                        Create = createIndexAction
                    }
                }
            };

            await client.UpdateTableAsync(updateRequest);
            Console.WriteLine($"GSI {requiredIndexName} criado com sucesso!");
            
            // Aguardar índice ficar ativo
            await WaitForIndexToBeActiveAsync(client, tableName, requiredIndexName);
        }
        else
        {
            Console.WriteLine($"GSI {requiredIndexName} já existe na tabela {tableName}.");
        }
    }

    static async Task CreateOrderTableAsync(IAmazonDynamoDB client)
    {
        var tableName = DynamoDbTableConfiguration.FASTFOOD_ORDERS_TABLE;

        // Verificar se a tabela já existe
        TableDescription? existingTable = null;
        try
        {
            var describeRequest = new DescribeTableRequest { TableName = tableName };
            var response = await client.DescribeTableAsync(describeRequest);
            existingTable = response.Table;
            Console.WriteLine($"Tabela {tableName} já existe.");
        }
        catch (ResourceNotFoundException)
        {
            // Tabela não existe, continuar com a criação
        }

        // Se tabela existe, verificar e criar GSIs faltantes
        if (existingTable != null)
        {
            await EnsureOrderTableIndexesAsync(client, tableName, existingTable);
            return;
        }

        Console.WriteLine($"Criando tabela {tableName}...");

        var request = new CreateTableRequest
        {
            TableName = tableName,
            AttributeDefinitions = new List<AttributeDefinition>
            {
                new AttributeDefinition
                {
                    AttributeName = DynamoDbTableConfiguration.ORDER_ID_ATTRIBUTE,
                    AttributeType = ScalarAttributeType.S
                },
                new AttributeDefinition
                {
                    AttributeName = DynamoDbTableConfiguration.CUSTOMER_ID_ATTRIBUTE,
                    AttributeType = ScalarAttributeType.S
                },
                new AttributeDefinition
                {
                    AttributeName = DynamoDbTableConfiguration.CREATED_AT_ATTRIBUTE,
                    AttributeType = ScalarAttributeType.S
                },
                new AttributeDefinition
                {
                    AttributeName = DynamoDbTableConfiguration.ORDER_STATUS_ATTRIBUTE,
                    AttributeType = ScalarAttributeType.N
                },
                new AttributeDefinition
                {
                    AttributeName = DynamoDbTableConfiguration.CODE_ATTRIBUTE,
                    AttributeType = ScalarAttributeType.S
                }
            },
            KeySchema = new List<KeySchemaElement>
            {
                new KeySchemaElement
                {
                    AttributeName = DynamoDbTableConfiguration.ORDER_ID_ATTRIBUTE,
                    KeyType = KeyType.HASH
                }
            },
            GlobalSecondaryIndexes = new List<GlobalSecondaryIndex>
            {
                // GSI1: CustomerId-CreatedAt-Index
                new GlobalSecondaryIndex
                {
                    IndexName = DynamoDbTableConfiguration.ORDER_CUSTOMER_ID_INDEX,
                    KeySchema = new List<KeySchemaElement>
                    {
                        new KeySchemaElement
                        {
                            AttributeName = DynamoDbTableConfiguration.CUSTOMER_ID_ATTRIBUTE,
                            KeyType = KeyType.HASH
                        },
                        new KeySchemaElement
                        {
                            AttributeName = DynamoDbTableConfiguration.CREATED_AT_ATTRIBUTE,
                            KeyType = KeyType.RANGE
                        }
                    },
                    Projection = new Projection { ProjectionType = ProjectionType.ALL },
                    ProvisionedThroughput = new ProvisionedThroughput
                    {
                        ReadCapacityUnits = 5,
                        WriteCapacityUnits = 5
                    }
                },
                // GSI2: Status-CreatedAt-Index
                new GlobalSecondaryIndex
                {
                    IndexName = DynamoDbTableConfiguration.ORDER_STATUS_INDEX,
                    KeySchema = new List<KeySchemaElement>
                    {
                        new KeySchemaElement
                        {
                            AttributeName = DynamoDbTableConfiguration.ORDER_STATUS_ATTRIBUTE,
                            KeyType = KeyType.HASH
                        },
                        new KeySchemaElement
                        {
                            AttributeName = DynamoDbTableConfiguration.CREATED_AT_ATTRIBUTE,
                            KeyType = KeyType.RANGE
                        }
                    },
                    Projection = new Projection { ProjectionType = ProjectionType.ALL },
                    ProvisionedThroughput = new ProvisionedThroughput
                    {
                        ReadCapacityUnits = 5,
                        WriteCapacityUnits = 5
                    }
                },
                // GSI3: Code-Index
                new GlobalSecondaryIndex
                {
                    IndexName = DynamoDbTableConfiguration.ORDER_CODE_INDEX,
                    KeySchema = new List<KeySchemaElement>
                    {
                        new KeySchemaElement
                        {
                            AttributeName = DynamoDbTableConfiguration.CODE_ATTRIBUTE,
                            KeyType = KeyType.HASH
                        },
                        new KeySchemaElement
                        {
                            AttributeName = DynamoDbTableConfiguration.ORDER_ID_ATTRIBUTE,
                            KeyType = KeyType.RANGE
                        }
                    },
                    Projection = new Projection { ProjectionType = ProjectionType.ALL }
                }
            },
            BillingMode = BillingMode.PAY_PER_REQUEST
        };

        await client.CreateTableAsync(request);
        Console.WriteLine($"Tabela {tableName} criada com sucesso!");

        // Aguardar tabela ficar ativa
        await WaitForTableToBeActiveAsync(client, tableName);
    }

    static async Task EnsureOrderTableIndexesAsync(IAmazonDynamoDB client, string tableName, TableDescription existingTable)
    {
        var existingIndexNames = existingTable.GlobalSecondaryIndexes?.Select(gsi => gsi.IndexName).ToList() ?? new List<string>();
        var requiredIndexes = new[]
        {
            DynamoDbTableConfiguration.ORDER_CUSTOMER_ID_INDEX,
            DynamoDbTableConfiguration.ORDER_STATUS_INDEX,
            DynamoDbTableConfiguration.ORDER_CODE_INDEX
        };

        var indexesToCreate = requiredIndexes.Where(idx => !existingIndexNames.Contains(idx)).ToList();

        if (!indexesToCreate.Any())
        {
            Console.WriteLine($"Todos os GSIs já existem na tabela {tableName}.");
            return;
        }

        foreach (var indexName in indexesToCreate)
        {
            Console.WriteLine($"Criando GSI {indexName} na tabela {tableName}...");
            
            var updateRequest = new UpdateTableRequest
            {
                TableName = tableName
            };

            // Definir atributos e schema do índice
            var attributeDefinitions = new List<AttributeDefinition>();
            var keySchema = new List<KeySchemaElement>();

            if (indexName == DynamoDbTableConfiguration.ORDER_CUSTOMER_ID_INDEX)
            {
                attributeDefinitions.Add(new AttributeDefinition { AttributeName = DynamoDbTableConfiguration.CUSTOMER_ID_ATTRIBUTE, AttributeType = ScalarAttributeType.S });
                attributeDefinitions.Add(new AttributeDefinition { AttributeName = DynamoDbTableConfiguration.CREATED_AT_ATTRIBUTE, AttributeType = ScalarAttributeType.S });
                keySchema.Add(new KeySchemaElement { AttributeName = DynamoDbTableConfiguration.CUSTOMER_ID_ATTRIBUTE, KeyType = KeyType.HASH });
                keySchema.Add(new KeySchemaElement { AttributeName = DynamoDbTableConfiguration.CREATED_AT_ATTRIBUTE, KeyType = KeyType.RANGE });
            }
            else if (indexName == DynamoDbTableConfiguration.ORDER_STATUS_INDEX)
            {
                attributeDefinitions.Add(new AttributeDefinition { AttributeName = DynamoDbTableConfiguration.ORDER_STATUS_ATTRIBUTE, AttributeType = ScalarAttributeType.N });
                attributeDefinitions.Add(new AttributeDefinition { AttributeName = DynamoDbTableConfiguration.CREATED_AT_ATTRIBUTE, AttributeType = ScalarAttributeType.S });
                keySchema.Add(new KeySchemaElement { AttributeName = DynamoDbTableConfiguration.ORDER_STATUS_ATTRIBUTE, KeyType = KeyType.HASH });
                keySchema.Add(new KeySchemaElement { AttributeName = DynamoDbTableConfiguration.CREATED_AT_ATTRIBUTE, KeyType = KeyType.RANGE });
            }
            else if (indexName == DynamoDbTableConfiguration.ORDER_CODE_INDEX)
            {
                attributeDefinitions.Add(new AttributeDefinition { AttributeName = DynamoDbTableConfiguration.CODE_ATTRIBUTE, AttributeType = ScalarAttributeType.S });
                attributeDefinitions.Add(new AttributeDefinition { AttributeName = DynamoDbTableConfiguration.ORDER_ID_ATTRIBUTE, AttributeType = ScalarAttributeType.S });
                keySchema.Add(new KeySchemaElement { AttributeName = DynamoDbTableConfiguration.CODE_ATTRIBUTE, KeyType = KeyType.HASH });
                keySchema.Add(new KeySchemaElement { AttributeName = DynamoDbTableConfiguration.ORDER_ID_ATTRIBUTE, KeyType = KeyType.RANGE });
            }

            var isOnDemand = existingTable.BillingModeSummary?.BillingMode == BillingMode.PAY_PER_REQUEST;
            
            var createIndexAction = new CreateGlobalSecondaryIndexAction
            {
                IndexName = indexName,
                KeySchema = keySchema,
                Projection = new Projection { ProjectionType = ProjectionType.ALL }
            };

            // Só adiciona ProvisionedThroughput se não for PAY_PER_REQUEST
            if (!isOnDemand)
            {
                createIndexAction.ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = 5,
                    WriteCapacityUnits = 5
                };
            }

            updateRequest.AttributeDefinitions = attributeDefinitions;
            updateRequest.GlobalSecondaryIndexUpdates = new List<GlobalSecondaryIndexUpdate>
            {
                new GlobalSecondaryIndexUpdate
                {
                    Create = createIndexAction
                }
            };

            await client.UpdateTableAsync(updateRequest);
            Console.WriteLine($"GSI {indexName} criado com sucesso!");
            
            // Aguardar índice ficar ativo
            await WaitForIndexToBeActiveAsync(client, tableName, indexName);
        }
    }

    static async Task WaitForTableToBeActiveAsync(IAmazonDynamoDB client, string tableName)
    {
        Console.WriteLine($"Aguardando tabela {tableName} ficar ativa...");
        
        var request = new DescribeTableRequest { TableName = tableName };
        TableStatus status;

        do
        {
            await Task.Delay(2000);
            var response = await client.DescribeTableAsync(request);
            status = response.Table.TableStatus;
            Console.WriteLine($"Status da tabela {tableName}: {status}");
        } while (status != TableStatus.ACTIVE);

        Console.WriteLine($"Tabela {tableName} está ativa!");
    }

    static async Task WaitForIndexToBeActiveAsync(IAmazonDynamoDB client, string tableName, string indexName)
    {
        Console.WriteLine($"Aguardando GSI {indexName} ficar ativo...");
        
        var request = new DescribeTableRequest { TableName = tableName };
        IndexStatus status;

        do
        {
            await Task.Delay(2000);
            var response = await client.DescribeTableAsync(request);
            var index = response.Table.GlobalSecondaryIndexes?.FirstOrDefault(gsi => gsi.IndexName == indexName);
            status = index?.IndexStatus ?? IndexStatus.CREATING;
            Console.WriteLine($"Status do GSI {indexName}: {status}");
        } while (status != IndexStatus.ACTIVE);

        Console.WriteLine($"GSI {indexName} está ativo!");
    }

    /// <summary>
    /// Carrega produtos iniciais na tabela (idempotente - não insere se já existir)
    /// </summary>
    static async Task SeedInitialProductsAsync(IAmazonDynamoDB client)
    {
        Console.WriteLine("Iniciando carga inicial de produtos...");

        var tableName = DynamoDbTableConfiguration.FASTFOOD_PRODUCTS_TABLE;

        // Verificar se a tabela existe antes de tentar inserir
        try
        {
            await client.DescribeTableAsync(new DescribeTableRequest { TableName = tableName });
        }
        catch (ResourceNotFoundException)
        {
            Console.WriteLine($"Tabela {tableName} não existe. Pulando carga inicial.");
            return;
        }

        var products = GetInitialProducts();

        int inserted = 0;
        int skipped = 0;

        foreach (var product in products)
        {
            var productId = product[DynamoDbTableConfiguration.PRODUCT_ID_ATTRIBUTE].S;

            // Verificar se produto já existe
            var getRequest = new GetItemRequest
            {
                TableName = tableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { DynamoDbTableConfiguration.PRODUCT_ID_ATTRIBUTE, new AttributeValue { S = productId } }
                }
            };

            var getResponse = await client.GetItemAsync(getRequest);

            if (getResponse.Item.Any())
            {
                Console.WriteLine($"Produto {productId} já existe. Pulando inserção.");
                skipped++;
                continue;
            }

            // Inserir produto
            var putRequest = new PutItemRequest
            {
                TableName = tableName,
                Item = product
            };

            await client.PutItemAsync(putRequest);
            Console.WriteLine($"Produto {productId} inserido com sucesso.");
            inserted++;
        }

        Console.WriteLine($"Carga inicial concluída: {inserted} produtos inseridos, {skipped} produtos já existentes.");
    }

    /// <summary>
    /// Retorna lista de produtos iniciais para carga
    /// </summary>
    static List<Dictionary<string, AttributeValue>> GetInitialProducts()
    {
        var products = new List<Dictionary<string, AttributeValue>>();

        // Produto 1: Hambúrguer Artesanal
        products.Add(new Dictionary<string, AttributeValue>
        {
            { DynamoDbTableConfiguration.PRODUCT_ID_ATTRIBUTE, new AttributeValue { S = "de904cba-a00b-48f1-a362-6cf9d196ec5f" } },
            { "Name", new AttributeValue { S = "Hambúrguer Artesanal" } },
            { DynamoDbTableConfiguration.NAME_ATTRIBUTE, new AttributeValue { S = "Hambúrguer Artesanal" } },
            { "Description", new AttributeValue { S = "Hambúrguer artesanal com carne 180g e queijo cheddar" } },
            { DynamoDbTableConfiguration.CATEGORY_ATTRIBUTE, new AttributeValue { N = "1" } },
            { "Price", new AttributeValue { N = "24.0" } },
            { "ImageUrl", new AttributeValue { S = "https://images.pexels.com/photos/1639562/pexels-photo-1639562.jpeg" } },
            { "IsActive", new AttributeValue { BOOL = true } },
            { DynamoDbTableConfiguration.CREATED_AT_ATTRIBUTE, new AttributeValue { S = "2025-01-01T00:00:00Z" } },
            { "BaseIngredients", new AttributeValue
                {
                    L = new List<AttributeValue>
                    {
                        new AttributeValue { M = new Dictionary<string, AttributeValue>
                        {
                            { "Id", new AttributeValue { S = "e2d3724b-9cca-49e0-82e5-96109f3442db" } },
                            { "Name", new AttributeValue { S = "Pão de Brioche" } },
                            { "Price", new AttributeValue { N = "2.0" } }
                        }},
                        new AttributeValue { M = new Dictionary<string, AttributeValue>
                        {
                            { "Id", new AttributeValue { S = "a23ab374-d82d-4025-a116-25ef6cd25c63" } },
                            { "Name", new AttributeValue { S = "Carne 180g" } },
                            { "Price", new AttributeValue { N = "15.0" } }
                        }},
                        new AttributeValue { M = new Dictionary<string, AttributeValue>
                        {
                            { "Id", new AttributeValue { S = "1a4409e0-3307-4419-92d5-485e3e302493" } },
                            { "Name", new AttributeValue { S = "Queijo Cheddar" } },
                            { "Price", new AttributeValue { N = "3.0" } }
                        }},
                        new AttributeValue { M = new Dictionary<string, AttributeValue>
                        {
                            { "Id", new AttributeValue { S = "fbafa1c2-62b3-40c4-9084-2509af6a263a" } },
                            { "Name", new AttributeValue { S = "Alface" } },
                            { "Price", new AttributeValue { N = "1.0" } }
                        }},
                        new AttributeValue { M = new Dictionary<string, AttributeValue>
                        {
                            { "Id", new AttributeValue { S = "924aefd6-0d50-4018-8299-52d23760467f" } },
                            { "Name", new AttributeValue { S = "Tomate" } },
                            { "Price", new AttributeValue { N = "1.0" } }
                        }}
                    }
                }
            }
        });

        // Produto 2: Pizza Margherita
        products.Add(new Dictionary<string, AttributeValue>
        {
            { DynamoDbTableConfiguration.PRODUCT_ID_ATTRIBUTE, new AttributeValue { S = "758621ad-3807-4a84-af7d-ce8c64245c48" } },
            { "Name", new AttributeValue { S = "Pizza Margherita" } },
            { DynamoDbTableConfiguration.NAME_ATTRIBUTE, new AttributeValue { S = "Pizza Margherita" } },
            { "Description", new AttributeValue { S = "Pizza tradicional italiana com mussarela e manjericão" } },
            { DynamoDbTableConfiguration.CATEGORY_ATTRIBUTE, new AttributeValue { N = "1" } },
            { "Price", new AttributeValue { N = "30.0" } },
            { "ImageUrl", new AttributeValue { S = "https://images.pexels.com/photos/10068752/pexels-photo-10068752.jpeg" } },
            { "IsActive", new AttributeValue { BOOL = true } },
            { DynamoDbTableConfiguration.CREATED_AT_ATTRIBUTE, new AttributeValue { S = "2025-01-01T00:00:00Z" } },
            { "BaseIngredients", new AttributeValue
                {
                    L = new List<AttributeValue>
                    {
                        new AttributeValue { M = new Dictionary<string, AttributeValue>
                        {
                            { "Id", new AttributeValue { S = "e05445ae-0ea3-4bb3-a0c7-bf6ee7b40538" } },
                            { "Name", new AttributeValue { S = "Massa" } },
                            { "Price", new AttributeValue { N = "5.0" } }
                        }},
                        new AttributeValue { M = new Dictionary<string, AttributeValue>
                        {
                            { "Id", new AttributeValue { S = "276bfc29-7c0a-4ed1-8e08-df579f4427cc" } },
                            { "Name", new AttributeValue { S = "Molho" } },
                            { "Price", new AttributeValue { N = "3.0" } }
                        }},
                        new AttributeValue { M = new Dictionary<string, AttributeValue>
                        {
                            { "Id", new AttributeValue { S = "25826a5e-8be5-47ff-8548-95a2d1cba3de" } },
                            { "Name", new AttributeValue { S = "Mussarela" } },
                            { "Price", new AttributeValue { N = "8.0" } }
                        }},
                        new AttributeValue { M = new Dictionary<string, AttributeValue>
                        {
                            { "Id", new AttributeValue { S = "6222dc26-45ff-47de-b3e7-9a99e933c3e1" } },
                            { "Name", new AttributeValue { S = "Manjericão" } },
                            { "Price", new AttributeValue { N = "1.0" } }
                        }}
                    }
                }
            }
        });

        // Produto 3: Refrigerante Cola 350ml
        products.Add(new Dictionary<string, AttributeValue>
        {
            { DynamoDbTableConfiguration.PRODUCT_ID_ATTRIBUTE, new AttributeValue { S = "9a8a232b-d238-429a-9d89-820373ad5f50" } },
            { "Name", new AttributeValue { S = "Refrigerante Cola 350ml" } },
            { DynamoDbTableConfiguration.NAME_ATTRIBUTE, new AttributeValue { S = "Refrigerante Cola 350ml" } },
            { "Description", new AttributeValue { S = "Refrigerante cola tradicional 350ml" } },
            { DynamoDbTableConfiguration.CATEGORY_ATTRIBUTE, new AttributeValue { N = "2" } },
            { "Price", new AttributeValue { N = "6.0" } },
            { "ImageUrl", new AttributeValue { S = "https://images.pexels.com/photos/2983100/pexels-photo-2983100.jpeg" } },
            { "IsActive", new AttributeValue { BOOL = true } },
            { DynamoDbTableConfiguration.CREATED_AT_ATTRIBUTE, new AttributeValue { S = "2025-01-01T00:00:00Z" } }
        });

        // Produto 4: Batata Frita
        products.Add(new Dictionary<string, AttributeValue>
        {
            { DynamoDbTableConfiguration.PRODUCT_ID_ATTRIBUTE, new AttributeValue { S = "3667e0c8-151a-4dc2-9dd7-a42dde596f34" } },
            { "Name", new AttributeValue { S = "Batata Frita" } },
            { DynamoDbTableConfiguration.NAME_ATTRIBUTE, new AttributeValue { S = "Batata Frita" } },
            { "Description", new AttributeValue { S = "Batata frita crocante porção média" } },
            { DynamoDbTableConfiguration.CATEGORY_ATTRIBUTE, new AttributeValue { N = "3" } },
            { "Price", new AttributeValue { N = "12.0" } },
            { "ImageUrl", new AttributeValue { S = "https://images.pexels.com/photos/1586942/pexels-photo-1586942.jpeg" } },
            { "IsActive", new AttributeValue { BOOL = true } },
            { DynamoDbTableConfiguration.CREATED_AT_ATTRIBUTE, new AttributeValue { S = "2025-01-01T00:00:00Z" } }
        });

        // Produto 5: Brownie de Chocolate
        products.Add(new Dictionary<string, AttributeValue>
        {
            { DynamoDbTableConfiguration.PRODUCT_ID_ATTRIBUTE, new AttributeValue { S = "60d17e4d-2496-410a-8598-a7d11579dceb" } },
            { "Name", new AttributeValue { S = "Brownie de Chocolate" } },
            { DynamoDbTableConfiguration.NAME_ATTRIBUTE, new AttributeValue { S = "Brownie de Chocolate" } },
            { "Description", new AttributeValue { S = "Brownie caseiro de chocolate" } },
            { DynamoDbTableConfiguration.CATEGORY_ATTRIBUTE, new AttributeValue { N = "4" } },
            { "Price", new AttributeValue { N = "9.0" } },
            { "ImageUrl", new AttributeValue { S = "https://images.pexels.com/photos/2067396/pexels-photo-2067396.jpeg" } },
            { "IsActive", new AttributeValue { BOOL = true } },
            { DynamoDbTableConfiguration.CREATED_AT_ATTRIBUTE, new AttributeValue { S = "2025-01-01T00:00:00Z" } }
        });

        return products;
    }
}
