# Subtask 07: Implementar OrderDynamoDbRepository

## Status
- **Estado:** 📋 Pendente
- **Data de Conclusão:** [DD/MM/AAAA]

## Descrição
Implementar repositório DynamoDB para Order que faz o mapeamento entre entidades de domínio/DTOs e atributos DynamoDB. Este repositório será usado pelo OrderDynamoDbDataSource. **IMPORTANTE:** Items (OrderedProducts) são armazenados como lista dentro do item Order (denormalização).

## Passos de implementação
- [ ] Criar arquivo `src/Infra/FastFood.OrderHub.Infra.Persistence/Repositories/OrderDynamoDbRepository.cs`
- [ ] Implementar métodos:
  - `GetByIdAsync(Guid id)` → GetItem DynamoDB (retorna Order completo com Items)
  - `GetByCustomerIdAsync(Guid customerId)` → Query no GSI CustomerId-CreatedAt-Index
  - `GetPagedAsync(int page, int pageSize, int? status)` → Query/Scan com paginação
  - `GetByStatusAsync(int status)` → Query no GSI Status-CreatedAt-Index
  - `GetByStatusWithoutPreparationAsync(int status)` → Query com filtro adicional
  - `AddAsync(OrderDto dto)` → PutItem
  - `UpdateAsync(OrderDto dto)` → UpdateItem (atualiza Order e Items)
  - `DeleteAsync(Guid id)` → DeleteItem
  - `ExistsAsync(Guid id)` → GetItem (apenas verificar existência)
  - `ExistsByCodeAsync(string code)` → Query no GSI Code-Index
  - `GenerateOrderCodeAsync()` → Lógica de geração (mesma do UseCase atual)
- [ ] Implementar métodos auxiliares de mapeamento:
  - `MapToDynamoDb(OrderDto)` → Dictionary<string, AttributeValue>
  - `MapFromDynamoDb(Dictionary<string, AttributeValue>)` → OrderDto
- [ ] Tratar conversão de Items (OrderedProducts):
  - List<OrderedProductDto> → List<Map> (AttributeValue)
  - Incluir snapshot completo: ProductId, ProductName, Category, Quantity, FinalPrice, Observation, CustomIngredients[]
- [ ] Implementar paginação usando `ExclusiveStartKey` (DynamoDB)
- [ ] Tratar limite de 400KB por item (validar tamanho antes de salvar)

## Estrutura DynamoDB

### Item Order
```json
{
  "OrderId": "ORDER#guid",
  "Code": "ORD202501011234",
  "CustomerId": "guid",
  "CreatedAt": "2025-01-01T00:00:00Z",
  "OrderStatus": 1,
  "TotalPrice": 25.50,
  "OrderSource": "WEB",
  "Items": [
    {
      "Id": "guid",
      "ProductId": "guid",
      "ProductName": "Hambúrguer",
      "Category": 1,
      "Quantity": 2,
      "FinalPrice": 25.50,
      "Observation": "Sem cebola",
      "CustomIngredients": [
        {
          "Id": "guid",
          "Name": "Queijo",
          "Price": 2.00,
          "Quantity": 2
        }
      ]
    }
  ]
}
```

## Como testar
- Criar testes unitários mockando `IAmazonDynamoDB`
- Testar mapeamento de DTO → DynamoDB → DTO (round-trip)
- Validar que Items são serializados/deserializados corretamente
- Testar paginação com ExclusiveStartKey
- Validar tratamento de limite de 400KB

## Critérios de aceite
- [ ] Arquivo `OrderDynamoDbRepository.cs` criado
- [ ] Todos os métodos da interface implementados
- [ ] Mapeamento DTO ↔ DynamoDB funcionando (incluindo Items)
- [ ] Paginação implementada (ExclusiveStartKey)
- [ ] Validação de tamanho de item (< 400KB)
- [ ] Tratamento de erros implementado
- [ ] Código compila sem erros
- [ ] Testes unitários criados

## Observações
- **CRÍTICO:** Items são parte do Order (denormalização), não tabela separada
- Snapshot completo de Product no momento do pedido (ProductName, Category, FinalPrice)
- CustomIngredients com snapshot de preço
- Monitorar tamanho do item (limite 400KB)
- Paginação DynamoDB usa ExclusiveStartKey (não Skip/Take)

