# Subtask 08: Implementar OrderDynamoDbDataSource

## Status
- **Estado:** 📋 Pendente
- **Data de Conclusão:** [DD/MM/AAAA]

## Descrição
Implementar `OrderDynamoDbDataSource` que implementa `IOrderDataSource` e usa `OrderDynamoDbRepository` para acessar DynamoDB. Esta classe substitui `OrderDataSource` (EF Core) mantendo a mesma interface.

## Passos de implementação
- [ ] Criar arquivo `src/Infra/FastFood.OrderHub.Infra.Persistence/DataSources/OrderDynamoDbDataSource.cs`
- [ ] Implementar interface `IOrderDataSource`:
  - `GetByIdAsync(Guid id)` → Retorna Order completo com Items
  - `GetByCustomerIdAsync(Guid customerId)` → Query GSI CustomerId
  - `GetPagedAsync(int page, int pageSize, int? status)` → Paginação DynamoDB
  - `GetByStatusAsync(int status)` → Query GSI Status
  - `GetByStatusWithoutPreparationAsync(int status)` → Query com filtro adicional
  - `AddAsync(OrderDto dto)` → Criar novo Order
  - `UpdateAsync(OrderDto dto)` → Atualizar Order (incluindo Items)
  - `DeleteAsync(Guid id)` → Deletar Order
  - `ExistsAsync(Guid id)` → Verificar existência
  - `ExistsByCodeAsync(string code)` → Query GSI Code
  - `GenerateOrderCodeAsync()` → Gerar código único
- [ ] Injetar `OrderDynamoDbRepository` via construtor
- [ ] Delegar chamadas para o repositório
- [ ] Converter paginação DynamoDB (ExclusiveStartKey) para formato esperado (Page/PageSize)
- [ ] Tratar exceções e converter para exceções de aplicação quando necessário

## Arquivos de Referência
- Interface: `02-Core/FastFood.Common/Interfaces/DataSources/IOrderDataSource.cs` (projeto atual)
- Implementação atual: `01-InterfacesExternas/FastFood.Infra.Persistence/DataSources/OrderDataSource.cs`

## Desafios Especiais

### Paginação DynamoDB
- DynamoDB não suporta Skip/Take
- Usar `ExclusiveStartKey` para paginação
- Converter Page/PageSize para `Limit` e `ExclusiveStartKey`
- Retornar `PagedList` com informações de paginação

### Items como Parte do Order
- Items são serializados como lista dentro do item Order
- Ao atualizar Order, atualizar Items também (UpdateItem com SET Items = :items)
- Garantir que Items estão sempre sincronizados com Order

## Como testar
- Testar todos os métodos da interface
- Validar que retorna os mesmos DTOs que a implementação EF Core
- Testar paginação (conversão Page → ExclusiveStartKey)
- Validar que Items são incluídos em GetByIdAsync
- Testar geração de código único

## Critérios de aceite
- [ ] Arquivo `OrderDynamoDbDataSource.cs` criado
- [ ] Implementa `IOrderDataSource` completamente
- [ ] Usa `OrderDynamoDbRepository` internamente
- [ ] Paginação DynamoDB implementada (ExclusiveStartKey)
- [ ] Items incluídos em todas as queries de Order
- [ ] Tratamento de erros adequado
- [ ] Código compila sem erros
- [ ] Testes unitários criados

## Observações
- Manter compatibilidade total com a interface `IOrderDataSource`
- Não alterar contratos de DTOs (mantém compatibilidade com UseCases)
- Paginação DynamoDB é diferente de EF Core (adaptar lógica)


