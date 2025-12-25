# Storie-01: Estrutura Inicial do Projeto .NET 8

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** [DD/MM/AAAA]

## Descrição
Como desenvolvedor, quero criar a estrutura inicial do projeto OrderHub com todas as camadas da Clean Architecture em .NET 8, para ter uma base sólida e organizada para desenvolvimento das funcionalidades do microsserviço.

## Objetivo
Criar a estrutura completa do projeto .NET 8 seguindo Clean Architecture, incluindo todas as camadas (Domain, Application, Infra, Infra.Persistence, CrossCutting, Api, Migrator), configurar a solução (.sln), implementar uma rota "olá mundo" na API para validação inicial, configurar Swagger para documentação da API, e criar o projeto Migrator básico para futuras migrações do DynamoDB.

## Escopo Técnico
- Tecnologias: .NET 8, ASP.NET Core, Swagger/OpenAPI
- Arquivos afetados:
  - `FastFood.OrderHub.sln` (solução)
  - `src/Core/FastFood.OrderHub.Domain/` (projeto Domain)
  - `src/Core/FastFood.OrderHub.Application/` (projeto Application)
  - `src/Core/FastFood.OrderHub.Infra/` (projeto Infra)
  - `src/Core/FastFood.OrderHub.Infra.Persistence/` (projeto Infra.Persistence)
  - `src/Core/FastFood.OrderHub.CrossCutting/` (projeto CrossCutting)
  - `src/InterfacesExternas/FastFood.OrderHub.Api/` (projeto API)
  - `src/InterfacesExternas/FastFood.OrderHub.Migrator/` (projeto Migrator)
  - `src/tests/FastFood.OrderHub.Tests.Unit/` (projeto de testes)
- Recursos AWS: Nenhum (apenas estrutura de código)

## Subtasks

- [ ] [Subtask 01: Criar estrutura de pastas e solução .NET](./subtask/Subtask-01-Criar_estrutura_pastas_solucao.md) - *Data de Conclusão: [DD/MM/AAAA]*
- [ ] [Subtask 02: Criar projetos das camadas Core (Domain, Application, Infra)](./subtask/Subtask-02-Criar_projetos_camadas_core.md) - *Data de Conclusão: [DD/MM/AAAA]*
- [ ] [Subtask 03: Criar projetos Infra.Persistence e CrossCutting](./subtask/Subtask-03-Criar_projetos_infra_crosscutting.md) - *Data de Conclusão: [DD/MM/AAAA]*
- [ ] [Subtask 04: Criar projeto API e configurar Dependency Injection](./subtask/Subtask-04-Criar_projeto_api_configurar_di.md) - *Data de Conclusão: [DD/MM/AAAA]*
- [ ] [Subtask 05: Implementar rota "olá mundo" e configurar Swagger](./subtask/Subtask-05-Implementar_rota_ola_mundo_swagger.md) - *Data de Conclusão: [DD/MM/AAAA]*
- [ ] [Subtask 06: Criar projeto de testes unitários e teste básico](./subtask/Subtask-06-Criar_projeto_testes_unitarios.md) - *Data de Conclusão: [DD/MM/AAAA]*
- [ ] [Subtask 07: Criar projeto Migrator básico](./subtask/Subtask-07-Criar_projeto_migrator_basico.md) - *Data de Conclusão: [DD/MM/AAAA]*

## Critérios de Aceite da História

- [ ] Solução `.sln` criada e todos os projetos adicionados corretamente
- [ ] Estrutura de pastas seguindo o padrão definido em `orderhub-context.mdc`
- [ ] Todos os projetos compilando sem erros
- [ ] API rodando localmente e respondendo na rota `/api/hello`
- [ ] Swagger configurado e acessível em `/swagger`
- [ ] Rota "olá mundo" retornando resposta JSON válida
- [ ] Projeto de testes criado e pelo menos um teste básico passando
- [ ] Dependências entre projetos configuradas corretamente (Domain sem dependências, Application depende de Domain, etc.)
- [ ] Namespaces seguindo padrão `FastFood.OrderHub.{Camada}`
- [ ] Comando `dotnet build` executando com sucesso na raiz da solução
- [ ] Projeto Migrator criado e executando com Console.WriteLine básico

