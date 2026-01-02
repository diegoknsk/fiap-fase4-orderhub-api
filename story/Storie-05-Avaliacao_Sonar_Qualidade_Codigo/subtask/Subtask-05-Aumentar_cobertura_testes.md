# Subtask 05: Aumentar cobertura de testes para 80%

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** [DD/MM/AAAA]

## Descrição
Implementar testes adicionais para aumentar a cobertura de código para pelo menos 80%. Focar em áreas críticas do código que ainda não estão cobertas por testes.

## Passos de implementação
- [ ] Analisar relatório de cobertura atual do Sonar Cloud
- [ ] Identificar áreas com baixa cobertura
- [ ] Priorizar áreas críticas (UseCases, Domain, Gateways)
- [ ] Criar testes unitários para UseCases não cobertos
- [ ] Criar testes unitários para entidades de domínio
- [ ] Criar testes unitários para serviços e validadores
- [ ] Criar testes de integração para controllers
- [ ] Validar que cobertura alcançou 80% ou superior
- [ ] Executar testes localmente e validar que passam

## Estrutura esperada

Os testes devem:
- Seguir padrões definidos em `rules/TEST_WRITING_RULES.md`
- Usar padrão AAA (Arrange, Act, Assert)
- Ter nomenclatura clara e descritiva
- Cobrir casos de sucesso e falha
- Ser independentes e executáveis isoladamente
- Usar mocks apropriados para dependências externas

## Como testar
- Executar `dotnet test --collect:"XPlat Code Coverage"`
- Verificar relatório de cobertura no Sonar Cloud
- Validar que cobertura está acima de 80%
- Executar todos os testes e verificar que passam
- Verificar que novos testes seguem padrões definidos

## Critérios de aceite
- [ ] Cobertura de testes alcançando 80% ou superior
- [ ] Testes criados seguem padrões definidos
- [ ] Todos os testes passando
- [ ] UseCases críticos cobertos por testes
- [ ] Entidades de domínio cobertas por testes
- [ ] Serviços e validadores cobertos por testes
- [ ] Controllers cobertos por testes de integração
- [ ] Relatório de cobertura validado no Sonar Cloud
