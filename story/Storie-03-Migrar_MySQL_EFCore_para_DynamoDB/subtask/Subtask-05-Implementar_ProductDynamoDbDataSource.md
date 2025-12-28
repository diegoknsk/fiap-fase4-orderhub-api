# Subtask 05: Implementar ProductDynamoDbDataSource

## Status
- **Estado:** ✅ Concluído
- **Data de Conclusão:** 28/12/2024

## Descrição
Implementar `ProductDynamoDbDataSource` que implementa `IProductDataSource` e usa `ProductDynamoDbRepository` para acessar DynamoDB. Esta classe substitui `ProductDataSource` (EF Core) mantendo a mesma interface.

## Passos de implementação
- [x] Criar arquivo `src/Infra/FastFood.OrderHub.Infra.Persistence/DataSources/ProductDynamoDbDataSource.cs`
- [x] Implementar interface `IProductDataSource`:
  - [x] `GetByIdAsync(Guid id)`
  - [x] `GetAvailableAsync()` → Filtrar por IsActive=true
  - [x] `GetByCategoryAsync(int category)`
  - [x] `GetPagedAsync(int page, int pageSize, int? category, string? name)` → Novo método para paginação
  - [x] `ExistsAsync(Guid id)`
  - [x] `AddAsync(ProductDto dto)` → Define IsActive=true e CreatedAt automaticamente
  - [x] `UpdateAsync(ProductDto dto)`
  - [x] `RemoveAsync(Guid id)` → Soft delete (IsActive=false)
- [x] Injetar `ProductDynamoDbRepository` via construtor
- [x] Delegar chamadas para o repositório
- [x] Implementar lógica de filtro `IsActive` em `GetAvailableAsync()` e `GetPagedAsync()`
- [x] Tratar exceções e converter para exceções de aplicação quando necessário

## Arquivos de Referência
- Interface: `02-Core/FastFood.Common/Interfaces/DataSources/IProductDataSource.cs` (projeto atual)
- Implementação atual: `01-InterfacesExternas/FastFood.Infra.Persistence/DataSources/ProductDataSource.cs`

## Como testar
- Testar todos os métodos da interface
- Validar que retorna os mesmos DTOs que a implementação EF Core
- Testar filtro IsActive em GetAvailableAsync
- Validar tratamento de erros

## Critérios de aceite
- [x] Arquivo `ProductDynamoDbDataSource.cs` criado
- [x] Implementa `IProductDataSource` completamente
- [x] Usa `ProductDynamoDbRepository` internamente
- [x] Filtro IsActive implementado em GetAvailableAsync e GetPagedAsync
- [x] Tratamento de erros adequado
- [x] Código compila sem erros
- [x] Método GetPagedAsync adicionado para suportar paginação

## Observações
- Manter compatibilidade total com a interface `IProductDataSource`
- Não alterar contratos de DTOs (mantém compatibilidade com UseCases)
- Soft delete é preferível para manter histórico

