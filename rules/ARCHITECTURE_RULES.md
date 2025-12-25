# Regras de Arquitetura - FastFood Auth Lambda

## Objetivo

Serviço de autenticação e identificação, publicado como **AWS Lambda**, mas estruturado como uma **API ASP.NET Core tradicional**, utilizando `Amazon.Lambda.AspNetCoreServer` para hospedar a aplicação.

A arquitetura segue **~80% Clean Architecture** (simplificação pragmática mantendo separação de camadas e inversão de dependência), com foco em mercado, simplicidade e consistência com os demais microsserviços do sistema (OrderHub, PayStream, KitchenFlow).

---

## Hosting (Lambda como API)

- O serviço deve usar ASP.NET Core normalmente.
- O hosting Lambda deve ser configurado com:

```csharp
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);
```

- **Em ambiente local:**
  - A aplicação roda com Kestrel normalmente.

- **Em produção:**
  - Kestrel é substituído pelo runtime do Lambda automaticamente.

- **Regra:** a aplicação não deve conter código específico de Lambda fora do bootstrap.

---

## Swagger

- Swagger é **obrigatório** e deve estar habilitado:
  - Em ambiente local
  - Em ambiente Lambda (quando permitido pelo API Gateway)
- Endpoints devem ser documentados normalmente com XML comments.
- Swagger é obrigatório para padronização e inspeção de contrato.

---

## Estrutura de Diretórios do Projeto

A estrutura do projeto segue uma organização clara separando camadas core, interfaces externas, testes e documentação:

```
projeto/
├── src/
│   ├── Core/                          # Camadas que utilizamos (lógica de negócio)
│   │   ├── FastFood.Auth.Application  # UseCases, Ports, InputModels, OutputModels, Presenters
│   │   ├── FastFood.Auth.Domain       # Entidades, VOs, validações, invariantes
│   │   ├── FastFood.Auth.Infra        # Serviços externos: Cognito, Token, etc.
│   │   └── FastFood.Auth.Infra.Persistence  # PostgreSQL com EF Core
│   │
│   └── InterfacesExternas/           # Camadas que usamos (adapters de entrada/saída)
│       ├── FastFood.Auth.Lambda.Admin      # Lambda Admin (ASP.NET Core - API/Lambda host)
│       ├── FastFood.Auth.Lambda.Customer  # Lambda Customer (ASP.NET Core - API/Lambda host)
│       └── FastFood.Auth.Migrator         # Console app para executar migrations
│
├── src/tests/                         # Testes automatizados
│   ├── FastFood.Auth.Tests.Unit      # Testes unitários
│   └── FastFood.Auth.Tests.Bdd       # Testes BDD/SpecFlow
│
└── story/                             # Documentação de histórias técnicas
    └── Storie-XX-Descricao/
        ├── story.md
        └── subtask/
```

### Regras de Organização

- **Core**: Contém toda a lógica de negócio e regras de domínio. Estas são as camadas que o sistema utiliza para processar informações.
- **InterfacesExternas**: Contém adapters de entrada/saída (APIs, Lambdas, Console Apps). Estas são as camadas que o sistema usa para se comunicar com o mundo externo.
- **tests**: Todos os testes automatizados (unitários, integração, BDD).
- **story**: Documentação técnica das histórias de desenvolvimento.

## Estrutura de Projetos

```
FastFood.Auth.Lambda.Admin      (ASP.NET Core - API/Lambda host para Admin)
FastFood.Auth.Lambda.Customer   (ASP.NET Core - API/Lambda host para Customer)
FastFood.Auth.Application       (UseCases, Ports, InputModels, OutputModels, Presenters)
FastFood.Auth.Domain            (Entidades, VOs, validações, invariantes)
FastFood.Auth.Infra             (Serviços externos: Cognito, Token, etc.)
FastFood.Auth.Infra.Persistence (PostgreSQL com EF Core)
FastFood.Auth.Migrator          (Console app para executar migrations)
FastFood.Auth.Tests.Unit        (Testes unitários)
FastFood.Auth.Tests.Bdd         (Testes BDD/SpecFlow)
```

---

## Regras de Dependência

```
Lambda → Application
Application → Domain
Infra.* → Application (implementa Ports)
Domain → Nenhuma dependência externa
```

**Domain NÃO referencia:**
- ASP.NET
- EF Core
- AWS SDK
- HttpClient
- Configuration
- Qualquer framework ou biblioteca externa

---

## Estrutura da Camada Application (Organização Horizontal por Contexto)

A camada Application deve ser organizada **horizontalmente por contexto** (Customer, Admin, etc.), não verticalmente por tipo:

```
FastFood.Auth.Application/
  UseCases/
    Customer/
      CreateAnonymousCustomerUseCase.cs
      RegisterCustomerUseCase.cs
      IdentifyCustomerUseCase.cs
    Admin/
      AuthenticateAdminUseCase.cs
  InputModels/
    Customer/
      CreateAnonymousCustomerInputModel.cs
      RegisterCustomerInputModel.cs
      IdentifyCustomerInputModel.cs
    Admin/
      AuthenticateAdminInputModel.cs
  OutputModels/
    Customer/
      CreateAnonymousCustomerOutputModel.cs
      RegisterCustomerOutputModel.cs
      IdentifyCustomerOutputModel.cs
    Admin/
      AuthenticateAdminOutputModel.cs
  Presenters/
    Customer/
      CreateAnonymousCustomerPresenter.cs
      RegisterCustomerPresenter.cs
      IdentifyCustomerPresenter.cs
    Admin/
      AuthenticateAdminPresenter.cs
  Ports/
    ICustomerRepository.cs
    ITokenService.cs
    ICognitoService.cs
    IMessageBus.cs (se necessário)
```

### Regras de Nomenclatura

- **UseCase**: `<Verbo><Entidade>UseCase` (ex: `CreateAnonymousCustomerUseCase`)
- **InputModel**: `<UseCaseName>InputModel` (ex: `CreateAnonymousCustomerInputModel`)
- **OutputModel**: `<UseCaseName>OutputModel` (ex: `CreateAnonymousCustomerOutputModel`)
- **Presenter**: `<UseCaseName>Presenter` (ex: `CreateAnonymousCustomerPresenter`)
- **Port**: `I<ServiceName>` (ex: `ICustomerRepository`, `ITokenService`)

---

## Fluxo Padrão (Request → Response)

```
Controller (Lambda)
  ↓ recebe RequestModel
  ↓ mapeia para InputModel
  ↓ chama UseCase.ExecuteAsync(InputModel)
UseCase (Application)
  ↓ executa lógica de negócio
  ↓ chama Ports (repositórios, serviços)
  ↓ obtém OutputModel
  ↓ chama Presenter.Present(OutputModel)
Presenter (Application)
  ↓ recebe OutputModel
  ↓ transforma em ResponseModel (da API)
  ↓ retorna ResponseModel
UseCase (Application)
  ↓ retorna ResponseModel para o Controller
Controller (Lambda)
  ↓ recebe ResponseModel do UseCase
  ↓ retorna HTTP Response
```

### Regras Críticas do Fluxo

1. **Controller NÃO acessa DbContext direto** - sempre via UseCase
2. **UseCase NÃO recebe RequestModel da API** - recebe InputModel da Application
3. **UseCase é responsável por chamar o Presenter** - não é responsabilidade do Controller
4. **UseCase retorna ResponseModel** - já transformado pelo Presenter
5. **Presenter transforma OutputModel em ResponseModel** - adaptação quando necessário
6. **Controller apenas recebe ResponseModel e retorna HTTP** - não chama Presenter diretamente

---

## API (Controllers / Lambda)

Controllers são **adapters de transporte** (HTTP/API Gateway).

### Responsabilidades dos Controllers

- Autenticação/autorização HTTP
- Validação básica de request (ModelState)
- Mapear `RequestModel` → `InputModel`
- Chamar `UseCase.ExecuteAsync(InputModel)`
- Receber `ResponseModel` do UseCase
- Retornar HTTP Response

### Proibido nos Controllers

- ❌ Regra de negócio
- ❌ Acesso direto a banco (DbContext) ou SDKs
- ❌ Chamar Presenter diretamente (responsabilidade do UseCase)
- ❌ Criar ResponseModels diretamente (deve usar Presenters via UseCase)
- ❌ Criar Presenters próprios (deve usar Presenters da Application)
- ❌ Lógica de transformação complexa (deve estar no Presenter)

### Endpoints Customer

- `POST /api/customer/anonymous` - Criar customer anônimo
- `POST /api/customer/register` - Registrar customer por CPF
- `POST /api/customer/identify` - Identificar customer existente por CPF

### Endpoints Admin

- `POST /api/admin/login` - Autenticar admin via AWS Cognito

---

## Application Layer

### UseCases

- UseCases são **pequenos e focados** (uma única responsabilidade)
- Cada UseCase deve ter um objetivo claro e não deve orquestrar múltiplos fluxos complexos
- UseCases recebem **InputModels** da Application (não RequestModels da API)
- UseCases executam lógica de negócio e chamam **Ports** (interfaces) para acesso a dados/serviços externos
- UseCases obtêm **OutputModels** da execução
- **UseCases são responsáveis por chamar o Presenter** para transformar OutputModel em ResponseModel
- UseCases retornam **ResponseModels** (já transformados pelo Presenter)

### InputModels

- InputModels definem o contrato de **entrada** dos UseCases
- Devem estar na camada Application
- Representam os dados necessários para executar o UseCase
- Podem depender de tipos do Domain (Value Objects, Enums)

### OutputModels

- OutputModels definem o contrato de **saída** dos UseCases
- Devem estar na camada Application
- Representam os dados retornados pelo UseCase
- Podem depender de tipos do Domain (Value Objects, Enums)

### Presenters

- Presenters são **obrigatórios** e devem estar na camada Application
- **São chamados pelo UseCase** (não pelo Controller)
- Responsabilidade: transformar `OutputModel` em `ResponseModel` (da API)
- Por padrão, fazem mapeamento direto, mas podem fazer transformações quando necessário
- API consome os OutputModels através dos Presenters, mas a chamada é feita pelo UseCase

### Ports (Interfaces)

- Ports definem contratos para acesso a dados e serviços externos
- Exemplos:
  - `ICustomerRepository` - acesso a dados de Customer
  - `ITokenService` - geração/validação de tokens JWT
  - `ICognitoService` - autenticação via AWS Cognito
  - `IMessageBus` / `IEventPublisher` - publicação de eventos (se necessário)
- Implementações concretas ficam na camada Infra

### Facade (Opcional)

- **Facade NÃO é obrigatório**
- Use Facade **apenas** quando:
  - Um endpoint precisa orquestrar **3 ou mais UseCases**
  - Há lógica de orquestração complexa entre múltiplos UseCases
- Se um endpoint chama apenas 1-2 UseCases, não use Facade
- Facade deve estar na camada Application

---

## Domain Layer

- Entidades, Value Objects, Enums e validações de domínio
- Exceções de domínio para regras inválidas
- **Nenhuma dependência externa** (zero dependências de framework)

### Entidade Customer

```csharp
- Id (Guid)
- Name (string?, nullable)
- Email (Email?, Value Object, nullable)
- Cpf (Cpf?, Value Object, nullable)
- CustomerType (CustomerTypeEnum)
- CreatedAt (DateTime)
```

### CustomerType Enum

```csharp
- Registered = 1
- Anonymous = 2
```

### Value Objects

- **Cpf**: validação de CPF brasileiro (11 dígitos, algoritmo de validação)
- **Email**: validação de formato de email

---

## Infraestrutura

### Infra (Serviços Externos)

- Implementa Ports de serviços externos
- Exemplos:
  - `TokenService` (implementa `ITokenService`) - geração JWT
  - `CognitoService` (implementa `ICognitoService`) - autenticação Cognito

### Infra.Persistence

- Implementa Ports de persistência (ex: `ICustomerRepository`)
- Usa Entity Framework Core com PostgreSQL (Npgsql)
- Entidades de banco são **separadas** das entidades de domínio
- **DbContext nunca é acessado fora da Infra.Persistence**

### Persistência - Tabela Customers

```sql
- Id (Guid, PK)
- Name (varchar 500, nullable)
- Email (varchar 255, nullable)
- Cpf (varchar 11, nullable)
- CustomerType (int) - 1 = Registered, 2 = Anonymous
- CreatedAt (datetime)
```

- Value Objects (Cpf, Email) são mapeados como strings no banco

---

## Autenticação e Tokens

### Customer Authentication

- Tokens JWT gerados para customers (identificados ou anônimos)
- Claims obrigatórias:
  - `sub`: CustomerId (Guid)
  - `customerId`: CustomerId (Guid)
  - `jti`: JWT ID (Guid)
  - `iat`: Issued At (Unix timestamp)
- Configuração via `appsettings.json`:
  - `JwtSettings:Secret`
  - `JwtSettings:Issuer`
  - `JwtSettings:Audience`
  - `JwtSettings:ExpirationHours`

### Admin Authentication

- Autenticação via AWS Cognito
- Usar `AdminInitiateAuthRequest` com `AuthFlowType.ADMIN_USER_PASSWORD_AUTH`
- Configuração via variáveis de ambiente:
  - `COGNITO__REGION`
  - `COGNITO__USERPOOLID`
  - `COGNITO__CLIENTID`
- Retornar `AccessToken`, `IdToken`, `ExpiresIn`, `TokenType`
- Port `ICognitoService` na Application
- Implementação concreta na Infra usando `AWSSDK.CognitoIdentityProvider`

---

## Testes e Qualidade

### Estrutura de Testes

- **FastFood.Auth.Tests.Unit**: Testes unitários
- **FastFood.Auth.Tests.Bdd**: Testes BDD (SpecFlow ou estilo BDD com xUnit)

### Testes Unitários

- Testes para:
  - Domain (regras e invariantes)
  - UseCases (com mocks dos Ports)
  - Value Objects (validações)
- Usar xUnit, Moq ou NSubstitute para mocks

### Testes BDD

- Pelo menos **1 cenário BDD por serviço**
- Exemplo: "Dado CPF válido, quando identificar, então retorna token"
- Usar SpecFlow ou estilo BDD com xUnit

### Cobertura e Qualidade

- **Cobertura mínima alvo: >= 80%** (cobertura de linha)
- **Sonar Quality Gate**: deve passar sem code smells críticos e vulnerabilidades bloqueantes
- Testes devem ser executados no CI/CD

---

## Convenções de Código

### .NET e C#

- Versão: **.NET 8** para todos os projetos
- Seguir convenções do C# Coding Conventions (Microsoft)
- Usar PascalCase para classes, métodos e propriedades públicas
- Usar camelCase para variáveis locais e campos privados
- Prefixar interfaces com "I" (ex: `ICustomerRepository`)

### Dependency Injection

- DI configurada no bootstrap (Program.cs do Lambda)
- Injetar UseCases, Presenters e implementações dos Ports
- Nada de instanciar dependências fora do bootstrap

### Gerenciamento de Solução

- **SEMPRE adicionar novos projetos ao arquivo de solução após criá-los**
- Executar: `dotnet sln <arquivo-solucao> add <caminho-projeto>`
- Projetos devem estar na raiz da solução (não em pastas virtuais)
- Manter estrutura de diretórios físicos (src/Core/, src/InterfacesExternas/, src/tests/, story/)

---

## Containerização e Docker

### Dockerfiles

Todos os Dockerfiles devem seguir as seguintes regras de segurança e boas práticas:

#### Multi-Stage Build (Obrigatório)

- **Stage 1 (build)**: Usar `mcr.microsoft.com/dotnet/sdk:8.0` para compilar
- **Stage 2 (runtime)**: Usar `public.ecr.aws/lambda/dotnet:8` para runtime Lambda
- Otimiza o tamanho da imagem final (apenas runtime, sem SDK)

#### Segurança (Correções SonarQube)

- **USER não-root (OBRIGATÓRIO)**: Sempre executar como usuário não-root
  ```dockerfile
  # Rodar como não-root sem depender de groupadd/useradd.
  # 1001:1001 é um UID/GID arbitrário não-root (não precisa existir no /etc/passwd).
  RUN chown -R 1001:1001 "${LAMBDA_TASK_ROOT}"
  USER 1001:1001
  ```
- **Justificativa**: Executar containers como root é um Security Hotspot crítico no SonarQube
- **Solução**: Usar UID/GID arbitrário (1001:1001) que não precisa existir no /etc/passwd

#### Estrutura Padrão de Dockerfile

```dockerfile
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar arquivos de projeto (.csproj) para restaurar dependências
COPY ["src/Core/...", "src/Core/..."]
COPY ["src/InterfacesExternas/...", "src/InterfacesExternas/..."]

# Restaurar e publicar
RUN dotnet restore "..."
RUN dotnet publish "..." -c Release -o /app/publish --no-restore

# Stage 2: Runtime - AWS Lambda .NET 8
FROM public.ecr.aws/lambda/dotnet:8

# Copiar aplicação publicada
COPY --from=build /app/publish ${LAMBDA_TASK_ROOT}

# Segurança: Rodar como não-root
RUN chown -R 1001:1001 "${LAMBDA_TASK_ROOT}"
USER 1001:1001

ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_ENVIRONMENT=Production

CMD ["Assembly::Namespace.Class::Method"]
```

#### Nomenclatura de Dockerfiles

- `Dockerfile` - Dockerfile principal (se houver apenas um)
- `Dockerfile.<nome-projeto>` - Dockerfiles específicos (ex: `Dockerfile.auth-admin-lambda`, `Dockerfile.auth-customer-lambda`)

---

## CI/CD e GitHub Actions

### Uso de Versões (Correções SonarQube)

**REGRA CRÍTICA**: Sempre usar **commit SHA hash completo** ao invés de tags de versão.

#### ❌ INCORRETO (Security Hotspot)

```yaml
- uses: aws-actions/configure-aws-credentials@v4
- uses: aws-actions/amazon-ecr-login@v2
- uses: docker/setup-buildx-action@v3
```

#### ✅ CORRETO (Seguro e Reprodutível)

```yaml
# v4
- uses: aws-actions/configure-aws-credentials@ff717079ee2060e4bcee96c4779b553acc87447c
# v2
- uses: aws-actions/amazon-ecr-login@a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6q7r8s9t0
# v3
- uses: docker/setup-buildx-action@1a2b3c4d5e6f7g8h9i0j1k2l3m4n5o6p7q8r9s0t
```

#### Justificativa

- **Tags podem ser movidas/reescritas**: Tags Git podem ser deletadas e recriadas apontando para commits diferentes
- **SHA é imutável**: Commit SHA hash é único e imutável, garantindo reprodutibilidade
- **Segurança**: Previne ataques de supply chain onde tags maliciosas podem ser injetadas
- **Rastreabilidade**: Permite rastrear exatamente qual versão da action foi usada

#### Como Obter o SHA

1. Acessar o repositório da action no GitHub
2. Ir para a aba "Releases" ou "Commits"
3. Encontrar o commit da versão desejada (ex: v4)
4. Copiar o SHA completo do commit
5. Usar no workflow mantendo comentário com a versão para referência

#### Padrão Recomendado

```yaml
# Sempre manter comentário com versão para referência
# v4.0.2
- uses: actions/checkout@a5ac7e51b410bd4179b2a86c3b1a1c2d3e4f5g6h
```

### Workflows

- Todos os workflows devem estar em `.github/workflows/`
- Usar nomes descritivos: `build-and-push.yml`, `deploy-terraform.yml`, `run-tests.yml`
- Sempre validar workflows com `act` ou GitHub Actions localmente antes de commitar

---

## Consistência com Outros Serviços

A arquitetura do AuthLambda deve ser **idêntica** à dos serviços HTTP (OrderHub, PayStream, KitchenFlow), mudando apenas o tipo de hosting (Lambda vs EKS).

**Padrão único mental:**
- ✅ Mesma estrutura de pastas (Application, Domain, Infra)
- ✅ Mesma organização horizontal por contexto
- ✅ Mesmo fluxo (InputModel → UseCase → OutputModel → Presenter)
- ✅ Swagger em tudo
- ✅ Testes unitários + BDD
- ✅ Clean Architecture ~80%

---

## O que isso garante

- ✅ **Um único padrão mental** (API/Lambda)
- ✅ Swagger em tudo
- ✅ Clean Arch defendável
- ✅ Excelente DX (Developer Experience)
- ✅ Excelente narrativa para banca e entrevistas
- ✅ Consistência entre todos os serviços do ecossistema

> *"O Auth é um Lambda, mas arquiteturalmente é uma API ASP.NET Core como qualquer outra; só mudamos o host."*

---

## Principais Mudanças e Lições Aprendidas

Esta seção documenta as principais mudanças arquiteturais e correções aplicadas durante o desenvolvimento, para serem reutilizadas em próximos projetos.

### 1. Estrutura de Diretórios Separada

**Mudança**: Separação clara entre camadas core e interfaces externas.

**Estrutura Anterior**:
```
src/
  FastFood.Auth.Lambda/
  FastFood.Auth.Application/
  FastFood.Auth.Domain/
```

**Estrutura Atual**:
```
src/
  Core/                    # Camadas que utilizamos (lógica de negócio)
    FastFood.Auth.Application/
    FastFood.Auth.Domain/
    FastFood.Auth.Infra/
    FastFood.Auth.Infra.Persistence/
  InterfacesExternas/      # Camadas que usamos (adapters)
    FastFood.Auth.Lambda.Admin/
    FastFood.Auth.Lambda.Customer/
    FastFood.Auth.Migrator/
```

**Benefícios**:
- Separação clara de responsabilidades
- Facilita identificação de dependências
- Melhor organização para projetos com múltiplas interfaces (APIs, Lambdas, Console Apps)

### 2. Correção de Security Hotspots em Dockerfiles

**Problema**: SonarQube identificou Security Hotspots críticos em Dockerfiles executando como root.

**Solução Aplicada**:
```dockerfile
# Rodar como não-root sem depender de groupadd/useradd.
# 1001:1001 é um UID/GID arbitrário não-root (não precisa existir no /etc/passwd).
RUN chown -R 1001:1001 "${LAMBDA_TASK_ROOT}"
USER 1001:1001
```

**Impacto**:
- Elimina Security Hotspots do SonarQube
- Segue boas práticas de segurança de containers
- Não requer criação de usuário no sistema (usa UID/GID arbitrário)

**Aplicar em**: Todos os Dockerfiles de novos projetos.

### 3. Uso de Commit SHA em GitHub Actions

**Problema**: Uso de tags de versão (`@v4`, `@v2`) em workflows do GitHub Actions é um Security Hotspot.

**Solução Aplicada**:
```yaml
# ❌ ANTES (inseguro)
- uses: aws-actions/configure-aws-credentials@v4

# ✅ DEPOIS (seguro)
# v4.0.2
- uses: aws-actions/configure-aws-credentials@ff717079ee2060e4bcee96c4779b553acc87447c
```

**Impacto**:
- Elimina Security Hotspots do SonarQube
- Garante reprodutibilidade (SHA é imutável)
- Previne ataques de supply chain
- Melhora rastreabilidade

**Aplicar em**: Todos os workflows do GitHub Actions em novos projetos.

### 4. Organização Horizontal por Contexto

**Mudança**: Application layer organizada horizontalmente por contexto (Customer, Admin) ao invés de verticalmente por tipo.

**Estrutura Anterior** (Vertical):
```
Application/
  Commands/
    Admin/
    Customer/
  UseCases/
    Admin/
    Customer/
```

**Estrutura Atual** (Horizontal):
```
Application/
  UseCases/
    Customer/
    Admin/
  InputModels/
    Customer/
    Admin/
  OutputModels/
    Customer/
    Admin/
  Presenters/
    Customer/
    Admin/
```

**Benefícios**:
- Facilita localização de código relacionado
- Melhor coesão (tudo de Customer junto)
- Mais fácil de entender e manter

### 5. Presenters na Camada Application

**Mudança**: Presenters movidos da camada Lambda (API) para a camada Application.

**Justificativa**:
- Application define o contrato de saída
- API apenas consome o contrato
- Elimina duplicação de ResponseModels
- Centraliza transformações de dados

**Aplicar em**: Todos os novos projetos seguindo Clean Architecture.

### 6. Cobertura de Testes e Qualidade

**Metas Estabelecidas**:
- Cobertura mínima: **≥ 80%** (cobertura de linha)
- Duplicação máxima: **≤ 3%**
- Security Hotspots: **0** (ou justificados)
- Quality Gate do SonarQube: **deve passar**

**Ferramentas**:
- SonarQube para análise estática
- Coverlet para cobertura de código
- xUnit para testes unitários
- SpecFlow para testes BDD

**Aplicar em**: Todos os novos projetos com as mesmas metas de qualidade.

---

## Checklist para Novos Projetos

Ao iniciar um novo projeto, verificar:

- [ ] Estrutura de diretórios separada (Core/ e InterfacesExternas/)
- [ ] Dockerfiles com USER não-root (1001:1001)
- [ ] GitHub Actions usando commit SHA ao invés de tags
- [ ] Application layer organizada horizontalmente por contexto
- [ ] Presenters na camada Application
- [ ] Cobertura de testes ≥ 80%
- [ ] Duplicação ≤ 3%
- [ ] Quality Gate do SonarQube passando
- [ ] Swagger habilitado e documentado
- [ ] Testes BDD implementados


