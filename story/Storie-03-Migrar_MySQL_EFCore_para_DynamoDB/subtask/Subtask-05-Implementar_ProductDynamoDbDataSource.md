# Subtask 05: Implementar ProductDynamoDbDataSource

## Status
- **Estado:** 📋 Pendente
- **Data de Conclusão:** [DD/MM/AAAA]

## Descrição
Implementar `ProductDynamoDbDataSource` que implementa `IProductDataSource` e usa `ProductDynamoDbRepository` para acessar DynamoDB. Esta classe substitui `ProductDataSource` (EF Core) mantendo a mesma interface.

## Passos de implementação
- [ ] Criar arquivo `src/Infra/FastFood.OrderHub.Infra.Persistence/DataSources/ProductDynamoDbDataSource.cs`
- [ ] Implementar interface `IProductDataSource`:
  - `GetByIdAsync(Guid id)`
  - `GetAvailableAsync()` → Filtrar por IsActive=true
  - `GetByCategoryAsync(int category)`
  - `ExistsAsync(Guid id)`
  - `AddAsync(ProductDto dto)`
  - `UpdateAsync(ProductDto dto)`
  - `RemoveAsync(Guid id)` → Soft delete (IsActive=false) ou DeleteItem
- [ ] Injetar `ProductDynamoDbRepository` via construtor
- [ ] Delegar chamadas para o repositório
- [ ] Implementar lógica de filtro `IsActive` em `GetAvailableAsync()`
- [ ] Tratar exceções e converter para exceções de aplicação quando necessário

## Arquivos de Referência
- Interface: `02-Core/FastFood.Common/Interfaces/DataSources/IProductDataSource.cs` (projeto atual)
- Implementação atual: `01-InterfacesExternas/FastFood.Infra.Persistence/DataSources/ProductDataSource.cs`

## Como testar
- Testar todos os métodos da interface
- Validar que retorna os mesmos DTOs que a implementação EF Core
- Testar filtro IsActive em GetAvailableAsync
- Validar tratamento de erros

## Critérios de aceite
- [ ] Arquivo `ProductDynamoDbDataSource.cs` criado
- [ ] Implementa `IProductDataSource` completamente
- [ ] Usa `ProductDynamoDbRepository` internamente
- [ ] Filtro IsActive implementado em GetAvailableAsync
- [ ] Tratamento de erros adequado
- [ ] Código compila sem erros
- [ ] Testes unitários criados

## Observações
- Manter compatibilidade total com a interface `IProductDataSource`
- Não alterar contratos de DTOs (mantém compatibilidade com UseCases)
- Soft delete é preferível para manter histórico

