# Subtask 04: Implementar ProductDynamoDbRepository

## Status
- **Estado:** 📋 Pendente
- **Data de Conclusão:** [DD/MM/AAAA]

## Descrição
Implementar repositório DynamoDB para Product que faz o mapeamento entre entidades de domínio/DTOs e atributos DynamoDB. Este repositório será usado pelo ProductDynamoDbDataSource.

## Passos de implementação
- [ ] Criar arquivo `src/Infra/FastFood.OrderHub.Infra.Persistence/Repositories/ProductDynamoDbRepository.cs`
- [ ] Implementar interface ou classe com métodos:
  - `GetByIdAsync(Guid id)` → GetItem DynamoDB
  - `GetAllAsync()` → Scan (ou Query se usar GSI)
  - `GetByCategoryAsync(int category)` → Query no GSI Category-Index
  - `AddAsync(ProductDto dto)` → PutItem
  - `UpdateAsync(ProductDto dto)` → UpdateItem
  - `DeleteAsync(Guid id)` → DeleteItem (ou UpdateItem com IsActive=false para soft delete)
- [ ] Implementar métodos auxiliares de mapeamento:
  - `MapToDynamoDb(ProductDto)` → Dictionary<string, AttributeValue>
  - `MapFromDynamoDb(Dictionary<string, AttributeValue>)` → ProductDto
- [ ] Tratar conversão de tipos:
  - Guid → String (formato: `PROD#{Guid}` ou apenas `{Guid}`)
  - Enum → Number
  - List<BaseIngredient> → List<Map> (AttributeValue)
  - DateTime → String (ISO 8601)
- [ ] Implementar tratamento de erros (ItemNotFoundException, etc.)

## Estrutura DynamoDB

### Item Product
```json
{
  "ProductId": "PROD#guid",
  "Name": "string",
  "Description": "string",
  "Category": 1,
  "Price": 10.50,
  "ImageUrl": "string",
  "IsActive": true,
  "CreatedAt": "2025-01-01T00:00:00Z",
  "BaseIngredients": [
    {
      "Id": "guid",
      "Name": "string",
      "Price": 1.00
    }
  ]
}
```

## Como testar
- Criar testes unitários mockando `IAmazonDynamoDB`
- Testar mapeamento de DTO → DynamoDB → DTO (round-trip)
- Validar tratamento de erros (ItemNotFoundException)

## Critérios de aceite
- [ ] Arquivo `ProductDynamoDbRepository.cs` criado
- [ ] Métodos CRUD implementados
- [ ] Mapeamento DTO ↔ DynamoDB funcionando
- [ ] Tratamento de erros implementado
- [ ] Código compila sem erros
- [ ] Testes unitários criados (mock de IAmazonDynamoDB)

## Observações
- Usar `IAmazonDynamoDB` injetado via construtor
- BaseIngredients armazenados como lista dentro do item (denormalização)
- Considerar soft delete (IsActive=false) ao invés de DeleteItem físico

