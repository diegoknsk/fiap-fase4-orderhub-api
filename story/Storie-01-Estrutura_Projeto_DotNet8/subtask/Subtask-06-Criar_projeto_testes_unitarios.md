# Subtask 06: Criar projeto de testes unitários e teste básico

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** [DD/MM/AAAA]

## Descrição
Criar o projeto de testes unitários usando xUnit, adicionar referências necessárias, e implementar pelo menos um teste básico para validar que a estrutura de testes está funcionando corretamente.

## Passos de implementação
- [ ] Criar projeto `FastFood.OrderHub.Tests.Unit` em `src/tests/FastFood.OrderHub.Tests.Unit/` como xUnit Test Project .NET 8
- [ ] Adicionar referência do projeto de testes para Domain (para testar entidades futuras)
- [ ] Adicionar projeto à solução usando `dotnet sln add`
- [ ] Instalar pacote NuGet `xunit` (já vem no template, verificar)
- [ ] Instalar pacote NuGet `Moq` para mocks futuros
- [ ] Criar classe de teste básica `HelloWorldTests.cs` com um teste simples
- [ ] Executar testes com `dotnet test`
- [ ] Verificar que o teste passa

## Como testar
- Executar `dotnet test FastFood.OrderHub.sln` (deve executar testes e passar)
- Executar `dotnet test src/tests/FastFood.OrderHub.Tests.Unit/` (deve executar testes do projeto específico)
- Verificar que `dotnet sln list` mostra o projeto de testes
- Verificar que o projeto de testes tem referência ao Domain (ou outro projeto core)
- Verificar que o teste básico aparece na saída do `dotnet test`
- Executar `dotnet build` no projeto de testes individualmente

## Critérios de aceite
- [ ] Projeto `FastFood.OrderHub.Tests.Unit` criado e compilando
- [ ] Projeto adicionado à solução
- [ ] Pacote `xunit` disponível (vem no template)
- [ ] Pacote `Moq` instalado
- [ ] Classe de teste básica criada com pelo menos um teste
- [ ] `dotnet test` executa e o teste passa
- [ ] Estrutura de testes pronta para futuros testes unitários
- [ ] Referência ao Domain (ou outro projeto core) configurada

