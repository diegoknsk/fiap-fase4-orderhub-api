namespace FastFood.OrderHub.Infra.Persistence.Configurations;

/// <summary>
/// Configurações das tabelas DynamoDB
/// </summary>
public static class DynamoDbTableConfiguration
{
    // Nomes das tabelas
    public const string FASTFOOD_PRODUCTS_TABLE = "fastfood-products";
    public const string FASTFOOD_ORDERS_TABLE = "fastfood-orders";

    // Nomes dos índices
    public const string PRODUCT_CATEGORY_INDEX = "Category-Index";
    public const string ORDER_CUSTOMER_ID_INDEX = "CustomerId-CreatedAt-Index";
    public const string ORDER_STATUS_INDEX = "Status-CreatedAt-Index";
    public const string ORDER_CODE_INDEX = "Code-Index";

    // Nomes de atributos (Partition Keys, Sort Keys) - snake_case conforme Terraform
    public const string PRODUCT_ID_ATTRIBUTE = "product_id";
    public const string ORDER_ID_ATTRIBUTE = "order_id";
    public const string CATEGORY_ATTRIBUTE = "category";
    public const string NAME_ATTRIBUTE = "name";
    public const string CUSTOMER_ID_ATTRIBUTE = "customer_id";
    public const string CREATED_AT_ATTRIBUTE = "created_at";
    public const string ORDER_STATUS_ATTRIBUTE = "order_status";
    public const string CODE_ATTRIBUTE = "code";
}

/// <summary>
/// Configuração da tabela de produtos
/// </summary>
public class ProductTableConfiguration
{
    /// <summary>
    /// Partition Key: ProductId (String)
    /// </summary>
    public string PartitionKey => DynamoDbTableConfiguration.PRODUCT_ID_ATTRIBUTE;

    /// <summary>
    /// GSI1: Category-Index
    /// - Partition Key: Category (Number)
    /// - Sort Key: Name (String)
    /// </summary>
    public string CategoryIndexName => DynamoDbTableConfiguration.PRODUCT_CATEGORY_INDEX;
}

/// <summary>
/// Configuração da tabela de pedidos
/// </summary>
public class OrderTableConfiguration
{
    /// <summary>
    /// Partition Key: OrderId (String)
    /// </summary>
    public string PartitionKey => DynamoDbTableConfiguration.ORDER_ID_ATTRIBUTE;

    /// <summary>
    /// GSI1: CustomerId-CreatedAt-Index
    /// - Partition Key: CustomerId (String)
    /// - Sort Key: CreatedAt (String, ISO 8601)
    /// </summary>
    public string CustomerIdIndexName => DynamoDbTableConfiguration.ORDER_CUSTOMER_ID_INDEX;

    /// <summary>
    /// GSI2: Status-CreatedAt-Index
    /// - Partition Key: OrderStatus (Number)
    /// - Sort Key: CreatedAt (String, ISO 8601)
    /// </summary>
    public string StatusIndexName => DynamoDbTableConfiguration.ORDER_STATUS_INDEX;

    /// <summary>
    /// GSI3: Code-Index
    /// - Partition Key: Code (String)
    /// - Sort Key: OrderId (String)
    /// </summary>
    public string CodeIndexName => DynamoDbTableConfiguration.ORDER_CODE_INDEX;
}

