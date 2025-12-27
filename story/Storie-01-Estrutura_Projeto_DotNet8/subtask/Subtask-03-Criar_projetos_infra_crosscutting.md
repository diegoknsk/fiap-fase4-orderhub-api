# Subtask 03: Criar projetos Infra.Persistence e CrossCutting

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** [DD/MM/AAAA]

## Descrição
Criar os projetos Infra.Persistence (para implementações de persistência DynamoDB) e CrossCutting (para extensões e configurações compartilhadas). Configurar referências e adicionar à solução.

## Passos de implementação
- [ ] Criar projeto `FastFood.OrderHub.Infra.Persistence` em `src/Infra/FastFood.OrderHub.Infra.Persistence/` como class library .NET 8
- [ ] Criar projeto `FastFood.OrderHub.CrossCutting` em `src/Core/FastFood.OrderHub.CrossCutting/` como class library .NET 8
- [ ] Adicionar referência de Infra.Persistence para Application (para implementar Ports)
- [ ] Adicionar referência de CrossCutting para Application (para extensões de DI)
- [ ] Adicionar ambos os projetos à solução usando `dotnet sln add`
- [ ] Criar estrutura de pastas básica (Repositories em Infra.Persistence, Extensions em CrossCutting)
- [ ] Verificar compilação com `dotnet build`

## Como testar
- Executar `dotnet build FastFood.OrderHub.sln` (deve compilar sem erros)
- Executar `dotnet sln list` e verificar que os 5 projetos core aparecem na lista
- Verificar que `FastFood.OrderHub.Infra.Persistence.csproj` tem referência a `FastFood.OrderHub.Application`
- Verificar que `FastFood.OrderHub.CrossCutting.csproj` tem referência a `FastFood.OrderHub.Application`
- Executar `dotnet build` em cada projeto individualmente

## Critérios de aceite
- [ ] Projeto `FastFood.OrderHub.Infra.Persistence` criado e compilando
- [ ] Projeto `FastFood.OrderHub.CrossCutting` criado e compilando
- [ ] Referência de Infra.Persistence para Application configurada
- [ ] Referência de CrossCutting para Application configurada
- [ ] Ambos os projetos adicionados à solução
- [ ] `dotnet build` executa sem erros
- [ ] Estrutura de pastas criada (Repositories, Extensions)

