# Storie-03: Migrar MySQL/EF Core para DynamoDB

## Status
- **Estado:** ✅ Implementação Concluída (Parcial)
- **Data de Início:** 28/12/2024
- **Data de Conclusão:** 28/12/2024
- **Observação:** Subtask 09 pendente (UseCases ainda não implementados)

## Descrição
Como desenvolvedor, preciso migrar o módulo FastFood do repositório `fiap-fase3-aplicacao\fiap-fastfood` de MySQL/EF Core para DynamoDB, mantendo o máximo possível do comportamento atual das APIs, fluxos e regras de domínio. A migração deve seguir a "Solução A" (1 tabela para Orders, 1 tabela para Products) e respeitar integralmente a arquitetura já adotada no projeto.

## Objetivo Geral
1. Investigar profundamente o projeto atual para mapear todos os fluxos envolvidos (OrderController e ProductsController)
2. Criar adaptação para DynamoDB respeitando a arquitetura existente (Domain forte, Application/UseCases bem definidos, Infra apenas como implementação)
3. Manter comportamento atual das APIs (contratos e comportamento devem permanecer o mais próximos possível)
4. Atualizar documentação técnica (rules, README, diagramas, decisões arquiteturais) para permitir reaproveitamento do conhecimento

## Contexto do Projeto Atual

### Estrutura Atual
O projeto atual (`fiap-fase3-aplicacao\fiap-fastfood`) utiliza:
- **Banco de Dados:** MySQL com Entity Framework Core
- **Arquitetura:** Clean Architecture com separação em camadas:
  - `FastFood.Domain` - Entidades de domínio, regras de negócio
  - `FastFood.Application` - UseCases, Gateways, Orchestrators
  - `FastFood.Infra.Persistence` - DataSources (implementações EF Core)
  - `FastFood.Api` - Controllers HTTP

### Padrão Arquitetural de Referência
O projeto deve seguir o padrão arquitetural do projeto de referência:
- **Referência:** `C:\Projetos\Fiap\fiap-fase4-auth-lambda`
- **Padrão:** Clean Architecture com Ports/Adapters
- **Estrutura Application:**
  - UseCases pequenos e focados
  - InputModels/OutputModels
  - Presenters (chamados pelo UseCase)
  - Ports (interfaces) na Application
  - Implementações concretas na Infra

## Inventário Detalhado dos Endpoints

### OrderController (`01-InterfacesExternas/FastFood.Api/Controllers/OrderController.cs`)

#### Endpoints Mapeados:

1. **GET `/api/order/{id}`** - Obter pedido por ID
   - **Autorização:** Admin (Cognito)
   - **UseCase:** `GetOrderByIdUseCase`
   - **Orchestrator:** `OrderControllerOrchestrator.GetOrderByIdAsync`
   - **DataSources:** `IOrderDataSource.GetByIdAsync`
   - **Retorno:** `OrderModel` completo com OrderedProducts e Product details

2. **POST `/api/order/start`** - Iniciar novo pedido
   - **Autorização:** Customer (Bearer Token)
   - **UseCase:** `StartOrderUseCase`
   - **Orchestrator:** `OrderControllerOrchestrator.StartOrderAsync`
   - **DataSources:** `IOrderDataSource.AddAsync`, `IOrderDataSource.ExistsByCodeAsync`
   - **Input:** `OrderCustomerStartModel` (CustomerId)
   - **Regras de Negócio:**
     - Gera código único do pedido (formato: `ORD{yyyyMMdd}{random}`)
     - Valida unicidade do código
     - Cria Order com status `Started` e PaymentStatus `NotStarted`
     - Calcula TotalPrice inicial (0)

3. **POST `/api/order/add-product`** - Adicionar produto ao pedido
   - **Autorização:** Customer (Bearer Token)
   - **UseCase:** `AddProductToOrderUseCase`
   - **Orchestrator:** `OrderControllerOrchestrator.AddProductToOrderAsync`
   - **DataSources:** `IOrderDataSource.GetByIdAsync`, `IProductDataSource.GetByIdAsync`, `IOrderedProductDataSource.AddAsync`, `IOrderDataSource.UpdateAsync`
   - **Input:** `AddProductToOrderModel` (OrderId, ProductId, Quantity, Observation, CustomIngredients[])
   - **Regras de Negócio:**
     - Valida existência do pedido e produto
     - Cria OrderedProduct com ingredientes customizados (ou default do produto)
     - Calcula FinalPrice do OrderedProduct (basePrice + customizações) * quantity
     - Adiciona ao pedido e recalcula TotalPrice do Order
     - Persiste OrderedProduct e atualiza Order

4. **PUT `/api/order/update-product`** - Atualizar produto no pedido
   - **Autorização:** Customer (Bearer Token)
   - **UseCase:** `UpdateProductInOrderUseCase`
   - **Orchestrator:** `OrderControllerOrchestrator.UpdateProductInOrderAsync`
   - **DataSources:** `IOrderDataSource.GetByIdAsync`, `IOrderedProductDataSource.GetByIdAsync`, `IOrderedProductDataSource.UpdateAsync`, `IOrderDataSource.UpdateAsync`
   - **Input:** `UpdateProductInOrderModel` (OrderId, OrderedProductId, Quantity, Observation, CustomIngredients[])
   - **Regras de Negócio:**
     - Atualiza quantidade, observação e ingredientes customizados
     - Recalcula FinalPrice do OrderedProduct
     - Recalcula TotalPrice do Order

5. **DELETE `/api/order/remove-product`** - Remover produto do pedido
   - **Autorização:** Customer (Bearer Token)
   - **UseCase:** `RemoveProductFromOrderUseCase`
   - **Orchestrator:** `OrderControllerOrchestrator.RemoveProductFromOrderAsync`
   - **DataSources:** `IOrderDataSource.GetByIdAsync`, `IOrderedProductDataSource.DeleteAsync`, `IOrderDataSource.UpdateAsync`
   - **Input:** `RemoveProductFromOrderModel` (OrderId, OrderedProductId)
   - **Regras de Negócio:**
     - Remove OrderedProduct do pedido
     - Recalcula TotalPrice do Order

6. **POST `/api/order/{id}/confirm-selection`** - Confirmar seleção do pedido
   - **Autorização:** Customer (Bearer Token)
   - **UseCase:** `ConfirmOrderSelectionUseCase`
   - **Orchestrator:** `OrderControllerOrchestrator.ConfirmOrderSelectionAsync`
   - **DataSources:** `IOrderDataSource.GetByIdAsync`, `IOrderDataSource.UpdateAsync`
   - **Regras de Negócio:**
     - Altera OrderStatus para `AwaitingPayment`
     - Mantém TotalPrice calculado (snapshot)

7. **GET `/api/order`** - Listar pedidos paginados (Admin)
   - **Autorização:** Admin (Cognito)
   - **UseCase:** `GetPagedOrdersUseCase`
   - **Orchestrator:** `OrderControllerOrchestrator.GetPagedOrdersAsync`
   - **DataSources:** `IOrderDataSource.GetPagedAsync`
   - **Input:** `OrderFilterParameters` (Page, PageSize, Status?)
   - **Retorno:** `PagedList<OrderModel>`

### ProductsController (`01-InterfacesExternas/FastFood.Api/Controllers/ProductsController.cs`)

#### Endpoints Mapeados:

1. **GET `/api/products`** - Listar produtos paginados
   - **Autorização:** Admin (Cognito)
   - **UseCase:** `GetProductsPagedUseCase`
   - **Orchestrator:** `ProductControllerOrchestrator.GetProductsPagedAsync`
   - **DataSources:** `IProductDataSource.GetAvailableAsync` (com filtros)
   - **Input:** `ProductFilterParameters` (Page, PageSize, Category?, Name?)
   - **Retorno:** `PagedList<ProductModel>`

2. **GET `/api/products/{id}`** - Obter produto por ID
   - **Autorização:** Admin (Cognito)
   - **UseCase:** `GetProductByIdUseCase`
   - **Orchestrator:** `ProductControllerOrchestrator.GetProductByIdAsync`
   - **DataSources:** `IProductDataSource.GetByIdAsync`
   - **Retorno:** `ProductModel` completo com BaseIngredients

3. **POST `/api/products`** - Criar produto
   - **Autorização:** Admin (Cognito)
   - **UseCase:** `CreateProductUseCase`
   - **Orchestrator:** `ProductControllerOrchestrator.CreateAsync`
   - **DataSources:** `IProductDataSource.AddAsync`
   - **Input:** `ProductModel` (Name, Category, Price, Description, ImageUrl, BaseIngredients[])
   - **Regras de Negócio:**
     - Valida Name não vazio
     - Valida Price > 0
     - Cria Product com BaseIngredients (preço específico por produto)

4. **PUT `/api/products/{id}`** - Atualizar produto
   - **Autorização:** Admin (Cognito)
   - **UseCase:** `UpdateProductUseCase`
   - **Orchestrator:** `ProductControllerOrchestrator.UpdateAsync`
   - **DataSources:** `IProductDataSource.UpdateAsync`
   - **Input:** `ProductModel` completo
   - **Regras de Negócio:**
     - Atualiza propriedades do produto
     - Atualiza BaseIngredients (pode adicionar/remover)

5. **DELETE `/api/products/{id}`** - Remover produto
   - **Autorização:** Admin (Cognito)
   - **UseCase:** `DeleteProductUseCase`
   - **Orchestrator:** `ProductControllerOrchestrator.DeleteAsync`
   - **DataSources:** `IProductDataSource.RemoveAsync`
   - **Regras de Negócio:**
     - Remove produto (soft delete ou hard delete conforme regra)

### Controllers Dependentes

#### PaymentController
- **Dependência:** `IOrderDataSource` (para validar e obter pedido)
- **Uso:** Validação de ownership, obtenção de dados do pedido para pagamento

#### PreparationController
- **Dependência:** `IOrderDataSource` (para obter pedidos por status)
- **Uso:** Listagem de pedidos para preparação, atualização de status

#### DeliveryController
- **Dependência:** `IOrderDataSource` (para obter pedidos prontos)
- **Uso:** Listagem de pedidos prontos para entrega

## Entidades de Domínio (Preservar)

### Order (`02-Core/FastFood.Domain/Entities/OrderManagement/Order.cs`)
- **Propriedades:**
  - `Id` (Guid)
  - `Code` (string?)
  - `OrderedProducts` (ICollection<OrderedProduct>)
  - `PaymentStatus` (EnumPaymentStatus)
  - `OrderStatus` (EnumOrderStatus)
  - `CreatedAt` (DateTime)
  - `CustomerId` (Guid?)
  - `TotalPrice` (decimal)
- **Métodos de Domínio:**
  - `AddProduct(OrderedProduct)` - Adiciona produto e recalcula TotalPrice
  - `RemoveProduct(Guid)` - Remove produto e recalcula TotalPrice
  - `CalculateTotalPrice()` - Soma FinalPrice de todos OrderedProducts
  - `FinalizeOrderSelection()` - Altera status para AwaitingPayment
  - `UpdateStatus(EnumOrderStatus)` - Atualiza status
  - `SendToKitchen()`, `SetInPreparation()`, `SetReadyForPickup()` - Transições de status

### Product (`02-Core/FastFood.Domain/Entities/OrderManagement/Product.cs`)
- **Propriedades:**
  - `Id` (Guid)
  - `Name` (string?)
  - `Category` (EnumProductCategory)
  - `Ingredients` (ICollection<ProductBaseIngredient>)
  - `Price` (decimal)
  - `Image` (ImageProduct?)
  - `Description` (string?)
- **Métodos de Domínio:**
  - Validações de Name e Price

### OrderedProduct (`02-Core/FastFood.Domain/Entities/OrderManagement/OrderedProduct.cs`)
- **Propriedades:**
  - `Id` (Guid)
  - `ProductId` (Guid)
  - `Product` (Product?)
  - `OrderId` (Guid?)
  - `CustomIngredients` (ICollection<OrderedProductIngredient>)
  - `Observation` (string?)
  - `FinalPrice` (decimal)
  - `Quantity` (int)
- **Métodos de Domínio:**
  - `CalculateFinalPrice()` - Calcula preço base + customizações * quantity
  - `SetQuantity(int)` - Atualiza quantidade e recalcula preço
  - `SetIngredientQuantity(Guid, int)` - Atualiza quantidade de ingrediente
  - `SetObservation(string?)` - Atualiza observação

### ProductBaseIngredient (`02-Core/FastFood.Domain/Entities/OrderManagement/Ingredients/ProductBaseIngredient.cs`)
- **Propriedades:**
  - `Id` (Guid)
  - `Name` (string?)
  - `Price` (decimal)
  - `ProductId` (Guid)

### OrderedProductIngredient (`02-Core/FastFood.Domain/Entities/OrderManagement/Ingredients/OrderedProductIngredient.cs`)
- **Propriedades:**
  - `Id` (Guid)
  - `Name` (string?)
  - `Price` (decimal) - Snapshot do preço no momento do pedido
  - `Quantity` (int) - 0 a 10
  - `OrderedProductId` (Guid?)
  - `ProductBaseIngredientId` (Guid?)

## Interfaces de DataSource Atuais

### IOrderDataSource (`02-Core/FastFood.Common/Interfaces/DataSources/IOrderDataSource.cs`)
```csharp
Task<OrderDto?> GetByIdAsync(Guid id);
Task<List<OrderDto>> GetByCustomerIdAsync(Guid customerId);
Task<List<OrderDto>> GetPagedAsync(int page, int pageSize, int? status = null);
Task<List<OrderDto>> GetByStatusAsync(int status);
Task<List<OrderDto>> GetByStatusWithoutPreparationAsync(int status);
Task<Guid> AddAsync(OrderDto dto);
Task UpdateAsync(OrderDto dto);
Task DeleteAsync(Guid id);
Task<bool> ExistsAsync(Guid id);
Task<bool> ExistsByCodeAsync(string code);
Task<string> GenerateOrderCodeAsync();
```

### IProductDataSource (`02-Core/FastFood.Common/Interfaces/DataSources/IProductDataSource.cs`)
```csharp
Task<ProductDto?> GetByIdAsync(Guid id);
Task<List<ProductDto>> GetAvailableAsync();
Task<List<ProductDto>> GetByCategoryAsync(int category);
Task<bool> ExistsAsync(Guid id);
Task AddAsync(ProductDto dto);
Task UpdateAsync(ProductDto dto);
Task RemoveAsync(Guid id);
```

### IOrderedProductDataSource (`02-Core/FastFood.Common/Interfaces/DataSources/IOrderDataSource.cs`)
```csharp
Task<OrderedProductDto?> GetByIdAsync(Guid id);
Task<List<OrderedProductDto>> GetByOrderIdAsync(Guid orderId);
Task<Guid> AddAsync(OrderedProductDto dto);
Task UpdateAsync(OrderedProductDto dto);
Task DeleteAsync(Guid id);
Task<bool> ExistsAsync(Guid id);
```

## Modelagem DynamoDB Proposta

### Tabela: `fastfood-products`

**Partition Key (PK):** `ProductId` (String, formato: `PROD#{Guid}` ou apenas `{Guid}`)

**Atributos:**
- `ProductId` (String, PK)
- `Name` (String)
- `Description` (String, nullable)
- `Category` (Number) - EnumProductCategory (1=Meal, 2=SideDish, 3=Drink, 4=Dessert)
- `Price` (Number)
- `ImageUrl` (String, nullable)
- `IsActive` (Boolean, default: true) - Para soft delete
- `CreatedAt` (String, ISO 8601)
- `BaseIngredients` (List<Map>) - Lista de ingredientes base:
  ```json
  {
    "Id": "guid",
    "Name": "string",
    "Price": 0.00
  }
  ```

**Global Secondary Index (GSI) - Opcional:**
- **GSI1:** `Category-Index`
  - PK: `Category` (Number)
  - SK: `Name` (String)
  - **Uso:** Listagem de produtos por categoria ordenada por nome

**Decisão Arquitetural:**
- BaseIngredients armazenados como lista dentro do item Product (denormalização)
- Evita joins e permite leitura completa do produto com um único GetItem
- Trade-off: Se ingrediente mudar, precisa atualizar todos os produtos que o usam (mas ingredientes são específicos por produto)

### Tabela: `fastfood-orders`

**Partition Key (PK):** `OrderId` (String, formato: `ORDER#{Guid}` ou apenas `{Guid}`)

**Atributos:**
- `OrderId` (String, PK)
- `Code` (String, único) - Formato: `ORD{yyyyMMdd}{random}`
- `CustomerId` (String, nullable) - Guid do cliente
- `CreatedAt` (String, ISO 8601)
- `OrderStatus` (Number) - EnumOrderStatus
- `PaymentStatus` (Number) - EnumPaymentStatus
- `TotalPrice` (Number) - Snapshot calculado no checkout
- `OrderSource` (String, nullable) - Origem do pedido (ex: "WEB", "APP")
- `Items` (List<Map>) - Lista de OrderedProducts (snapshot completo):
  ```json
  {
    "Id": "guid",
    "ProductId": "guid",
    "ProductName": "string",
    "Category": 1,
    "Quantity": 1,
    "FinalPrice": 0.00,
    "Observation": "string",
    "CustomIngredients": [
      {
        "Id": "guid",
        "Name": "string",
        "Price": 0.00,
        "Quantity": 1
      }
    ]
  }
  ```

**Global Secondary Indexes (GSIs):**

1. **GSI1: `CustomerId-CreatedAt-Index`**
   - PK: `CustomerId` (String)
   - SK: `CreatedAt` (String, ISO 8601)
   - **Uso:** Listar pedidos de um cliente ordenados por data

2. **GSI2: `Status-CreatedAt-Index`** (Opcional, se necessário)
   - PK: `OrderStatus` (Number)
   - SK: `CreatedAt` (String, ISO 8601)
   - **Uso:** Listar pedidos por status (ex: fila de cozinha)

3. **GSI3: `Code-Index`** (Opcional, para validação de unicidade)
   - PK: `Code` (String)
   - SK: `OrderId` (String)
   - **Uso:** Validar unicidade do código do pedido

**Decisões Arquiteturais:**
- Items (OrderedProducts) armazenados como lista dentro do item Order (denormalização)
- Snapshot completo do produto no momento do pedido (ProductName, Category, FinalPrice)
- CustomIngredients com snapshot de preço (evita alterações retroativas)
- TotalPrice calculado no checkout e persistido (snapshot)
- Trade-off: Item pode ficar grande (limite DynamoDB: 400KB), mas permite leitura completa com um único GetItem
- Se necessário, pode separar Items em tabela separada (mas aumenta complexidade de queries)

## O que Muda vs. O que Permanece

### O que Muda (Apenas na Camada Infra)

1. **Implementação de DataSources:**
   - `OrderDataSource` → `OrderDynamoDbDataSource` (usa AWS SDK DynamoDB)
   - `ProductDataSource` → `ProductDynamoDbDataSource` (usa AWS SDK DynamoDB)
   - `OrderedProductDataSource` → Removido (Items são parte do Order)

2. **Queries:**
   - Sem joins (tudo denormalizado)
   - Uso de GSIs para consultas por CustomerId, Status, Category
   - Paginação via `ExclusiveStartKey` (DynamoDB) ao invés de `Skip/Take` (EF Core)

3. **Transações:**
   - DynamoDB TransactWriteItems para operações atômicas (ex: AddProduct + UpdateOrder)
   - Limite: 25 itens por transação, 4MB de dados

4. **Validações de Unicidade:**
   - `ExistsByCodeAsync` → Query no GSI3 (Code-Index) ou ConditionalCheckFailedException

### O que Permanece (Domain, Application, API)

1. **Entidades de Domínio:**
   - `Order`, `Product`, `OrderedProduct` permanecem **inalteradas**
   - Regras de negócio, invariantes e métodos de domínio **preservados**

2. **UseCases:**
   - Todos os UseCases permanecem **inalterados**
   - Continuam usando Gateways (abstração mantém compatibilidade)

3. **Controllers:**
   - Todos os endpoints permanecem **inalterados**
   - Contratos de API (Request/Response) **preservados**

4. **Gateways:**
   - Interfaces `IOrderGateway`, `IProductGateway` permanecem **inalteradas**
   - Apenas implementações internas mudam (mas não afetam UseCases)

5. **Orchestrators:**
   - `OrderControllerOrchestrator` e `ProductControllerOrchestrator` permanecem **inalterados**

## Arquivos e Camadas Afetadas

### Camada Infra.Persistence (Implementações)

**Arquivos a Criar:**
- `src/Infra/FastFood.OrderHub.Infra.Persistence/Repositories/OrderDynamoDbRepository.cs`
- `src/Infra/FastFood.OrderHub.Infra.Persistence/Repositories/ProductDynamoDbRepository.cs`
- `src/Infra/FastFood.OrderHub.Infra.Persistence/DataSources/OrderDynamoDbDataSource.cs`
- `src/Infra/FastFood.OrderHub.Infra.Persistence/DataSources/ProductDynamoDbDataSource.cs`
- `src/Infra/FastFood.OrderHub.Infra.Persistence/Configurations/DynamoDbConfiguration.cs`
- `src/Infra/FastFood.OrderHub.Infra.Persistence/Extensions/DynamoDbExtensions.cs`

**Arquivos a Modificar:**
- `src/Infra/FastFood.OrderHub.Infra.Persistence/FastFood.OrderHub.Infra.Persistence.csproj` (adicionar AWSSDK.DynamoDBv2)

### Camada Application (Ajustes Mínimos)

**Arquivos a Verificar/Ajustar:**
- `src/Core/FastFood.OrderHub.Application/Gateways/OrderGateway.cs` (pode precisar ajustes para remover dependência de OrderedProductDataSource)
- `src/Core/FastFood.OrderHub.Application/UseCases/OrderManagement/AddProductToOrderUseCase.cs` (ajustar para não usar OrderedProductDataSource separado)

**Arquivos que Permanecem Inalterados:**
- Todos os UseCases (exceto ajustes mínimos acima)
- Todos os Orchestrators
- Todos os Presenters
- Todas as interfaces de Gateway

### Camada Domain (Sem Alterações)

**Arquivos que Permanecem Inalterados:**
- Todas as entidades de domínio
- Todos os Value Objects
- Todas as regras de negócio

### Camada API (Ajustes de DI)

**Arquivos a Modificar:**
- `src/InterfacesExternas/FastFood.OrderHub.Api/Program.cs` (registrar DynamoDB clients e DataSources)
- `src/InterfacesExternas/FastFood.OrderHub.Api/appsettings.json` (configurações DynamoDB)

### Projeto Migrator

**Arquivos a Criar/Modificar:**
- `src/InterfacesExternas/FastFood.OrderHub.Migrator/Program.cs` (criar tabelas DynamoDB)
- `src/InterfacesExternas/FastFood.OrderHub.Migrator/appsettings.json` (configurações DynamoDB)

## Riscos e Limitações do DynamoDB

### Limites Técnicos

1. **Limite de Tamanho de Item: 400KB**
   - **Risco:** Orders com muitos Items podem exceder limite
   - **Mitigação:** 
     - Monitorar tamanho dos pedidos
     - Se necessário, separar Items em tabela separada (aumenta complexidade)
     - Limitar quantidade de produtos por pedido (regra de negócio)

2. **Consistência Eventual em GSIs**
   - **Risco:** Queries em GSIs podem retornar dados desatualizados temporariamente
   - **Mitigação:**
     - Usar consistência forte (StronglyConsistentRead) quando necessário (mais caro)
     - Para listagens, consistência eventual é aceitável

3. **Limite de Transações: 25 itens, 4MB**
   - **Risco:** Operações complexas podem exceder limite
   - **Mitigação:**
     - Dividir operações em múltiplas transações quando necessário
     - Revisar lógica de AddProduct/UpdateProduct para garantir que cabe em uma transação

4. **Sem Joins**
   - **Risco:** Consultas complexas não são possíveis
   - **Mitigação:**
     - Denormalização (já aplicada na modelagem)
     - Se necessário, fazer múltiplas queries e combinar em memória (Application layer)

### Riscos de Migração

1. **Perda de Dados**
   - **Mitigação:** Script de migração de dados do MySQL para DynamoDB (fora do escopo desta story)

2. **Incompatibilidade de Queries**
   - **Mitigação:** Mapear todas as queries atuais para DynamoDB antes de implementar

3. **Performance**
   - **Mitigação:** Testes de carga, otimização de GSIs conforme necessário

## Decisões Arquiteturais

### Decisão 1: Denormalização vs. Normalização
**Escolha:** Denormalização (Items dentro de Order, BaseIngredients dentro de Product)
**Motivo:** 
- DynamoDB não suporta joins
- Leitura completa com um único GetItem (melhor performance)
- Trade-off: Atualizações mais complexas (mas ingredientes raramente mudam)

### Decisão 2: Snapshot vs. Referência
**Escolha:** Snapshot completo (ProductName, Category, FinalPrice, CustomIngredients com preços)
**Motivo:**
- Evita alterações retroativas quando produto muda de preço
- Garante integridade histórica do pedido
- Trade-off: Mais espaço de armazenamento

### Decisão 3: Tabela Separada para OrderedProducts
**Escolha:** Não (Items são parte do Order)
**Motivo:**
- Simplifica queries (um GetItem retorna tudo)
- Melhor performance para leitura completa do pedido
- Trade-off: Limite de 400KB por item (monitorar)

### Decisão 4: GSIs para Queries Comuns
**Escolha:** Criar GSIs para CustomerId, Status (opcional), Code (opcional)
**Motivo:**
- Permite queries eficientes por cliente e status
- Code GSI para validação de unicidade
- Trade-off: Custo adicional de escrita e armazenamento

### Decisão 5: Formato de PK (Prefix vs. Guid Direto)
**Escolha:** Usar prefixo (`ORDER#{Guid}`, `PROD#{Guid}`) ou apenas Guid
**Motivo:**
- Prefixo facilita debugging e identificação visual
- Guid direto é mais simples
- **Recomendação:** Usar prefixo para clareza (decisão final na implementação)

## Subtasks

### Fase 1: Preparação e Configuração
- [Subtask 01: Configurar AWS SDK DynamoDB e Dependências](./subtask/Subtask-01-Configurar_AWS_SDK_DynamoDB.md)
- [Subtask 02: Criar Configurações DynamoDB (Tabelas, GSIs)](./subtask/Subtask-02-Criar_Configuracoes_DynamoDB.md)
- [Subtask 03: Configurar AWS Credentials e Connection](./subtask/Subtask-03-Configurar_AWS_Credentials.md)

### Fase 2: Implementação ProductDataSource
- [Subtask 04: Implementar ProductDynamoDbRepository](./subtask/Subtask-04-Implementar_ProductDynamoDbRepository.md)
- [Subtask 05: Implementar ProductDynamoDbDataSource](./subtask/Subtask-05-Implementar_ProductDynamoDbDataSource.md)
- [Subtask 06: Testar Endpoints ProductsController](./subtask/Subtask-06-Testar_Endpoints_ProductsController.md)

### Fase 3: Implementação OrderDataSource
- [Subtask 07: Implementar OrderDynamoDbRepository](./subtask/Subtask-07-Implementar_OrderDynamoDbRepository.md)
- [Subtask 08: Implementar OrderDynamoDbDataSource](./subtask/Subtask-08-Implementar_OrderDynamoDbDataSource.md)
- [Subtask 09: Ajustar UseCases para Remover OrderedProductDataSource](./subtask/Subtask-09-Ajustar_UseCases_OrderedProduct.md)
- [Subtask 10: Testar Endpoints OrderController](./subtask/Subtask-10-Testar_Endpoints_OrderController.md)

### Fase 4: Migrator e Seed
- [Subtask 11: Implementar Migrator para Criar Tabelas DynamoDB](./subtask/Subtask-11-Implementar_Migrator_DynamoDB.md)
- [Subtask 12: Criar Script de Seed Inicial (Opcional)](./subtask/Subtask-12-Criar_Script_Seed.md)

### Fase 5: Testes e Validação
- [Subtask 13: Testes Unitários para Repositórios DynamoDB](./subtask/Subtask-13-Testes_Unitarios_Repositorios.md)
- [Subtask 14: Testes de Integração End-to-End](./subtask/Subtask-14-Testes_Integracao_E2E.md)
- [Subtask 15: Validação de Performance e Limites](./subtask/Subtask-15-Validacao_Performance_Limites.md)

### Fase 6: Documentação
- [Subtask 16: Atualizar README com Modelagem DynamoDB](./subtask/Subtask-16-Atualizar_README.md)
- [Subtask 17: Documentar Decisões Arquiteturais (ADR)](./subtask/Subtask-17-Documentar_Decisoes_Arquiteturais.md)
- [Subtask 18: Atualizar Rules com Padrão DynamoDB](./subtask/Subtask-18-Atualizar_Rules_Padrao_DynamoDB.md)

## Checklist de Testes

### Testes Funcionais
- [ ] GET `/api/products` - Listar produtos paginados
- [ ] GET `/api/products/{id}` - Obter produto por ID
- [ ] POST `/api/products` - Criar produto
- [ ] PUT `/api/products/{id}` - Atualizar produto
- [ ] DELETE `/api/products/{id}` - Remover produto
- [ ] GET `/api/order/{id}` - Obter pedido por ID
- [ ] POST `/api/order/start` - Iniciar pedido
- [ ] POST `/api/order/add-product` - Adicionar produto ao pedido
- [ ] PUT `/api/order/update-product` - Atualizar produto no pedido
- [ ] DELETE `/api/order/remove-product` - Remover produto do pedido
- [ ] POST `/api/order/{id}/confirm-selection` - Confirmar seleção
- [ ] GET `/api/order` - Listar pedidos paginados

### Testes de Regras de Negócio
- [ ] Cálculo de FinalPrice do OrderedProduct (basePrice + customizações) * quantity
- [ ] Cálculo de TotalPrice do Order (soma de FinalPrice de todos Items)
- [ ] Validação de unicidade do código do pedido
- [ ] Transições de status do pedido
- [ ] Validação de quantidade de ingredientes (0 a 10)
- [ ] Snapshot de preços no momento do pedido

### Testes de Performance
- [ ] Leitura completa de Order com muitos Items (< 400KB)
- [ ] Query por CustomerId (GSI)
- [ ] Query por Status (GSI)
- [ ] Transações atômicas (AddProduct + UpdateOrder)

### Testes de Limites
- [ ] Item Order próximo ao limite de 400KB
- [ ] Transação com múltiplos itens (limite 25)
- [ ] Consistência eventual em GSIs

## Critérios de Aceite da História

### Funcionais
- [ ] Todos os endpoints de OrderController funcionando com DynamoDB
- [ ] Todos os endpoints de ProductsController funcionando com DynamoDB
- [ ] Comportamento das APIs idêntico ao atual (contratos preservados)
- [ ] Regras de negócio preservadas (cálculos, validações, transições de status)
- [ ] Tabelas DynamoDB criadas com estrutura correta (PK, GSIs)

### Técnicos
- [ ] Implementações DynamoDB apenas na camada Infra.Persistence
- [ ] Domain e Application permanecem sem dependência de DynamoDB
- [ ] Interfaces de DataSource mantidas (compatibilidade)
- [ ] Migrator criando tabelas corretamente
- [ ] Configurações AWS (credentials, region) via variáveis de ambiente

### Arquiteturais
- [ ] Padrão arquitetural do projeto de referência seguido
- [ ] Separação de responsabilidades mantida (Domain, Application, Infra)
- [ ] Sem vazamento de detalhes de infraestrutura para Domain/Application
- [ ] Documentação atualizada (README, Rules, ADRs)

### Qualidade
- [ ] Testes unitários para repositórios DynamoDB
- [ ] Testes de integração end-to-end
- [ ] Código compila sem erros
- [ ] Sem code smells críticos (Sonar)
- [ ] Performance validada (queries < 100ms para casos comuns)

## Observações Finais

- **Migração de Dados:** Esta story não inclui migração de dados existentes do MySQL para DynamoDB. Isso deve ser feito em uma story separada.
- **Rollback:** Manter código MySQL/EF Core comentado ou em branch separado para rollback se necessário.
- **Monitoramento:** Após deploy, monitorar:
  - Tamanho dos itens (evitar > 400KB)
  - Latência das queries
  - Consumo de RCU/WCU (Read/Write Capacity Units)
  - Erros de transação (limite 25 itens)

## Referências

- **Projeto de Referência Arquitetural:** `C:\Projetos\Fiap\fiap-fase4-auth-lambda`
- **Projeto Atual (MySQL):** `C:\Projetos\Fiap\fiap-fase3-aplicacao\fiap-fastfood`
- **Documentação DynamoDB:** [AWS DynamoDB Developer Guide](https://docs.aws.amazon.com/amazon-dynamodb/latest/developerguide/)
- **AWS SDK .NET:** [AWSSDK.DynamoDBv2](https://www.nuget.org/packages/AWSSDK.DynamoDBv2/)

