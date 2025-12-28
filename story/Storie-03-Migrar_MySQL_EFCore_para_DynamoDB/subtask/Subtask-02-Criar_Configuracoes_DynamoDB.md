# Subtask 02: Criar Configurações DynamoDB (Tabelas, GSIs)

## Status
- **Estado:** 📋 Pendente
- **Data de Conclusão:** [DD/MM/AAAA]

## Descrição
Criar classe de configuração para definir estrutura das tabelas DynamoDB (`fastfood-products` e `fastfood-orders`) incluindo Partition Keys, Sort Keys (se necessário) e Global Secondary Indexes (GSIs).

## Passos de implementação
- [ ] Criar arquivo `src/Infra/FastFood.OrderHub.Infra.Persistence/Configurations/DynamoDbTableConfiguration.cs`
- [ ] Definir constantes para nomes de tabelas:
  - `FASTFOOD_PRODUCTS_TABLE = "fastfood-products"`
  - `FASTFOOD_ORDERS_TABLE = "fastfood-orders"`
- [ ] Criar classe `ProductTableConfiguration` com:
  - PK: `ProductId` (String)
  - GSI1: `Category-Index` (PK: Category, SK: Name)
- [ ] Criar classe `OrderTableConfiguration` com:
  - PK: `OrderId` (String)
  - GSI1: `CustomerId-CreatedAt-Index` (PK: CustomerId, SK: CreatedAt)
  - GSI2: `Status-CreatedAt-Index` (PK: OrderStatus, SK: CreatedAt) - Opcional
  - GSI3: `Code-Index` (PK: Code, SK: OrderId) - Opcional
- [ ] Documentar estrutura de cada tabela e GSI

## Estrutura Esperada

### Tabela fastfood-products
- **PK:** `ProductId` (String)
- **Atributos:** Name, Description, Category, Price, ImageUrl, IsActive, CreatedAt, BaseIngredients[]
- **GSI1:** Category-Index (PK: Category, SK: Name)

### Tabela fastfood-orders
- **PK:** `OrderId` (String)
- **Atributos:** Code, CustomerId, CreatedAt, OrderStatus, PaymentStatus, TotalPrice, OrderSource, Items[]
- **GSI1:** CustomerId-CreatedAt-Index (PK: CustomerId, SK: CreatedAt)
- **GSI2:** Status-CreatedAt-Index (PK: OrderStatus, SK: CreatedAt) - Opcional
- **GSI3:** Code-Index (PK: Code, SK: OrderId) - Opcional

## Como testar
- Verificar que as classes de configuração compilam
- Validar que constantes estão corretas
- Documentar decisão sobre GSIs opcionais

## Critérios de aceite
- [ ] Arquivo `DynamoDbTableConfiguration.cs` criado
- [ ] Constantes de nomes de tabelas definidas
- [ ] Configurações de ProductTable e OrderTable criadas
- [ ] GSIs documentados e justificados
- [ ] Código compila sem erros

