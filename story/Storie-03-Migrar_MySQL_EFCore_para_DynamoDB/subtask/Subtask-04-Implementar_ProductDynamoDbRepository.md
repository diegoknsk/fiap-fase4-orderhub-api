# Subtask 04: Implementar ProductDynamoDbRepository

## Status
- **Estado:** ✅ Concluído
- **Data de Conclusão:** 28/12/2024

## Descrição
Implementar repositório DynamoDB para Product que faz o mapeamento entre entidades de domínio/DTOs e atributos DynamoDB. Este repositório será usado pelo ProductDynamoDbDataSource.

## Passos de implementação
- [x] Criar arquivo `src/Infra/FastFood.OrderHub.Infra.Persistence/Repositories/ProductDynamoDbRepository.cs`
- [x] Implementar interface ou classe com métodos:
  - [x] `GetByIdAsync(Guid id)` → GetItem DynamoDB
  - [x] `GetAllAsync()` → Scan (ou Query se usar GSI)
  - [x] `GetByCategoryAsync(int category)` → Query no GSI Category-Index
  - [x] `GetPagedAsync(int page, int pageSize, int? category, string? name)` → Paginação com filtros
  - [x] `AddAsync(ProductDto dto)` → PutItem
  - [x] `UpdateAsync(ProductDto dto)` → PutItem
  - [x] `DeleteAsync(Guid id)` → UpdateItem com IsActive=false (soft delete)
- [x] Implementar métodos auxiliares de mapeamento:
  - [x] `MapToDynamoDb(ProductDto)` → Dictionary<string, AttributeValue>
  - [x] `MapFromDynamoDb(Dictionary<string, AttributeValue>)` → ProductDto
- [x] Tratar conversão de tipos:
  - [x] Guid → String (formato: `{Guid}`)
  - [x] Enum → Number
  - [x] List<BaseIngredient> → List<Map> (AttributeValue)
  - [x] DateTime → String (ISO 8601)
  - [x] Decimal → String com CultureInfo.InvariantCulture (fix vírgula decimal)
- [x] Implementar tratamento de erros (ItemNotFoundException, etc.)

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
- [x] Arquivo `ProductDynamoDbRepository.cs` criado
- [x] Métodos CRUD implementados
- [x] Mapeamento DTO ↔ DynamoDB funcionando
- [x] Tratamento de erros implementado
- [x] Código compila sem erros
- [x] Paginação implementada com filtros (categoria e nome)
- [x] Fix formatação numérica (CultureInfo.InvariantCulture)

## Observações
- Usar `IAmazonDynamoDB` injetado via construtor
- BaseIngredients armazenados como lista dentro do item (denormalização)
- Considerar soft delete (IsActive=false) ao invés de DeleteItem físico

