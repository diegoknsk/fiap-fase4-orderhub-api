# Subtask 02: Criar projetos das camadas Core (Domain, Application, Infra)

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** [DD/MM/AAAA]

## Descrição
Criar os três primeiros projetos das camadas core: Domain (sem dependências), Application (depende de Domain) e Infra (depende de Application). Configurar as referências entre projetos e adicionar todos à solução.

## Passos de implementação
- [ ] Criar projeto `FastFood.OrderHub.Domain` em `src/Core/FastFood.OrderHub.Domain/` como class library .NET 8
- [ ] Criar projeto `FastFood.OrderHub.Application` em `src/Core/FastFood.OrderHub.Application/` como class library .NET 8
- [ ] Criar projeto `FastFood.OrderHub.Infra` em `src/Core/FastFood.OrderHub.Infra/` como class library .NET 8
- [ ] Adicionar referência de Application para Domain
- [ ] Adicionar referência de Infra para Application
- [ ] Adicionar todos os projetos à solução usando `dotnet sln add`
- [ ] Criar estrutura de pastas básica em cada projeto (Entities, UseCases, Ports, Services)
- [ ] Verificar compilação com `dotnet build`

## Como testar
- Executar `dotnet build FastFood.OrderHub.sln` (deve compilar sem erros)
- Executar `dotnet sln list` e verificar que os 3 projetos aparecem na lista
- Verificar que `FastFood.OrderHub.Application.csproj` tem referência a `FastFood.OrderHub.Domain`
- Verificar que `FastFood.OrderHub.Infra.csproj` tem referência a `FastFood.OrderHub.Application`
- Verificar que `FastFood.OrderHub.Domain.csproj` não tem referências a outros projetos do solution
- Executar `dotnet build` em cada projeto individualmente para validar dependências

## Critérios de aceite
- [ ] Projeto `FastFood.OrderHub.Domain` criado e compilando
- [ ] Projeto `FastFood.OrderHub.Application` criado e compilando
- [ ] Projeto `FastFood.OrderHub.Infra` criado e compilando
- [ ] Referência de Application para Domain configurada
- [ ] Referência de Infra para Application configurada
- [ ] Todos os projetos adicionados à solução
- [ ] `dotnet build` executa sem erros
- [ ] Namespaces seguem padrão `FastFood.OrderHub.{Camada}`

