# Regras para Escrita de Testes

Este documento define as regras e padrões para escrita de testes que devem ser seguidos em todos os projetos da organização.

## Índice

1. [Regras Gerais](#regras-gerais)
2. [Padrões de Nomenclatura](#padrões-de-nomenclatura)
3. [Estrutura de Organização](#estrutura-de-organização)
4. [Testes Unitários](#testes-unitários)
5. [Testes de Integração](#testes-de-integração)
6. [Testes BDD](#testes-bdd)
7. [Cobertura de Testes](#cobertura-de-testes)
8. [Integração com SonarCloud](#integração-com-sonarcloud)
9. [Boas Práticas](#boas-práticas)
10. [Anti-padrões](#anti-padrões)

---

## Regras Gerais

### Princípios Fundamentais

1. **Testes devem ser independentes**: Cada teste deve poder ser executado isoladamente, sem depender de outros testes.
2. **Testes devem ser determinísticos**: Um teste deve sempre produzir o mesmo resultado quando executado com as mesmas condições.
3. **Testes devem ser rápidos**: Testes unitários devem executar em milissegundos.
4. **Testes devem ser legíveis**: O código de teste deve ser autoexplicativo e fácil de entender.
5. **Testes devem ser mantíveis**: Mudanças no código de produção não devem quebrar testes desnecessariamente.
6. **SEMPRE executar testes após criá-los**: Após criar ou modificar testes, SEMPRE executar `dotnet test` para verificar se compilam e passam corretamente antes de considerar a tarefa concluída.

### Framework de Testes

- **xUnit**: Framework padrão para testes unitários e de integração
- **Moq**: Framework padrão para criação de mocks
- **FluentAssertions**: Biblioteca para assertions mais legíveis (opcional, mas recomendado)
- **SpecFlow**: Framework para testes BDD (quando aplicável)

---

## Padrões de Nomenclatura

### Estrutura de Nomenclatura

```
[ClasseOuMétodoSobTeste]_[Cenário]_[ResultadoEsperado]
```

### Exemplos

```csharp
// ✅ CORRETO
[Fact]
public void CreateOrder_WhenValidInput_ShouldReturnSuccess()
{
    // Arrange, Act, Assert
}

[Fact]
public void CreateOrder_WhenCustomerNotFound_ShouldThrowNotFoundException()
{
    // Arrange, Act, Assert
}

// ❌ INCORRETO
[Fact]
public void Test1() { }

[Fact]
public void CreateOrderTest() { }
```

### Convenções

- Use nomes descritivos em inglês
- Use underscores para separar partes do nome
- Seja específico sobre o cenário e resultado esperado
- Evite abreviações desnecessárias

---

## Estrutura de Organização

### Organização de Arquivos

```
src/tests/
├── FastFood.OrderHub.Tests.Unit/
│   ├── Domain/
│   │   └── Entities/
│   ├── Application/
│   │   └── UseCases/
│   └── Infra/
│       └── Services/
├── FastFood.OrderHub.Tests.Integration/
│   ├── Controllers/
│   └── Repositories/
└── FastFood.OrderHub.Tests.Bdd/
    └── Features/
```

### Regra de Espelhamento

A estrutura de testes deve espelhar a estrutura do código de produção:

- `Domain/Entities/OrderTests.cs` testa `Domain/Entities/Order.cs`
- `Application/UseCases/CreateOrderUseCaseTests.cs` testa `Application/UseCases/CreateOrderUseCase.cs`

---

## Testes Unitários

### Padrão AAA (Arrange, Act, Assert)

Todos os testes unitários devem seguir o padrão AAA:

```csharp
[Fact]
public void CreateOrder_WhenValidInput_ShouldReturnSuccess()
{
    // Arrange
    var customerId = Guid.NewGuid();
    var orderItems = new List<OrderItem> { /* ... */ };
    var useCase = new CreateOrderUseCase(/* dependencies */);
    
    // Act
    var result = useCase.Execute(customerId, orderItems);
    
    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    result.Value.OrderId.Should().NotBeEmpty();
}
```

### Regras para Testes Unitários

1. **Um teste, uma responsabilidade**: Cada teste deve verificar apenas um comportamento.
2. **Use mocks para dependências externas**: Não use dependências reais (banco de dados, APIs externas, etc.).
3. **Teste casos de sucesso e falha**: Cubra tanto o caminho feliz quanto os casos de erro.
4. **Teste valores limite**: Teste valores mínimos, máximos e edge cases.
5. **Evite lógica complexa no teste**: O teste deve ser simples e direto.

### Exemplo Completo

```csharp
public class CreateOrderUseCaseTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly CreateOrderUseCase _useCase;
    
    public CreateOrderUseCaseTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _useCase = new CreateOrderUseCase(
            _orderRepositoryMock.Object,
            _customerRepositoryMock.Object
        );
    }
    
    [Fact]
    public void CreateOrder_WhenValidInput_ShouldReturnSuccess()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = new Customer(customerId, "John Doe");
        var orderItems = new List<OrderItem>
        {
            new OrderItem(Guid.NewGuid(), 2, 10.50m)
        };
        
        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customerId))
            .ReturnsAsync(customer);
        
        _orderRepositoryMock
            .Setup(x => x.SaveAsync(It.IsAny<Order>()))
            .ReturnsAsync((Order o) => o);
        
        // Act
        var result = await _useCase.ExecuteAsync(customerId, orderItems);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.CustomerId.Should().Be(customerId);
        result.Value.Items.Should().HaveCount(1);
        
        _orderRepositoryMock.Verify(
            x => x.SaveAsync(It.IsAny<Order>()),
            Times.Once
        );
    }
    
    [Fact]
    public void CreateOrder_WhenCustomerNotFound_ShouldReturnFailure()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var orderItems = new List<OrderItem>();
        
        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customerId))
            .ReturnsAsync((Customer)null);
        
        // Act
        var result = await _useCase.ExecuteAsync(customerId, orderItems);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Customer not found");
        
        _orderRepositoryMock.Verify(
            x => x.SaveAsync(It.IsAny<Order>()),
            Times.Never
        );
    }
}
```

---

## Testes de Integração

### Quando Usar Testes de Integração

- Testar integração entre camadas (ex: Controller → UseCase → Repository)
- Testar integração com banco de dados
- Testar integração com serviços externos (com mocks quando possível)
- Validar contratos HTTP (endpoints da API)

### Estrutura de Testes de Integração

```csharp
public class OrderControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    
    public OrderControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }
    
    [Fact]
    public async Task CreateOrder_WhenValidRequest_ShouldReturn201Created()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<OrderItemRequest> { /* ... */ }
        };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var order = await response.Content.ReadFromJsonAsync<OrderResponse>();
        order.Should().NotBeNull();
        order.Id.Should().NotBeEmpty();
    }
}
```

### Regras para Testes de Integração

1. **Use TestContainers ou banco de dados em memória**: Evite usar banco de dados real em testes.
2. **Limpe dados após cada teste**: Garanta que testes não interfiram entre si.
3. **Teste cenários end-to-end**: Valide o fluxo completo da requisição.
4. **Use factories para setup**: Facilite a criação de dados de teste.

---

## Testes BDD

### Quando Usar Testes BDD

- Validar comportamentos críticos do sistema
- Documentar requisitos de negócio
- Facilitar comunicação entre equipes

### Estrutura de Testes BDD (SpecFlow)

```gherkin
Feature: Create Order
    As a customer
    I want to create an order
    So that I can purchase products

    Scenario: Customer creates a valid order
        Given I am a registered customer with ID "123e4567-e89b-12d3-a456-426614174000"
        And the customer has a valid address
        When I create an order with the following items:
            | ProductId | Quantity | Price |
            | prod-1    | 2        | 10.50 |
        Then the order should be created successfully
        And the order status should be "Pending"
        And the order total should be 21.00
```

### Implementação SpecFlow

```csharp
[Binding]
public class OrderSteps
{
    private readonly OrderContext _context;
    
    public OrderSteps(OrderContext context)
    {
        _context = context;
    }
    
    [Given(@"I am a registered customer with ID ""(.*)""")]
    public void GivenIAmARegisteredCustomer(string customerId)
    {
        _context.CustomerId = Guid.Parse(customerId);
    }
    
    [When(@"I create an order with the following items:")]
    public async Task WhenICreateAnOrder(Table table)
    {
        var items = table.CreateSet<OrderItemRequest>();
        _context.OrderRequest = new CreateOrderRequest
        {
            CustomerId = _context.CustomerId,
            Items = items.ToList()
        };
        
        _context.Response = await _context.Client
            .PostAsJsonAsync("/api/orders", _context.OrderRequest);
    }
    
    [Then(@"the order should be created successfully")]
    public void ThenTheOrderShouldBeCreatedSuccessfully()
    {
        _context.Response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
```

---

## Cobertura de Testes

### Meta de Cobertura

- **Mínimo obrigatório: 80% de cobertura de código**
- Cobertura deve ser medida em todas as camadas:
  - Domain: ≥ 90%
  - Application (UseCases): ≥ 80%
  - Infra (Gateways, Services): ≥ 75%
  - API (Controllers): ≥ 70%

### Exclusões de Cobertura

Os seguintes arquivos podem ser excluídos da cobertura:

- `Program.cs`
- `Startup.cs`
- `*Dto.cs` (Data Transfer Objects)
- `Migrations/**` (migrações de banco de dados)
- `*Configuration.cs` (classes de configuração simples)

### Como Medir Cobertura

```bash
# Executar testes com cobertura
dotnet test --collect:"XPlat Code Coverage"

# Ou usando Coverlet diretamente
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

---

## Integração com SonarCloud

### Configuração do Projeto de Teste

Para que a cobertura seja coletada corretamente e enviada ao SonarCloud, o projeto de teste deve incluir os pacotes Coverlet:

```xml
<ItemGroup>
  <PackageReference Include="coverlet.collector" Version="6.0.0" />
  <PackageReference Include="coverlet.msbuild" Version="6.0.0">
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    <PrivateAssets>all</PrivateAssets>
  </PackageReference>
</ItemGroup>
```

### Configuração do Workflow GitHub Actions

#### 1. Build com Símbolos de Debug

**CRÍTICO**: O build deve gerar arquivos `.pdb` (símbolos de debug) para que o SonarCloud possa processar a cobertura corretamente:

```yaml
- name: Build
  run: dotnet build FastFood.OrderHub.sln -c Release --no-restore /p:DebugType=portable /p:DebugSymbols=true
```

**Por quê?**: Os arquivos `.pdb` são essenciais para mapear o código coberto de volta ao código-fonte original.

#### 2. Execução de Testes com Cobertura

Configure os testes para gerar cobertura no formato OpenCover:

```yaml
- name: Test (Unit + BDD) with coverage
  run: >
    dotnet test FastFood.OrderHub.sln -c Release --no-build
    --logger "trx;LogFileName=test_results.trx"
    /p:CollectCoverage=true
    /p:CoverletOutputFormat="opencover"
    /p:CoverletOutput="./TestResults/coverage/"
```

**Importante**: 
- Use `CoverletOutputFormat="opencover"` (formato suportado pelo SonarCloud)
- O `CoverletOutput` deve apontar para um diretório relativo à raiz do projeto

#### 3. Consolidação de Arquivos de Cobertura

**PROBLEMA COMUM**: O Coverlet gera arquivos de cobertura dentro de cada projeto de teste (ex: `./src/tests/Projeto.Tests/TestResults/coverage/coverage.opencover.xml`), mas o SonarCloud precisa encontrar o arquivo em um local conhecido.

**Solução**: Consolide os arquivos de cobertura em um único local:

```yaml
- name: Consolidate coverage reports
  run: |
    mkdir -p ./TestResults/coverage
    # Find all coverage files and copy the first one (or merge if needed)
    COVERAGE_FILES=$(find . -path "*/TestResults/*/coverage.opencover.xml" -type f)
    if [ -n "$COVERAGE_FILES" ]; then
      FIRST_FILE=$(echo "$COVERAGE_FILES" | head -n 1)
      cp "$FIRST_FILE" ./TestResults/coverage/coverage.opencover.xml
      echo "=== Coverage file consolidated ==="
      ls -la ./TestResults/coverage/
    else
      echo "No coverage files found to consolidate"
    fi
```

**Nota**: Se houver múltiplos projetos de teste, considere usar uma ferramenta de merge para consolidar todos os arquivos em um único relatório.

#### 4. Configuração do SonarScanner Begin

Configure o SonarScanner com as propriedades corretas:

```yaml
- name: Sonar - Begin
  env:
    SONAR_TOKEN: ${{ secrets.SONAR_TOKEN }}
  run: >
    dotnet-sonarscanner begin
    /k:"organizacao_projeto"
    /o:"organizacao"
    /d:sonar.token="$SONAR_TOKEN"
    /d:sonar.cs.opencover.reportsPaths="**/TestResults/coverage/coverage.opencover.xml"
    /d:sonar.coverage.exclusions="**/*Program.cs,**/*Startup.cs,**/Migrations/**,**/*Dto.cs"
    /d:sonar.scanner.scanAll=false
    /d:sonar.projectBaseDir="${{ github.workspace }}"
```

**Propriedades importantes**:

- `sonar.cs.opencover.reportsPaths`: Use padrão `**/TestResults/coverage/coverage.opencover.xml` para encontrar o arquivo consolidado
- `sonar.coverage.exclusions`: Exclua arquivos que não devem ser contabilizados na cobertura
- `sonar.scanner.scanAll=false`: Desabilita análise multi-idioma (evita avisos e problemas)
- `sonar.projectBaseDir`: Define o diretório base do projeto (ajuda na resolução de caminhos)

#### 5. Verificação Antes do Sonar End

Adicione um step de verificação para garantir que o arquivo de cobertura existe antes do Sonar End:

```yaml
- name: Verify coverage file before Sonar End
  run: |
    echo "=== Verifying coverage file ==="
    if [ -f "./TestResults/coverage/coverage.opencover.xml" ]; then
      echo "✓ Coverage file exists at ./TestResults/coverage/coverage.opencover.xml"
      ls -lh ./TestResults/coverage/coverage.opencover.xml
      echo "File size: $(du -h ./TestResults/coverage/coverage.opencover.xml | cut -f1)"
    else
      echo "✗ Coverage file NOT found at ./TestResults/coverage/coverage.opencover.xml"
      echo "Searching for coverage files..."
      find . -name "coverage.opencover.xml" -type f
      exit 1
    fi
```

### Configuração do SonarCloud

#### ⚠️ CRÍTICO: Desabilitar Análise Automática

**Erro comum**: `ERROR: You are running CI analysis while Automatic Analysis is enabled.`

**Solução**: A Análise Automática deve ser desabilitada no SonarCloud para evitar conflitos com a análise via CI/CD:

1. Acesse o SonarCloud: https://sonarcloud.io
2. Navegue até o projeto
3. Vá em **Administration** → **Analysis Method**
4. Na seção **Automatic Analysis**, desative essa opção
5. Salve as alterações

**Por quê?**: A análise automática e a análise via CI/CD são mutuamente exclusivas. Se ambas estiverem ativas, o SonarCloud retornará erro.

### Checklist de Configuração SonarCloud

Antes de considerar a integração completa, verifique:

- [ ] Pacotes Coverlet instalados no projeto de teste (`coverlet.collector` e `coverlet.msbuild`)
- [ ] Build configurado com `/p:DebugType=portable /p:DebugSymbols=true`
- [ ] Testes executados com `/p:CollectCoverage=true /p:CoverletOutputFormat="opencover"`
- [ ] Step de consolidação de cobertura configurado
- [ ] `sonar.cs.opencover.reportsPaths` apontando para o arquivo consolidado
- [ ] `sonar.scanner.scanAll=false` configurado
- [ ] `sonar.projectBaseDir` configurado
- [ ] Step de verificação antes do Sonar End
- [ ] Análise Automática desabilitada no SonarCloud
- [ ] Secret `SONAR_TOKEN` configurado no GitHub

### Troubleshooting

#### Problema: Cobertura não aparece no SonarCloud

**Verificações**:

1. **Arquivo de cobertura existe?**
   - Verifique o step "List coverage files" ou "Verify coverage file"
   - Confirme que o arquivo foi consolidado em `./TestResults/coverage/coverage.opencover.xml`

2. **Caminho está correto?**
   - Verifique se `sonar.cs.opencover.reportsPaths` está usando o padrão correto
   - Use `**/TestResults/coverage/coverage.opencover.xml` para busca recursiva

3. **Símbolos de debug foram gerados?**
   - Confirme que o build inclui `/p:DebugType=portable /p:DebugSymbols=true`
   - Verifique se arquivos `.pdb` foram gerados junto com os `.dll`

4. **Análise Automática está desabilitada?**
   - Verifique em SonarCloud → Administration → Analysis Method
   - Certifique-se de que a Análise Automática está desativada

5. **Formato do arquivo está correto?**
   - O arquivo deve estar em formato OpenCover (`.opencover.xml`)
   - Verifique se o arquivo não está vazio ou corrompido

#### Problema: Erro "You are running CI analysis while Automatic Analysis is enabled"

**Solução**: Desabilite a Análise Automática no SonarCloud (veja seção acima).

#### Problema: Arquivo de cobertura não encontrado

**Solução**: 
- Verifique onde o Coverlet está gerando os arquivos (geralmente dentro de cada projeto de teste)
- Ajuste o step de consolidação para encontrar e copiar os arquivos corretamente
- Use `find . -name "coverage.opencover.xml"` para localizar todos os arquivos gerados

### Exemplo Completo de Workflow

```yaml
name: PR - Build, Test, Sonar

on:
  pull_request:
    branches: [ "main" ]
    types: [opened, synchronize, reopened]
  push:
    branches: [ "main" ]

jobs:
  quality:
    runs-on: ubuntu-latest

    steps:
      - name: Checkout
        uses: actions/checkout@b4ffde65f46336ab88eb53be808477a3936bae11 # v4
        with:
          fetch-depth: 0

      - name: Setup .NET
        uses: actions/setup-dotnet@67a3573c9a986a3f9c594539f4ab511d57bb3ce9 # v4
        with:
          dotnet-version: "8.0.x"

      - name: Cache Sonar
        uses: actions/cache@0057852bfaa89a56745cba8c7296529d2fc39830 # v4
        with:
          path: ~/.sonar/cache
          key: ${{ runner.os }}-sonar
          restore-keys: ${{ runner.os }}-sonar

      - name: Install SonarScanner for .NET
        run: dotnet tool install --global dotnet-sonarscanner

      - name: Sonar - Begin
        env:
          SONAR_TOKEN: ${{ secrets.SONAR_TOKEN }}
        run: >
          dotnet-sonarscanner begin
          /k:"organizacao_projeto"
          /o:"organizacao"
          /d:sonar.token="$SONAR_TOKEN"
          /d:sonar.cs.opencover.reportsPaths="**/TestResults/coverage/coverage.opencover.xml"
          /d:sonar.coverage.exclusions="**/*Program.cs,**/*Startup.cs,**/Migrations/**,**/*Dto.cs"
          /d:sonar.scanner.scanAll=false
          /d:sonar.projectBaseDir="${{ github.workspace }}"

      - name: Restore
        run: dotnet restore Projeto.sln

      - name: Build
        run: dotnet build Projeto.sln -c Release --no-restore /p:DebugType=portable /p:DebugSymbols=true

      - name: Test with coverage
        run: >
          dotnet test Projeto.sln -c Release --no-build
          --logger "trx;LogFileName=test_results.trx"
          /p:CollectCoverage=true
          /p:CoverletOutputFormat="opencover"
          /p:CoverletOutput="./TestResults/coverage/"

      - name: Consolidate coverage reports
        run: |
          mkdir -p ./TestResults/coverage
          COVERAGE_FILES=$(find . -path "*/TestResults/*/coverage.opencover.xml" -type f)
          if [ -n "$COVERAGE_FILES" ]; then
            FIRST_FILE=$(echo "$COVERAGE_FILES" | head -n 1)
            cp "$FIRST_FILE" ./TestResults/coverage/coverage.opencover.xml
            echo "=== Coverage file consolidated ==="
            ls -la ./TestResults/coverage/
          else
            echo "No coverage files found to consolidate"
          fi

      - name: Verify coverage file before Sonar End
        run: |
          if [ -f "./TestResults/coverage/coverage.opencover.xml" ]; then
            echo "✓ Coverage file exists"
            ls -lh ./TestResults/coverage/coverage.opencover.xml
          else
            echo "✗ Coverage file NOT found"
            find . -name "coverage.opencover.xml" -type f
            exit 1
          fi

      - name: Sonar - End
        env:
          SONAR_TOKEN: ${{ secrets.SONAR_TOKEN }}
        run: dotnet-sonarscanner end /d:sonar.token="$SONAR_TOKEN"
```

---

## Boas Práticas

### 1. Use Test Data Builders

```csharp
public class OrderBuilder
{
    private Guid _customerId = Guid.NewGuid();
    private List<OrderItem> _items = new();
    
    public OrderBuilder WithCustomerId(Guid customerId)
    {
        _customerId = customerId;
        return this;
    }
    
    public OrderBuilder WithItem(OrderItem item)
    {
        _items.Add(item);
        return this;
    }
    
    public Order Build()
    {
        return new Order(_customerId, _items);
    }
}

// Uso
var order = new OrderBuilder()
    .WithCustomerId(customerId)
    .WithItem(new OrderItem(productId, 2, 10.50m))
    .Build();
```

### 2. Use Helpers para Setup Comum

```csharp
public static class TestHelpers
{
    public static CreateOrderUseCase CreateOrderUseCase(
        IOrderRepository orderRepository = null,
        ICustomerRepository customerRepository = null)
    {
        return new CreateOrderUseCase(
            orderRepository ?? Mock.Of<IOrderRepository>(),
            customerRepository ?? Mock.Of<ICustomerRepository>()
        );
    }
}
```

### 3. Use FluentAssertions para Assertions Mais Legíveis

```csharp
// ✅ Usando FluentAssertions
result.Should().NotBeNull();
result.IsSuccess.Should().BeTrue();
result.Value.OrderId.Should().NotBeEmpty();

// ❌ Sem FluentAssertions
Assert.NotNull(result);
Assert.True(result.IsSuccess);
Assert.NotEqual(Guid.Empty, result.Value.OrderId);
```

### 4. Organize Testes por Cenário

```csharp
public class CreateOrderUseCaseTests
{
    [Fact]
    public void CreateOrder_WhenValidInput_ShouldReturnSuccess() { }
    
    [Fact]
    public void CreateOrder_WhenCustomerNotFound_ShouldReturnFailure() { }
    
    [Fact]
    public void CreateOrder_WhenItemsEmpty_ShouldReturnFailure() { }
    
    [Fact]
    public void CreateOrder_WhenRepositoryFails_ShouldReturnFailure() { }
}
```

---

## Anti-padrões

### ❌ NÃO Faça Isso

1. **Testes dependentes entre si**
```csharp
// ❌ ERRADO
[Fact]
public void Test1() 
{
    _sharedState = "value";
}

[Fact]
public void Test2() 
{
    Assert.Equal("value", _sharedState); // Depende de Test1
}
```

2. **Lógica complexa no teste**
```csharp
// ❌ ERRADO
[Fact]
public void Test()
{
    var result = Calculate();
    if (result > 0)
    {
        Assert.True(result % 2 == 0);
    }
    else
    {
        Assert.True(result < 0);
    }
}
```

3. **Múltiplas assertions para diferentes comportamentos**
```csharp
// ❌ ERRADO
[Fact]
public void Test()
{
    var result = Execute();
    Assert.NotNull(result);
    Assert.True(result.IsSuccess);
    Assert.Equal(10, result.Value);
    Assert.Equal("OK", result.Message);
    // Muitas assertions = múltiplos comportamentos testados
}
```

4. **Testes que dependem de ordem de execução**
```csharp
// ❌ ERRADO
[Fact]
[Order(1)]
public void Test1() { }

[Fact]
[Order(2)]
public void Test2() { }
```

5. **Ignorar testes quebrados**
```csharp
// ❌ ERRADO
[Fact(Skip = "Este teste está quebrado")]
public void Test() { }
```

### ✅ Faça Isso

1. **Testes independentes**
```csharp
// ✅ CORRETO
[Fact]
public void Test1() 
{
    var state = "value";
    Assert.Equal("value", state);
}

[Fact]
public void Test2() 
{
    var state = "value";
    Assert.Equal("value", state);
}
```

2. **Testes simples e diretos**
```csharp
// ✅ CORRETO
[Fact]
public void Calculate_WhenPositiveNumber_ShouldReturnEven()
{
    var result = Calculate(10);
    Assert.True(result % 2 == 0);
}
```

3. **Uma assertion por comportamento**
```csharp
// ✅ CORRETO
[Fact]
public void Execute_WhenValid_ShouldReturnSuccess()
{
    var result = Execute();
    Assert.True(result.IsSuccess);
}

[Fact]
public void Execute_WhenValid_ShouldReturnValue()
{
    var result = Execute();
    Assert.Equal(10, result.Value);
}
```

---

## Checklist de Qualidade

Antes de considerar um teste completo, verifique:

- [ ] Teste segue padrão AAA (Arrange, Act, Assert)
- [ ] Nome do teste descreve claramente o cenário e resultado esperado
- [ ] Teste é independente (não depende de outros testes)
- [ ] Teste é determinístico (sempre produz mesmo resultado)
- [ ] Dependências externas são mockadas
- [ ] Casos de sucesso e falha são cobertos
- [ ] Edge cases são testados
- [ ] Teste executa rapidamente (< 100ms para testes unitários)
- [ ] Código do teste é legível e bem organizado
- [ ] Assertions são claras e específicas

---

## Referências

- [xUnit Documentation](https://xunit.net/)
- [Moq Documentation](https://github.com/moq/moq4)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [SpecFlow Documentation](https://specflow.org/)
- [The Art of Unit Testing](https://www.artofunittesting.com/)

---

**Última atualização**: Janeiro 2025

**Nota**: Esta documentação inclui todas as boas práticas validadas para integração com SonarCloud, incluindo configuração de cobertura, consolidação de relatórios, e resolução de problemas comuns.
