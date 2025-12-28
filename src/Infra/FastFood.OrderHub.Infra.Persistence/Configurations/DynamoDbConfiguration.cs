using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;

namespace FastFood.OrderHub.Infra.Persistence.Configurations;

/// <summary>
/// Configuração para acesso ao DynamoDB
/// </summary>
public class DynamoDbConfiguration
{
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string? SessionToken { get; set; }
    public string Region { get; set; } = "us-east-1";
    public string? ServiceUrl { get; set; }

    /// <summary>
    /// Cria cliente DynamoDB configurado com credenciais e região
    /// Suporta Session Token para AWS Academy e credenciais temporárias
    /// </summary>
    public IAmazonDynamoDB CreateDynamoDbClient()
    {
        AWSCredentials credentials;

        // Se SessionToken for fornecido (AWS Academy), usar SessionAWSCredentials
        if (!string.IsNullOrWhiteSpace(SessionToken))
        {
            credentials = new SessionAWSCredentials(AccessKey, SecretKey, SessionToken);
        }
        else
        {
            credentials = new BasicAWSCredentials(AccessKey, SecretKey);
        }

        var region = RegionEndpoint.GetBySystemName(Region);

        var config = new AmazonDynamoDBConfig
        {
            RegionEndpoint = region
        };

        // Se ServiceUrl for fornecido (ex: LocalStack), usar para desenvolvimento local
        if (!string.IsNullOrWhiteSpace(ServiceUrl))
        {
            config.ServiceURL = ServiceUrl;
        }

        return new AmazonDynamoDBClient(credentials, config);
    }
}

