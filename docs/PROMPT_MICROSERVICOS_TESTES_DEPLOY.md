# Prompt para Criação de Testes e Actions de Deploy em Microserviços

Este documento contém o prompt completo e todas as lições aprendidas do projeto OrderHub para ser usado na criação de testes e workflows de CI/CD para os próximos microserviços da arquitetura FastFood.

---

## Contexto do Projeto

Você está criando testes e workflows de CI/CD para um novo microserviço .NET 8 seguindo Clean Architecture. O projeto deve seguir os mesmos padrões e lições aprendidas do projeto OrderHub, que alcançou 80%+ de cobertura de testes e integração completa com SonarCloud.

---

## Estrutura do Projeto

O microserviço segue a seguinte estrutura:

```
projeto/
├── src/
│   ├── Core/
│   │   ├── FastFood.{Servico}.Domain/
│   │   ├── FastFood.{Servico}.Application/
│   │   └── FastFood.{Servico}.CrossCutting/
│   ├── Infra/
│   │   ├── FastFood.{Servico}.Infra/
│   │   └── FastFood.{Servico}.Infra.Persistence/
│   └── InterfacesExternas/
│       ├── FastFood.{Servico}.Api/
│       └── FastFood.{Servico}.Migrator/ (se aplicável)
│
├── src/tests/
│   ├── FastFood.{Servico}.Tests.Unit/
│   └── FastFood.{Servico}.Tests.Bdd/
│
├── .github/
│   └── workflows/
│       ├── quality.yml
│       ├── push-to-ecr.yml
│       └── deploy-to-eks.yml
│
└── FastFood.{Servico}.sln
```

**Substitua `{Servico}` pelo nome do microserviço (ex: OrderHub, PayStream, etc.)**

---

## Tarefa 1: Criar Estrutura de Testes

### 1.1 Projeto de Testes Unitários

Criar projeto `FastFood.{Servico}.Tests.Unit` em `src/tests/FastFood.{Servico}.Tests.Unit/` com as seguintes características:

#### Pacotes NuGet Obrigatórios

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
  <PackageReference Include="xunit" Version="2.6.2" />
  <PackageReference Include="xunit.runner.visualstudio" Version="2.5.4">
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    <PrivateAssets>all</PrivateAssets>
  </PackageReference>
  <PackageReference Include="coverlet.collector" Version="6.0.0">
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    <PrivateAssets>all</PrivateAssets>
  </PackageReference>
  <PackageReference Include="coverlet.msbuild" Version="6.0.0">
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    <PrivateAssets>all</PrivateAssets>
  </PackageReference>
  <PackageReference Include="Moq" Version="4.20.70" />
  <PackageReference Include="FluentAssertions" Version="6.12.0" />
</ItemGroup>
```

#### Estrutura de Pastas

A estrutura de testes deve **espelhar** a estrutura do código de produção:

```
FastFood.{Servico}.Tests.Unit/
├── Domain/
│   └── Entities/
├── Application/
│   └── UseCases/
├── Infra/
│   └── Services/
└── InterfacesExternas/
    └── Controllers/
```

#### Padrão de Nomenclatura

```
[ClasseOuMétodoSobTeste]_[Cenário]_[ResultadoEsperado]
```

**Exemplos:**
- `CreateOrder_WhenValidInput_ShouldReturnSuccess`
- `CreateOrder_WhenCustomerNotFound_ShouldThrowNotFoundException`
- `ProcessPayment_WhenInvalidCard_ShouldReturnFailure`

#### Padrão AAA (Arrange, Act, Assert)

Todos os testes devem seguir este padrão:

```csharp
[Fact]
public void CreateOrder_WhenValidInput_ShouldReturnSuccess()
{
    // Arrange
    var customerId = Guid.NewGuid();
    var orderItems = new List<OrderItem> { /* ... */ };
    var useCase = new CreateOrderUseCase(/* dependencies */);
    
    // Act
    var result = await useCase.ExecuteAsync(customerId, orderItems);
    
    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    result.Value.OrderId.Should().NotBeEmpty();
}
```

#### Regras para Testes Unitários

1. **Um teste, uma responsabilidade**: Cada teste verifica apenas um comportamento
2. **Use mocks para dependências externas**: Não use dependências reais (banco, APIs externas)
3. **Teste casos de sucesso e falha**: Cubra caminho feliz e casos de erro
4. **Teste valores limite**: Teste mínimos, máximos e edge cases
5. **Testes independentes**: Cada teste deve poder executar isoladamente
6. **SEMPRE executar testes após criá-los**: Execute `dotnet test` para verificar compilação e execução

### 1.2 Projeto de Testes BDD (Opcional mas Recomendado)

Criar projeto `FastFood.{Servico}.Tests.Bdd` em `src/tests/FastFood.{Servico}.Tests.Bdd/` usando SpecFlow:

#### Pacotes NuGet

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
  <PackageReference Include="SpecFlow" Version="3.9.74" />
  <PackageReference Include="SpecFlow.xUnit" Version="3.9.74" />
  <PackageReference Include="xunit" Version="2.6.2" />
  <PackageReference Include="xunit.runner.visualstudio" Version="2.5.4" />
  <PackageReference Include="coverlet.collector" Version="6.0.0" />
  <PackageReference Include="coverlet.msbuild" Version="6.0.0" />
  <PackageReference Include="FluentAssertions" Version="6.12.0" />
</ItemGroup>
```

#### Estrutura

```
FastFood.{Servico}.Tests.Bdd/
└── Features/
    └── [NomeDoFeature].feature
```

#### Exemplo de Feature

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

### 1.3 Adicionar Projetos à Solução

**CRÍTICO**: Sempre adicionar novos projetos ao arquivo de solução:

```bash
dotnet sln FastFood.{Servico}.sln add src/tests/FastFood.{Servico}.Tests.Unit/FastFood.{Servico}.Tests.Unit.csproj
dotnet sln FastFood.{Servico}.sln add src/tests/FastFood.{Servico}.Tests.Bdd/FastFood.{Servico}.Tests.Bdd.csproj
```

---

## Tarefa 2: Criar Workflow de Qualidade (quality.yml)

Criar arquivo `.github/workflows/quality.yml` com a seguinte estrutura completa:

### 2.1 Estrutura Completa do Workflow

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
          GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
          PR_NUMBER: ${{ github.event.pull_request.number }}
          PR_HEAD_REF: ${{ github.event.pull_request.head.ref }}
          PR_BASE_REF: ${{ github.event.pull_request.base.ref }}
          GITHUB_HEAD_REF: ${{ github.head_ref }}
          GITHUB_BASE_REF: ${{ github.base_ref }}
          GITHUB_REPOSITORY: ${{ github.repository }}
          GITHUB_EVENT_NAME: ${{ github.event_name }}
        run: |
          if [ "$GITHUB_EVENT_NAME" == "pull_request" ]; then
            echo "=== Configuring SonarCloud for Pull Request ==="
            dotnet-sonarscanner begin \
              /k:"{ORGANIZACAO}_{PROJETO}" \
              /o:"{ORGANIZACAO}" \
              /d:sonar.token="$SONAR_TOKEN" \
              /d:sonar.cs.opencover.reportsPaths="TestResults/coverage/coverage.opencover.xml" \
              /d:sonar.coverage.exclusions="**/*Program.cs,**/*Startup.cs,**/Migrations/**,**/*Dto.cs" \
              /d:sonar.qualitygate.wait=true \
              /d:sonar.pullrequest.provider=github \
              /d:sonar.pullrequest.github.repository="$GITHUB_REPOSITORY" \
              /d:sonar.pullrequest.key="$PR_NUMBER" \
              /d:sonar.pullrequest.branch="$PR_HEAD_REF" \
              /d:sonar.pullrequest.base="$PR_BASE_REF"
          else
            echo "=== Configuring SonarCloud for Branch Analysis ==="
            dotnet-sonarscanner begin \
              /k:"{ORGANIZACAO}_{PROJETO}" \
              /o:"{ORGANIZACAO}" \
              /d:sonar.token="$SONAR_TOKEN" \
              /d:sonar.cs.opencover.reportsPaths="TestResults/coverage/coverage.opencover.xml" \
              /d:sonar.coverage.exclusions="**/*Program.cs,**/*Startup.cs,**/Migrations/**,**/*Dto.cs" \
              /d:sonar.qualitygate.wait=true
          fi

      - name: Restore
        run: dotnet restore FastFood.{Servico}.sln

      - name: Build
        run: dotnet build FastFood.{Servico}.sln -c Release --no-restore /p:DebugType=portable /p:DebugSymbols=true

      - name: Test (Unit + BDD) with coverage
        run: |
          mkdir -p TestResults/coverage
          dotnet test FastFood.{Servico}.sln -c Release --no-build \
            --logger "trx;LogFileName=test_results.trx" \
            /p:CollectCoverage=true \
            /p:CoverletOutputFormat="opencover" \
            /p:CoverletOutput="TestResults/coverage/"

      - name: List coverage files
        run: |
          echo "=== Coverage files generated ==="
          find . -name "coverage.opencover.xml" -type f || echo "No coverage files found"
          echo ""
          echo "=== TestResults directory structure ==="
          ls -la TestResults/ || echo "TestResults directory not found"

      - name: Consolidate coverage reports
        run: |
          mkdir -p TestResults/coverage
          COVERAGE_FILES=$(find . -name "coverage.opencover.xml" -type f 2>/dev/null | grep -v ".git" || true)
          echo "=== Found coverage files ==="
          if [ -n "$COVERAGE_FILES" ]; then
            echo "$COVERAGE_FILES"
            COVERAGE_COUNT=$(echo "$COVERAGE_FILES" | wc -l | tr -d ' ')
            echo "Found $COVERAGE_COUNT coverage file(s)"
            
            if [ "$COVERAGE_COUNT" -eq 1 ]; then
              echo "Copying single coverage file..."
              cp "$COVERAGE_FILES" TestResults/coverage/coverage.opencover.xml
            else
              echo "Multiple coverage files found, selecting largest..."
              LARGEST_FILE=""
              LARGEST_SIZE=0
              for file in $COVERAGE_FILES; do
                SIZE=$(stat -c%s "$file" 2>/dev/null || echo "0")
                if [ "$SIZE" -gt "$LARGEST_SIZE" ]; then
                  LARGEST_SIZE=$SIZE
                  LARGEST_FILE="$file"
                fi
              done
              if [ -n "$LARGEST_FILE" ]; then
                echo "Using largest file: $LARGEST_FILE"
                cp "$LARGEST_FILE" TestResults/coverage/coverage.opencover.xml
              else
                FIRST_FILE=$(echo "$COVERAGE_FILES" | head -n 1)
                echo "Using first file as fallback: $FIRST_FILE"
                cp "$FIRST_FILE" TestResults/coverage/coverage.opencover.xml
              fi
            fi
            
            echo "=== Coverage file consolidated ==="
            ls -lh TestResults/coverage/coverage.opencover.xml
          else
            echo "ERROR: No coverage files found to consolidate"
            exit 1
          fi

      - name: Verify coverage file before Sonar End
        run: |
          echo "=== Verifying coverage file ==="
          COVERAGE_PATH="TestResults/coverage/coverage.opencover.xml"
          if [ -f "$COVERAGE_PATH" ]; then
            echo "✓ Coverage file exists at $COVERAGE_PATH"
            ls -lh "$COVERAGE_PATH"
            if grep -q "<CoverageSession>" "$COVERAGE_PATH"; then
              echo "✓ File contains valid OpenCover XML structure"
            else
              echo "✗ File does not contain valid OpenCover XML structure"
              exit 1
            fi
          else
            echo "✗ Coverage file NOT found at $COVERAGE_PATH"
            find . -name "coverage.opencover.xml" -type f
            exit 1
          fi

      - name: Check Coverage Threshold (80%)
        run: |
          echo "=== Checking Coverage Threshold ==="
          COVERAGE_PATH="TestResults/coverage/coverage.opencover.xml"
          MIN_COVERAGE=80
          
          if [ ! -f "$COVERAGE_PATH" ]; then
            echo "✗ Coverage file not found at $COVERAGE_PATH"
            exit 1
          fi
          
          python3 << EOF
          import xml.etree.ElementTree as ET
          import sys
          
          try:
              tree = ET.parse('TestResults/coverage/coverage.opencover.xml')
              root = tree.getroot()
              
              total_sequence_points = 0
              visited_sequence_points = 0
              
              for summary in root.findall('.//Summary'):
                  num_seq = int(summary.get('numSequencePoints', 0))
                  visited_seq = int(summary.get('visitedSequencePoints', 0))
                  total_sequence_points += num_seq
                  visited_sequence_points += visited_seq
              
              if total_sequence_points == 0:
                  print("⚠ No sequence points found in coverage report")
                  sys.exit(1)
              
              coverage_percentage = (visited_sequence_points / total_sequence_points) * 100
              min_coverage = float(${MIN_COVERAGE})
              
              print("")
              print("==========================================")
              print("📊 CODE COVERAGE REPORT")
              print("==========================================")
              print(f"Total sequence points: {total_sequence_points:,}")
              print(f"Visited sequence points: {visited_sequence_points:,}")
              print(f"Coverage: {coverage_percentage:.2f}%")
              print(f"Minimum required: {min_coverage:.0f}%")
              print("==========================================")
              print("")
              
              if coverage_percentage < min_coverage:
                  print(f"❌ FAIL: Coverage {coverage_percentage:.2f}% is below minimum {min_coverage:.0f}%")
                  sys.exit(1)
              else:
                  print(f"✅ PASS: Coverage {coverage_percentage:.2f}% meets minimum {min_coverage:.0f}%")
                  sys.exit(0)
                  
          except Exception as e:
              print(f"✗ Error parsing coverage file: {e}")
              import traceback
              traceback.print_exc()
              sys.exit(1)
          EOF
          
          EXIT_CODE=$?
          if [ $EXIT_CODE -ne 0 ]; then
            echo ""
            echo "=========================================="
            echo "❌ COVERAGE CHECK FAILED"
            echo "=========================================="
            echo "The code coverage is below the minimum threshold of ${MIN_COVERAGE}%"
            echo "Please add more tests to increase coverage before merging."
            echo "=========================================="
            exit 1
          fi

      - name: Sonar - End
        env:
          SONAR_TOKEN: ${{ secrets.SONAR_TOKEN }}
        run: dotnet-sonarscanner end /d:sonar.token="$SONAR_TOKEN"
```

### 2.2 Configurações Críticas

#### Substituir Placeholders

- `{ORGANIZACAO}`: Nome da organização no SonarCloud (ex: `diegoknsk`)
- `{PROJETO}`: Nome do projeto no SonarCloud (ex: `fiap-fase4-orderhub-api`)
- `FastFood.{Servico}.sln`: Nome do arquivo de solução

#### Configurações Importantes

1. **Build com símbolos de debug**: `/p:DebugType=portable /p:DebugSymbols=true` (CRÍTICO para cobertura)
2. **Formato OpenCover**: `/p:CoverletOutputFormat="opencover"` (único formato suportado pelo SonarCloud)
3. **Exclusões de cobertura**: `**/*Program.cs,**/*Startup.cs,**/Migrations/**,**/*Dto.cs`
4. **Quality Gate wait**: `/d:sonar.qualitygate.wait=true` (bloqueia se Quality Gate falhar)

### 2.3 Configuração do SonarCloud

#### ⚠️ CRÍTICO: Desabilitar Análise Automática

**Erro comum**: `ERROR: You are running CI analysis while Automatic Analysis is enabled.`

**Solução**: 
1. Acesse https://sonarcloud.io
2. Navegue até o projeto
3. Vá em **Administration** → **Analysis Method**
4. Desative **Automatic Analysis**
5. Salve as alterações

**Por quê?**: Análise automática e CI/CD são mutuamente exclusivas.

#### Configurar Secrets no GitHub

1. Acesse Settings → Secrets and variables → Actions
2. Adicione secret `SONAR_TOKEN` com o token do SonarCloud

#### Configurar Quality Gate

1. Acesse SonarCloud → Quality Gates
2. Configure cobertura mínima de 80%
3. Configure bloqueio para erros graves de Security, Reliability e Maintainability

---

## Tarefa 3: Criar Workflow de Push para ECR (push-to-ecr.yml)

Criar arquivo `.github/workflows/push-to-ecr.yml`:

```yaml
name: Push to ECR

on:
  workflow_dispatch:

env:
  AWS_REGION: us-east-1
  ECR_REPOSITORY: fiap-fase4-infra-{servico-lowercase}

jobs:
  build-and-push:
    runs-on: ubuntu-latest
    permissions:
      contents: read

    steps:
      - name: Checkout code
        uses: actions/checkout@b4ffde65f46336ab88eb53be808477a3936bae11 # v4

      - name: Configure AWS credentials
        uses: aws-actions/configure-aws-credentials@ff717079ee2060e4bcee96c4779b553acc87447c # v4
        with:
          aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}
          aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
          aws-session-token: ${{ secrets.AWS_SESSION_TOKEN }}
          aws-region: ${{ env.AWS_REGION }}

      - name: Login to Amazon ECR
        uses: aws-actions/amazon-ecr-login@062b18b96a7aff071d4dc91bc00c4c1a7945b076 # v2
        id: ecr-login

      - name: Set up Docker Buildx
        uses: docker/setup-buildx-action@8d2750c68a42422c14e847fe6c8ac0403b4cbd6f # v3

      - name: Generate image tag
        id: image-tag
        run: |
          IMAGE_TAG=$(git rev-parse --short HEAD)
          echo "tag=$IMAGE_TAG" >> $GITHUB_OUTPUT
          echo "Image tag: $IMAGE_TAG"

      - name: Build and push API image
        uses: docker/build-push-action@ca052bb54ab0790a636c9b5f226502c73d547a25 # v5
        with:
          context: .
          file: ./src/InterfacesExternas/FastFood.{Servico}.Api/Dockerfile
          push: true
          tags: |
            ${{ steps.ecr-login.outputs.registry }}/${{ env.ECR_REPOSITORY }}:api-${{ steps.image-tag.outputs.tag }}
            ${{ steps.ecr-login.outputs.registry }}/${{ env.ECR_REPOSITORY }}:api-latest
          cache-from: type=registry,ref=${{ steps.ecr-login.outputs.registry }}/${{ env.ECR_REPOSITORY }}:api-latest
          cache-to: type=inline

      - name: Build and push Migrator image
        if: ${{ true }}  # Ajustar se não houver migrator
        uses: docker/build-push-action@ca052bb54ab0790a636c9b5f226502c73d547a25 # v5
        with:
          context: .
          file: ./src/InterfacesExternas/FastFood.{Servico}.Migrator/Dockerfile
          push: true
          tags: |
            ${{ steps.ecr-login.outputs.registry }}/${{ env.ECR_REPOSITORY }}:migrator-${{ steps.image-tag.outputs.tag }}
            ${{ steps.ecr-login.outputs.registry }}/${{ env.ECR_REPOSITORY }}:migrator-latest
          cache-from: type=registry,ref=${{ steps.ecr-login.outputs.registry }}/${{ env.ECR_REPOSITORY }}:migrator-latest
          cache-to: type=inline

      - name: Validate images in ECR
        run: |
          echo "Validating images in ECR..."
          aws ecr describe-images \
            --repository-name ${{ env.ECR_REPOSITORY }} \
            --image-ids imageTag=api-${{ steps.image-tag.outputs.tag }} \
            --region ${{ env.AWS_REGION }}
          echo "Images validated successfully!"

      - name: Output image URLs
        run: |
          echo "API Image: ${{ steps.ecr-login.outputs.registry }}/${{ env.ECR_REPOSITORY }}:api-${{ steps.image-tag.outputs.tag }}"
```

**Substituir**:
- `{servico-lowercase}`: Nome do serviço em minúsculas (ex: `orderhub-api`)
- `FastFood.{Servico}.Api`: Nome do projeto da API
- Ajustar step do Migrator se não existir

---

## Tarefa 4: Criar Workflow de Deploy para EKS (deploy-to-eks.yml)

Criar arquivo `.github/workflows/deploy-to-eks.yml`:

```yaml
name: Deploy {Servico} Application

on:
  workflow_run:
    workflows: ["PR - Build, Test, Sonar"]
    types:
      - completed
    branches:
      - main
  workflow_dispatch:
    inputs:
      skip_migrator:
        description: 'Pular execução do Migrator'
        required: false
        type: boolean
        default: false

env:
  AWS_REGION: us-east-1
  EKS_CLUSTER_NAME: eks-fiap-fase4-infra
  KUBERNETES_NAMESPACE: {servico-lowercase}
  DEPLOYMENT_NAME: {servico-lowercase}-api
  CONTAINER_NAME: api
  ECR_REPOSITORY: fiap-fase4-infra-{servico-lowercase}

jobs:
  check-quality:
    runs-on: ubuntu-latest
    if: ${{ github.event.workflow_run.conclusion == 'success' }}
    steps:
      - name: Check Quality Gate
        run: |
          echo "✅ Quality Gate passed - proceeding with deployment"
          echo "Workflow run: ${{ github.event.workflow_run.id }}"
          echo "Conclusion: ${{ github.event.workflow_run.conclusion }}"

  deploy:
    needs: check-quality
    if: ${{ (github.event_name == 'workflow_run' && github.event.workflow_run.head_repository.full_name == github.repository && github.event.workflow_run.head_branch == 'main' && github.event.workflow_run.conclusion == 'success') || (github.event_name == 'workflow_dispatch' && github.repository_owner != '') }}
    runs-on: ubuntu-latest
    
    outputs:
      tag: ${{ steps.set-tag.outputs.tag }}
      api-image: ${{ steps.set-tag.outputs.api-image }}
      migrator-image: ${{ steps.set-tag.outputs.migrator-image }}
    
    steps:
    - name: Checkout code
      uses: actions/checkout@b4ffde65f46336ab88eb53be808477a3936bae11 # v4
      with:
        ref: 'main'
    
    - name: Configure AWS credentials
      uses: aws-actions/configure-aws-credentials@ff717079ee2060e4bcee96c4779b553acc87447c # v4
      with:
        aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}
        aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
        aws-session-token: ${{ secrets.AWS_SESSION_TOKEN }}
        aws-region: ${{ env.AWS_REGION }}
    
    - name: Login to Amazon ECR
      id: login-ecr
      uses: aws-actions/amazon-ecr-login@062b18b96a7aff071d4dc91bc00c4c1a7945b076 # v2
    
    - name: Set TAG and image names
      id: set-tag
      run: |
        TAG=$(git rev-parse --short HEAD)
        API_IMAGE="${{ steps.login-ecr.outputs.registry }}/${{ env.ECR_REPOSITORY }}:api-${TAG}"
        MIGRATOR_IMAGE="${{ steps.login-ecr.outputs.registry }}/${{ env.ECR_REPOSITORY }}:migrator-${TAG}"
        
        echo "tag=${TAG}" >> $GITHUB_OUTPUT
        echo "api-image=${API_IMAGE}" >> $GITHUB_OUTPUT
        echo "migrator-image=${MIGRATOR_IMAGE}" >> $GITHUB_OUTPUT
        
        echo "TAG: ${TAG}"
        echo "API Image: ${API_IMAGE}"
        echo "Migrator Image: ${MIGRATOR_IMAGE}"
    
    - name: Set up Docker Buildx
      uses: docker/setup-buildx-action@8d2750c68a42422c14e847fe6c8ac0403b4cbd6f # v3
    
    - name: Build and push API image
      uses: docker/build-push-action@ca052bb54ab0790a636c9b5f226502c73d547a25 # v5
      with:
        context: .
        file: ./src/InterfacesExternas/FastFood.{Servico}.Api/Dockerfile
        push: true
        tags: |
          ${{ steps.set-tag.outputs.api-image }}
          ${{ steps.login-ecr.outputs.registry }}/${{ env.ECR_REPOSITORY }}:api-latest
        cache-from: type=registry,ref=${{ steps.login-ecr.outputs.registry }}/${{ env.ECR_REPOSITORY }}:api-latest
        cache-to: type=inline
    
    - name: Build and push Migrator image
      if: ${{ !github.event.inputs.skip_migrator }}
      uses: docker/build-push-action@ca052bb54ab0790a636c9b5f226502c73d547a25 # v5
      with:
        context: .
        file: ./src/InterfacesExternas/FastFood.{Servico}.Migrator/Dockerfile
        push: true
        tags: |
          ${{ steps.set-tag.outputs.migrator-image }}
          ${{ steps.login-ecr.outputs.registry }}/${{ env.ECR_REPOSITORY }}:migrator-latest
        cache-from: type=registry,ref=${{ steps.login-ecr.outputs.registry }}/${{ env.ECR_REPOSITORY }}:migrator-latest
        cache-to: type=inline
    
    - name: Configure kubectl
      run: |
        aws eks update-kubeconfig --region ${{ env.AWS_REGION }} --name ${{ env.EKS_CLUSTER_NAME }}
        kubectl version --client
    
    - name: Update API deployment
      run: |
        echo "Updating ${DEPLOYMENT_NAME} deployment with new image..."
        kubectl set image deployment/${DEPLOYMENT_NAME} ${CONTAINER_NAME}=${{ steps.set-tag.outputs.api-image }} -n ${{ env.KUBERNETES_NAMESPACE }}
        
        echo "Waiting for rollout to complete..."
        kubectl rollout status deployment/${DEPLOYMENT_NAME} -n ${{ env.KUBERNETES_NAMESPACE }} --timeout=300s
        
        echo "API deployment updated successfully"
    
    - name: Recreate Migrator job
      if: ${{ !github.event.inputs.skip_migrator }}
      run: |
        echo "Deleting existing migrator job if it exists..."
        kubectl delete job {servico-lowercase}-migrator -n ${{ env.KUBERNETES_NAMESPACE }} --ignore-not-found=true
        
        echo "Waiting for job to be fully deleted..."
        sleep 10
        
        echo "Creating new migrator job..."
        cat <<EOF | kubectl apply -f -
        apiVersion: batch/v1
        kind: Job
        metadata:
          name: {servico-lowercase}-migrator
          namespace: ${{ env.KUBERNETES_NAMESPACE }}
          labels:
            app: {servico-lowercase}-migrator
            service: {servico-lowercase}
        spec:
          completions: 1
          parallelism: 1
          backoffLimit: 3
          template:
            metadata:
              labels:
                app: {servico-lowercase}-migrator
                service: {servico-lowercase}
            spec:
              restartPolicy: Never
              containers:
              - name: migrator
                image: ${{ steps.set-tag.outputs.migrator-image }}
                imagePullPolicy: Always
                resources:
                  requests:
                    cpu: "100m"
                    memory: "256Mi"
                  limits:
                    cpu: "500m"
                    memory: "512Mi"
                terminationGracePeriodSeconds: 30
                envFrom:
                - configMapRef:
                    name: {servico-lowercase}-config
                - secretRef:
                    name: {servico-lowercase}-secrets
                command: ["dotnet", "FastFood.{Servico}.Migrator.dll"]
        EOF
        
        echo "Waiting for migrator job to complete..."
        kubectl wait --for=condition=complete job/{servico-lowercase}-migrator -n ${{ env.KUBERNETES_NAMESPACE }} --timeout=600s
        
        echo "Migrator job completed successfully"
    
    - name: Verify deployment
      run: |
        echo "Verifying deployment..."
        kubectl get deployment ${DEPLOYMENT_NAME} -n ${{ env.KUBERNETES_NAMESPACE }}
        kubectl get pods -l app={servico-lowercase}-api -n ${{ env.KUBERNETES_NAMESPACE }}
        if [ "${{ github.event.inputs.skip_migrator }}" != "true" ]; then
          kubectl get job {servico-lowercase}-migrator -n ${{ env.KUBERNETES_NAMESPACE }}
          kubectl get pods -l app={servico-lowercase}-migrator -n ${{ env.KUBERNETES_NAMESPACE }}
        fi
        echo "Deployment verified successfully!"
```

**Substituir**:
- `{Servico}`: Nome do serviço (ex: `OrderHub`)
- `{servico-lowercase}`: Nome do serviço em minúsculas (ex: `orderhub`)
- Ajustar ConfigMaps e Secrets conforme necessário

---

## Lições Aprendidas Críticas

### 1. Cobertura de Testes

- **Meta obrigatória: 80% de cobertura**
- **Build com símbolos de debug é CRÍTICO**: Sem `/p:DebugType=portable /p:DebugSymbols=true`, a cobertura não será processada corretamente
- **Formato OpenCover**: Único formato suportado pelo SonarCloud
- **Consolidação de arquivos**: Coverlet gera arquivos em múltiplos locais, é necessário consolidar em um único arquivo
- **Verificação antes do Sonar End**: Sempre verificar se o arquivo de cobertura existe e é válido

### 2. SonarCloud

- **Desabilitar Análise Automática**: OBRIGATÓRIO para evitar conflitos com CI/CD
- **Quality Gate Wait**: Usar `/d:sonar.qualitygate.wait=true` para bloquear merges quando Quality Gate falhar
- **Exclusões de cobertura**: Excluir Program.cs, Startup.cs, Migrations, DTOs
- **Pull Request Analysis**: Configurar corretamente para análise de PRs

### 3. Workflows GitHub Actions

- **Usar commit SHA para actions**: Não usar tags, sempre usar versões fixas (SHA do commit)
- **Cache do Sonar**: Usar cache para otimizar execução
- **Segurança**: Sempre fazer checkout de `main` no deploy para evitar execução de código não confiável
- **Dependências entre workflows**: Deploy só executa após Quality Gate passar

### 4. Estrutura de Testes

- **Espelhar estrutura de produção**: Facilita manutenção e localização de testes
- **Padrão AAA**: Sempre usar Arrange, Act, Assert
- **Nomenclatura descritiva**: Nome do teste deve descrever claramente o cenário
- **Testes independentes**: Cada teste deve poder executar isoladamente
- **SEMPRE executar testes**: Após criar testes, sempre executar `dotnet test` para verificar

### 5. Docker e ECR

- **Cache de imagens**: Usar cache do ECR para acelerar builds
- **Tags**: Usar commit SHA para tags de imagens
- **Validação**: Sempre validar que imagens foram criadas no ECR

### 6. Kubernetes

- **Rollout status**: Sempre aguardar rollout completar antes de considerar deploy bem-sucedido
- **Migrator jobs**: Deletar job anterior antes de criar novo
- **Timeouts**: Configurar timeouts apropriados para operações

---

## Checklist Final

Antes de considerar a implementação completa, verificar:

### Testes
- [ ] Projeto de testes unitários criado com pacotes corretos
- [ ] Projeto de testes BDD criado (se aplicável)
- [ ] Projetos adicionados à solução
- [ ] Estrutura de pastas espelha código de produção
- [ ] Testes seguem padrão AAA
- [ ] Nomenclatura descritiva nos testes
- [ ] `dotnet test` executa sem erros
- [ ] Cobertura mínima de 80% alcançada

### Workflow de Qualidade
- [ ] Arquivo `quality.yml` criado
- [ ] Placeholders substituídos (organização, projeto, solução)
- [ ] Build com símbolos de debug configurado
- [ ] Testes com cobertura em formato OpenCover
- [ ] Consolidação de arquivos de cobertura configurada
- [ ] Verificação de arquivo de cobertura antes do Sonar End
- [ ] Verificação de threshold de 80% de cobertura
- [ ] Secret `SONAR_TOKEN` configurado no GitHub

### SonarCloud
- [ ] Projeto criado no SonarCloud
- [ ] Análise Automática desabilitada
- [ ] Quality Gate configurado com cobertura mínima de 80%
- [ ] Token gerado e configurado como secret no GitHub

### Workflow de Push ECR
- [ ] Arquivo `push-to-ecr.yml` criado
- [ ] Placeholders substituídos
- [ ] ECR repository criado na AWS
- [ ] Secrets AWS configurados no GitHub

### Workflow de Deploy EKS
- [ ] Arquivo `deploy-to-eks.yml` criado
- [ ] Placeholders substituídos
- [ ] Dependência do workflow de qualidade configurada
- [ ] Namespace Kubernetes criado
- [ ] ConfigMaps e Secrets criados no Kubernetes
- [ ] Deployment criado no Kubernetes

### Validação Final
- [ ] Workflow de qualidade executa em PR
- [ ] Cobertura aparece no SonarCloud
- [ ] Quality Gate bloqueia merges quando necessário
- [ ] Deploy executa automaticamente após merge na main
- [ ] Imagens são criadas e enviadas para ECR
- [ ] Deploy no EKS funciona corretamente
- [ ] Migrator job executa (se aplicável)

---

## Referências

- [Regras de Escrita de Testes](./rules/TEST_WRITING_RULES.md)
- [Regras de Arquitetura](./rules/ARCHITECTURE_RULES.md)
- [Contexto do Projeto](./rules/orderhub-context.mdc)

---

**Última atualização**: Janeiro 2025

**Baseado em**: Lições aprendidas do projeto OrderHub que alcançou 80%+ de cobertura de testes e integração completa com SonarCloud e EKS.
