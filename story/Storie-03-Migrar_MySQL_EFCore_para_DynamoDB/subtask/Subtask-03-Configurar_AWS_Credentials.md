# Subtask 03: Configurar AWS Credentials e Connection

## Status
- **Estado:** 📋 Pendente
- **Data de Conclusão:** [DD/MM/AAAA]

## Descrição
Configurar acesso ao DynamoDB usando Access Key e Secret Key via variáveis de ambiente, seguindo o padrão do projeto (não usar IAM roles). Configurar cliente DynamoDB no Program.cs da API.

## Passos de implementação
- [ ] Criar classe `DynamoDbConfiguration` em `src/Infra/FastFood.OrderHub.Infra.Persistence/Configurations/DynamoDbConfiguration.cs`
- [ ] Definir propriedades:
  - `AccessKey` (string)
  - `SecretKey` (string)
  - `Region` (string, ex: "us-east-1")
  - `ServiceUrl` (string, opcional, para LocalStack em desenvolvimento)
- [ ] Criar método `CreateDynamoDbClient()` que retorna `IAmazonDynamoDB`
- [ ] Configurar credenciais via `BasicAWSCredentials` (Access Key + Secret Key)
- [ ] Configurar região via `AmazonDynamoDBConfig`
- [ ] Adicionar configuração em `appsettings.json`:
  ```json
  {
    "DynamoDb": {
      "AccessKey": "",
      "SecretKey": "",
      "Region": "us-east-1",
      "ServiceUrl": "" // Opcional para LocalStack
    }
  }
  ```
- [ ] Registrar `IAmazonDynamoDB` no `Program.cs` da API
- [ ] Documentar variáveis de ambiente necessárias:
  - `DYNAMODB__ACCESSKEY`
  - `DYNAMODB__SECRETKEY`
  - `DYNAMODB__REGION`
  - `DYNAMODB__SERVICEURL` (opcional)

## Como testar
- Verificar que `IAmazonDynamoDB` está registrado no DI container
- Testar criação do cliente (sem fazer queries reais ainda)
- Validar que configurações são lidas de appsettings.json e variáveis de ambiente

## Critérios de aceite
- [ ] Classe `DynamoDbConfiguration` criada
- [ ] Método `CreateDynamoDbClient()` implementado
- [ ] Configurações em `appsettings.json`
- [ ] `IAmazonDynamoDB` registrado no DI container
- [ ] Suporte a variáveis de ambiente documentado
- [ ] Código compila sem erros

## Observações
- **IMPORTANTE:** Sempre usar Access Key e Secret Key (não IAM roles)
- Credenciais sensíveis devem estar em variáveis de ambiente (não commitar no código)
- Para desenvolvimento local, pode usar LocalStack com ServiceUrl

