# Subtask 02: Implementar teste BDD para GetOrderById

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** [DD/MM/AAAA]

## Descrição
Implementar um teste BDD completo para o caso de uso GetOrderById, incluindo arquivo `.feature` com cenários Gherkin e steps implementados em C# usando SpecFlow.

## Passos de implementação
- [ ] Criar arquivo `Features/GetOrderById.feature` com cenários BDD
- [ ] Implementar cenário: "Admin obtém pedido por ID com sucesso"
- [ ] Implementar cenário: "Customer obtém seu próprio pedido por ID com sucesso"
- [ ] Implementar cenário: "Customer tenta obter pedido de outro cliente e recebe erro"
- [ ] Criar classe `Steps/GetOrderByIdSteps.cs` com binding dos steps
- [ ] Implementar contexto compartilhado para os testes
- [ ] Usar mocks para dependências (IOrderDataSource, IRequestContext)
- [ ] Usar FluentAssertions para assertions

## Como testar
- Executar `dotnet test` no projeto (testes devem compilar e executar)
- Verificar que os cenários aparecem no Test Explorer
- Executar cada cenário individualmente e validar que passam
- Verificar que as assertions estão corretas usando FluentAssertions

## Critérios de aceite
- [ ] Arquivo `Features/GetOrderById.feature` criado com pelo menos 3 cenários
- [ ] Cenários seguem padrão Gherkin (Given-When-Then)
- [ ] Steps implementados em `Steps/GetOrderByIdSteps.cs`
- [ ] Contexto compartilhado criado para gerenciar estado entre steps
- [ ] Mocks configurados corretamente para dependências
- [ ] Testes executam com sucesso (`dotnet test`)
- [ ] Assertions usam FluentAssertions
- [ ] Testes validam comportamentos reais do sistema
