# Subtask 01: Criar projeto FastFood.OrderHub.Tests.Bdd

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** [DD/MM/AAAA]

## Descrição
Criar o projeto de testes BDD `FastFood.OrderHub.Tests.Bdd` em `src/tests/` com todas as dependências necessárias (SpecFlow, xUnit, FluentAssertions) e estrutura de pastas adequada.

## Passos de implementação
- [ ] Criar diretório `src/tests/FastFood.OrderHub.Tests.Bdd/`
- [ ] Criar arquivo `.csproj` com configuração de projeto de testes
- [ ] Adicionar pacotes NuGet: SpecFlow 3.9.74, SpecFlow.xUnit 3.9.74, xUnit 2.6.2, FluentAssertions 6.12.0
- [ ] Adicionar pacotes de cobertura: coverlet.collector e coverlet.msbuild
- [ ] Criar estrutura de pastas: `Features/` e `Steps/`
- [ ] Configurar referências aos projetos necessários (Application, Domain, etc.)

## Como testar
- Executar `dotnet restore` no projeto (deve restaurar pacotes sem erros)
- Executar `dotnet build` no projeto (deve compilar sem erros)
- Verificar que a estrutura de pastas foi criada corretamente
- Validar que todos os pacotes NuGet foram instalados corretamente

## Critérios de aceite
- [ ] Projeto criado em `src/tests/FastFood.OrderHub.Tests.Bdd/`
- [ ] Arquivo `.csproj` criado com configuração correta
- [ ] Todos os pacotes NuGet instalados (SpecFlow, xUnit, FluentAssertions, coverlet)
- [ ] Estrutura de pastas `Features/` e `Steps/` criada
- [ ] Projeto compila sem erros
- [ ] Referências aos projetos necessários configuradas
