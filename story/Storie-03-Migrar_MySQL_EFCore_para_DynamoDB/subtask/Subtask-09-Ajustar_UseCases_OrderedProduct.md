# Subtask 09: Ajustar UseCases para Remover OrderedProductDataSource

## Status
- **Estado:** 📋 Pendente
- **Data de Conclusão:** [DD/MM/AAAA]

## Descrição
Ajustar UseCases que usam `IOrderedProductDataSource` separado, pois no DynamoDB os Items (OrderedProducts) são parte do Order (denormalização). Os UseCases devem atualizar o Order completo incluindo Items, ao invés de manipular OrderedProducts separadamente.

## UseCases Afetados

### AddProductToOrderUseCase
**Mudança necessária:**
- Remover dependência de `IOrderedProductDataSource`
- Ao adicionar produto, atualizar Order.Items diretamente
- Usar `OrderGateway.UpdateAsync(order)` que atualiza Order completo com Items

### UpdateProductInOrderUseCase
**Mudança necessária:**
- Remover dependência de `IOrderedProductDataSource`
- Buscar Order completo com Items
- Atualizar Item específico dentro de Order.Items
- Salvar Order completo atualizado

### RemoveProductFromOrderUseCase
**Mudança necessária:**
- Remover dependência de `IOrderedProductDataSource`
- Buscar Order completo com Items
- Remover Item específico de Order.Items
- Salvar Order completo atualizado

## Passos de implementação
- [ ] Revisar `AddProductToOrderUseCase.cs`
  - Remover `OrderedProductGateway` do construtor
  - Remover chamada `_orderedProductGateway.AddAsync(orderedProduct)`
  - Garantir que `order.AddProduct(orderedProduct)` atualiza Items
  - Usar apenas `_orderGateway.UpdateAsync(order)` para salvar
- [ ] Revisar `UpdateProductInOrderUseCase.cs`
  - Remover `OrderedProductGateway` do construtor
  - Buscar Order completo com Items via `_orderGateway.GetByIdAsync`
  - Atualizar Item específico dentro de `order.OrderedProducts`
  - Usar `_orderGateway.UpdateAsync(order)` para salvar
- [ ] Revisar `RemoveProductFromOrderUseCase.cs`
  - Remover `OrderedProductGateway` do construtor
  - Buscar Order completo com Items via `_orderGateway.GetByIdAsync`
  - Remover Item de `order.OrderedProducts`
  - Usar `_orderGateway.UpdateAsync(order)` para salvar
- [ ] Atualizar `OrderControllerOrchestrator.cs`
  - Remover `IOrderedProductDataSource` do construtor
  - Remover criação de `OrderedProductGateway`
  - Atualizar injeção de dependências nos UseCases
- [ ] Verificar que todos os UseCases compilam sem erros

## Arquivos a Modificar
- `src/Core/FastFood.OrderHub.Application/UseCases/OrderManagement/AddProductToOrderUseCase.cs`
- `src/Core/FastFood.OrderHub.Application/UseCases/OrderManagement/UpdateProductInOrderUseCase.cs`
- `src/Core/FastFood.OrderHub.Application/UseCases/OrderManagement/RemoveProductFromOrderUseCase.cs`
- `src/Core/FastFood.OrderHub.Application/Controllers/OrderManagement/OrderControllerOrchestrator.cs`

## Como testar
- Testar AddProductToOrder: verificar que Item é adicionado ao Order.Items
- Testar UpdateProductInOrder: verificar que Item específico é atualizado
- Testar RemoveProductFromOrder: verificar que Item é removido de Order.Items
- Validar que Order.Items está sempre sincronizado após operações

## Critérios de aceite
- [ ] `AddProductToOrderUseCase` não usa mais `OrderedProductGateway`
- [ ] `UpdateProductInOrderUseCase` não usa mais `OrderedProductGateway`
- [ ] `RemoveProductFromOrderUseCase` não usa mais `OrderedProductGateway`
- [ ] `OrderControllerOrchestrator` não injeta mais `IOrderedProductDataSource`
- [ ] Todos os UseCases atualizam Order completo com Items
- [ ] Código compila sem erros
- [ ] Testes unitários atualizados

## Observações
- **IMPORTANTE:** Items são parte do Order no DynamoDB (denormalização)
- Não há mais tabela separada para OrderedProducts
- Todas as operações devem atualizar Order completo
- OrderGateway.UpdateAsync deve atualizar Items também

