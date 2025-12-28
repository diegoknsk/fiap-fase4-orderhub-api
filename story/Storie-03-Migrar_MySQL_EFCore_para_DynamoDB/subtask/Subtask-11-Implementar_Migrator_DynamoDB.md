# Subtask 11: Implementar Migrator para Criar Tabelas DynamoDB

## Status
- **Estado:** 📋 Pendente
- **Data de Conclusão:** [DD/MM/AAAA]

## Descrição
Implementar projeto Migrator que cria as tabelas DynamoDB (`fastfood-products` e `fastfood-orders`) com suas respectivas GSIs. O Migrator deve ser executável como console app e pode ser usado como K8s Job.

## Passos de implementação
- [ ] Criar arquivo `src/InterfacesExternas/FastFood.OrderHub.Migrator/Program.cs`
- [ ] Configurar `IAmazonDynamoDB` usando `DynamoDbConfiguration`
- [ ] Implementar método `CreateProductTableAsync(IAmazonDynamoDB client)`
  - Definir `CreateTableRequest` para `fastfood-products`
  - PK: `ProductId` (String)
  - GSI1: `Category-Index` (PK: Category, SK: Name)
  - ProvisionedThroughput: RCU=5, WCU=5 (ou OnDemand)
- [ ] Implementar método `CreateOrderTableAsync(IAmazonDynamoDB client)`
  - Definir `CreateTableRequest` para `fastfood-orders`
  - PK: `OrderId` (String)
  - GSI1: `CustomerId-CreatedAt-Index` (PK: CustomerId, SK: CreatedAt)
  - GSI2: `Status-CreatedAt-Index` (PK: OrderStatus, SK: CreatedAt) - Opcional
  - GSI3: `Code-Index` (PK: Code, SK: OrderId) - Opcional
  - ProvisionedThroughput: RCU=5, WCU=5 (ou OnDemand)
- [ ] Implementar verificação de existência de tabelas (evitar erro se já existir)
- [ ] Implementar tratamento de erros (ResourceInUseException, etc.)
- [ ] Adicionar logs informativos (criação de tabelas, GSIs)
- [ ] Configurar `appsettings.json` com credenciais DynamoDB

## Estrutura do Migrator

```csharp
class Program
{
    static async Task Main(string[] args)
    {
        // Configurar DynamoDB client
        var client = CreateDynamoDbClient();
        
        // Criar tabelas
        await CreateProductTableAsync(client);
        await CreateOrderTableAsync(client);
        
        Console.WriteLine("Migração concluída com sucesso!");
    }
}
```

## Como testar
- Executar `dotnet run --project src/InterfacesExternas/FastFood.OrderHub.Migrator`
- Verificar que tabelas são criadas no DynamoDB
- Verificar que GSIs são criados corretamente
- Testar execução múltipla (não deve dar erro se tabelas já existem)
- Validar estrutura das tabelas no AWS Console

## Critérios de aceite
- [ ] Arquivo `Program.cs` do Migrator implementado
- [ ] Método `CreateProductTableAsync` criado
- [ ] Método `CreateOrderTableAsync` criado
- [ ] GSIs criados corretamente
- [ ] Verificação de existência de tabelas implementada
- [ ] Tratamento de erros adequado
- [ ] Logs informativos adicionados
- [ ] Configurações em `appsettings.json`
- [ ] Migrator executa sem erros
- [ ] Tabelas criadas no DynamoDB com estrutura correta

## Observações
- ProvisionedThroughput pode ser ajustado conforme necessidade
- Para desenvolvimento, pode usar OnDemand (mais simples)
- Migrator pode ser executado como K8s Job antes do deploy da API
- Considerar adicionar opção de deletar tabelas (para desenvolvimento)

