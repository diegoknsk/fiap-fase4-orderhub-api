using Amazon.DynamoDBv2.Model;

namespace FastFood.OrderHub.Infra.Persistence.Extensions;

/// <summary>
/// Extensões úteis para operações DynamoDB
/// </summary>
public static class DynamoDbExtensions
{
    /// <summary>
    /// Verifica se um item existe no resultado do DynamoDB
    /// </summary>
    public static bool HasItem(this GetItemResponse response)
    {
        return response.Item != null && response.Item.Any();
    }

    /// <summary>
    /// Obtém valor string de um atributo DynamoDB de forma segura
    /// </summary>
    public static string? GetStringValue(this Dictionary<string, AttributeValue> item, string key)
    {
        return item.ContainsKey(key) ? item[key].S : null;
    }

    /// <summary>
    /// Obtém valor numérico de um atributo DynamoDB de forma segura
    /// </summary>
    public static decimal? GetDecimalValue(this Dictionary<string, AttributeValue> item, string key)
    {
        if (!item.ContainsKey(key) || string.IsNullOrEmpty(item[key].N))
            return null;

        return decimal.TryParse(item[key].N, out var value) ? value : null;
    }

    /// <summary>
    /// Obtém valor inteiro de um atributo DynamoDB de forma segura
    /// </summary>
    public static int? GetIntValue(this Dictionary<string, AttributeValue> item, string key)
    {
        if (!item.ContainsKey(key) || string.IsNullOrEmpty(item[key].N))
            return null;

        return int.TryParse(item[key].N, out var value) ? value : null;
    }

    /// <summary>
    /// Obtém valor Guid de um atributo DynamoDB de forma segura
    /// </summary>
    public static Guid? GetGuidValue(this Dictionary<string, AttributeValue> item, string key)
    {
        var stringValue = GetStringValue(item, key);
        if (string.IsNullOrWhiteSpace(stringValue))
            return null;

        return Guid.TryParse(stringValue, out var value) ? value : null;
    }

    /// <summary>
    /// Obtém valor DateTime de um atributo DynamoDB de forma segura
    /// </summary>
    public static DateTime? GetDateTimeValue(this Dictionary<string, AttributeValue> item, string key)
    {
        var stringValue = GetStringValue(item, key);
        if (string.IsNullOrWhiteSpace(stringValue))
            return null;

        return DateTime.TryParse(stringValue, out var value) ? value : null;
    }

    /// <summary>
    /// Obtém valor booleano de um atributo DynamoDB de forma segura
    /// </summary>
    public static bool? GetBoolValue(this Dictionary<string, AttributeValue> item, string key)
    {
        if (!item.ContainsKey(key))
            return null;

        return item[key].BOOL;
    }
}



