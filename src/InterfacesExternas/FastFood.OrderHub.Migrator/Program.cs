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
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        var dynamoDbConfig = configuration.GetSection("DynamoDb").Get<DynamoDbConfiguration>() 
            ?? new DynamoDbConfiguration
            {
                AccessKey = configuration["DynamoDb:AccessKey"] ?? Environment.GetEnvironmentVariable("DYNAMODB__ACCESSKEY") ?? string.Empty,
                SecretKey = configuration["DynamoDb:SecretKey"] ?? Environment.GetEnvironmentVariable("DYNAMODB__SECRETKEY") ?? string.Empty,
                SessionToken = configuration["DynamoDb:SessionToken"] ?? Environment.GetEnvironmentVariable("DYNAMODB__SESSIONTOKEN"),
                Region = configuration["DynamoDb:Region"] ?? Environment.GetEnvironmentVariable("DYNAMODB__REGION") ?? "us-east-1",
                ServiceUrl = configuration["DynamoDb:ServiceUrl"] ?? Environment.GetEnvironmentVariable("DYNAMODB__SERVICEURL")
            };

        var client = dynamoDbConfig.CreateDynamoDbClient();

        try
        {
            // Criar tabelas
            await CreateProductTableAsync(client);
            await CreateOrderTableAsync(client);

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
}
