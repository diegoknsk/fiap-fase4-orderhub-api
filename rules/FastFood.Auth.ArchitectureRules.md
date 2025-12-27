# RULES — AuthFastFood (Lambda ASP.NET Core)

## Objetivo
Serviço de autenticação e identificação, publicado como **AWS Lambda**, mas
estruturado como uma **API ASP.NET Core tradicional**, utilizando
Amazon.Lambda.AspNetCoreServer para hospedar a aplicação.

A arquitetura segue ~80% Clean Architecture, com foco em mercado, simplicidade
e consistência com os demais microsserviços do sistema.

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

## Swagger
- Swagger deve estar habilitado:
  - Em ambiente local
  - Em ambiente Lambda (quando permitido pelo API Gateway)
- Endpoints devem ser documentados normalmente.
- Swagger é obrigatório para padronização e inspeção de contrato.

## Estrutura de projetos
- FastFood.Auth.Lambda (ASP.NET Core - API/Lambda host)
- FastFood.Auth.Application
- FastFood.Auth.Domain
- FastFood.Auth.Infra.Persistence (PostgreSQL com EF Core)
- FastFood.Auth.Tests.Unit
- FastFood.Auth.Tests.Bdd

## Regras de dependência
- Api -> Application
- Application -> Domain
- Infra.* implementa interfaces (ports) da Application
- Domain não referencia:
  - ASP.NET
  - EF Core
  - AWS SDK
  - HttpClient
  - Configuration

## Fluxo padrão
Controller -> UseCase.Execute(Command) -> Ports -> Infra

A arquitetura do AuthLambda deve ser idêntica à dos serviços EKS,
mudando apenas o tipo de hosting.

## API (Controllers)
- Controllers são adapters de transporte.
- **Responsabilidades:**
  - autenticação/autorização
  - validação básica de request
  - mapear RequestModel -> InputModel
  - chamar UseCase.ExecuteAsync(InputModel)
  - receber ResponseModel do UseCase e retornar HTTP Response
- **Proibido:**
  - regra de negócio no controller
  - acesso direto a banco ou SDKs
  - chamar Presenter diretamente (responsabilidade do UseCase)

### Endpoints Customer
- `POST /customer/identify` - Identificar customer por CPF
- `POST /customer/register` - Registrar novo customer
- `POST /customer/anonymous` - Criar customer anônimo

### Endpoints Admin
- `POST /admin/login` - Autenticar admin via AWS Cognito

## Application
- UseCases pequenos e focados:
  - IdentifyCustomerUseCase
  - RegisterCustomerUseCase
  - CreateAnonymousCustomerUseCase
  - AuthenticateAdminUseCase
- UseCases recebem apenas InputModels da Application.
- **Responsabilidade do UseCase:**
  - Executar lógica de negócio
  - Chamar Ports (repositórios, serviços)
  - Chamar o Presenter para transformar OutputModel em ResponseModel
  - Retornar ResponseModel (não OutputModel diretamente)
- **Presenters:**
  - Devem estar na camada Application.
  - São chamados pelo UseCase (não pelo Controller).
  - Fazem adaptação/transformação dos OutputModels em ResponseModels quando necessário.
- Ports (interfaces) ficam aqui:
  - ICustomerRepository
  - ITokenService
  - ICognitoService
  - IMessageBus / IEventPublisher (se necessário)

## Domain
- Entidades, Value Objects e validações.
- Exceções de domínio para regras inválidas.
- Nenhuma dependência externa.
- **Entidade Customer:**
  - Id (Guid)
  - Name (string?, nullable)
  - Email (Email?, Value Object, nullable)
  - Cpf (Cpf?, Value Object, nullable)
  - CustomerType (EnumCustomerType)
  - CreatedAt (DateTime)
- **CustomerType:**
  - Registered = 1
  - Anonymous = 2
- **Value Objects:**
  - Cpf: validação de CPF brasileiro (11 dígitos, algoritmo de validação)
  - Email: validação de formato de email

## Persistência
- Banco de dados: **PostgreSQL**
- Infra.Persistence implementa ICustomerRepository
- Entidades de banco são separadas das entidades de domínio
- DbContext nunca é acessado fora da Infra.
- **Tabela Customers:**
  - Id (Guid, PK)
  - Name (varchar 500, nullable)
  - Email (varchar 255, nullable)
  - Cpf (varchar 11, nullable)
  - CustomerType (int) - 1 = Registered, 2 = Anonymous
  - CreatedAt (datetime)
- Value Objects (Cpf, Email) são mapeados como strings no banco
- Usar Entity Framework Core com Npgsql.EntityFrameworkCore.PostgreSQL

## Message Bus
- Comunicação assíncrona via port IMessageBus/IEventPublisher.
- Implementação concreta (SNS/SQS/EventBridge) fica na Infra.

## Autenticação e Tokens

### Customer Authentication
- Tokens JWT gerados para customers (identificados ou anônimos)
- Claims obrigatórias:
  - `sub`: CustomerId (Guid)
  - `customerId`: CustomerId (Guid)
  - `jti`: JWT ID (Guid)
  - `iat`: Issued At (Unix timestamp)
- Configuração via appsettings.json:
  - JwtSettings:Secret
  - JwtSettings:Issuer
  - JwtSettings:Audience
  - JwtSettings:ExpirationHours

### Admin Authentication
- Autenticação via AWS Cognito
- Usar AdminInitiateAuthRequest com AuthFlowType.ADMIN_USER_PASSWORD_AUTH
- Configuração via variáveis de ambiente:
  - COGNITO__REGION
  - COGNITO__USERPOOLID
  - COGNITO__CLIENTID
- Retornar AccessToken, IdToken, ExpiresIn, TokenType
- Port ICognitoService na Application
- Implementação concreta na Infra usando AWSSDK.CognitoIdentityProvider

## Testes e Qualidade
- Testes unitários para:
  - Domain (regras e invariantes)
  - UseCases (com mocks dos ports)
- Pelo menos 1 cenário BDD:
  - "Dado CPF válido, quando identificar, então retorna token"
- Cobertura mínima alvo: >= 80%
- Sonar:
  - Sem code smells críticos
  - Sem vulnerabilidades bloqueantes

## Convenções
- UseCase: <Verbo><Entidade>UseCase
- Command/Query: <Ação><Entidade>Command/Query
- Nada de instanciar dependências fora do bootstrap (DI).
- A arquitetura do AuthFastFood deve espelhar a dos serviços HTTP do sistema.

---

## O que isso te garante (bem importante)
- ✅ **Um único padrão mental** (API/Lambda)
- ✅ Swagger em tudo
- ✅ Clean Arch defendável
- ✅ Excelente DX
- ✅ Excelente narrativa para banca e entrevistas

> *"O Auth é um Lambda, mas arquiteturalmente é uma API ASP.NET Core como qualquer outra; só mudamos o host."*

