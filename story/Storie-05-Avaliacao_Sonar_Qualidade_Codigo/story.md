# Storie-05: Avaliação Sonar e Qualidade de Código

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** [DD/MM/AAAA]

## Descrição
Como desenvolvedor, quero implementar uma avaliação completa do Sonar Cloud no projeto OrderHub, alcançando 85% de cobertura de testes, resolvendo todos os erros graves identificados, e configurando um pipeline CI/CD automatizado que execute testes, análise Sonar Cloud e deploy para EKS.

## Objetivo
Implementar uma solução completa de qualidade de código que inclua:
- Configuração do Sonar Cloud para análise contínua
- Aumento da cobertura de testes para 85%
- Resolução de todos os erros graves (Security, Reliability, Maintainability)
- Pipeline CI/CD automatizado integrando testes, Sonar Cloud e deploy EKS
- Arquivo de regras para escrita de testes reutilizável em outros projetos
- Ajuste de configurações sensíveis (appsettings) para não versionar dados de desenvolvimento

## Escopo Técnico
- Tecnologias: .NET 8, Sonar Cloud, GitHub Actions, Coverlet, xUnit
- Arquivos afetados:
  - `.github/workflows/` (novos workflows CI/CD)
  - `src/InterfacesExternas/FastFood.OrderHub.Api/appsettings.json` (limpar valores)
  - `src/InterfacesExternas/FastFood.OrderHub.Api/appsettings.Development.json` (criar, não versionar)
  - `src/InterfacesExternas/FastFood.OrderHub.Migrator/appsettings.json` (limpar valores)
  - `src/InterfacesExternas/FastFood.OrderHub.Migrator/appsettings.Development.json` (criar, não versionar)
  - `.gitignore` (adicionar appsettings.Development.json)
  - `rules/` (criar arquivo de regras de testes)
  - `src/tests/` (aumentar cobertura de testes)
- Recursos AWS: EKS (deploy automático), ECR (push de imagens)

## Subtasks

- [ ] [Subtask 01: Ajustar appsettings.json e criar appsettings.Development.json](./subtask/Subtask-01-Ajustar_appsettings.md)
- [ ] [Subtask 02: Criar arquivo de regras para escrita de testes](./subtask/Subtask-02-Criar_regras_escrita_testes.md)
- [ ] [Subtask 03: Configurar workflow GitHub Actions com testes e Sonar Cloud](./subtask/Subtask-03-Configurar_workflow_testes_sonar.md)
- [ ] [Subtask 04: Integrar workflow de deploy EKS com pipeline de qualidade](./subtask/Subtask-04-Integrar_deploy_eks_pipeline.md)
- [ ] [Subtask 05: Aumentar cobertura de testes para 85%](./subtask/Subtask-05-Aumentar_cobertura_testes.md)
- [ ] [Subtask 06: Resolver erros graves de Security](./subtask/Subtask-06-Resolver_erros_security.md)
- [ ] [Subtask 07: Resolver erros graves de Reliability](./subtask/Subtask-07-Resolver_erros_reliability.md)
- [ ] [Subtask 08: Resolver erros graves de Maintainability](./subtask/Subtask-08-Resolver_erros_maintainability.md)
- [ ] [Subtask 09: Validar Quality Gate do Sonar Cloud](./subtask/Subtask-09-Validar_quality_gate.md)

## Critérios de Aceite da História

- [ ] `appsettings.json` não contém valores sensíveis (apenas estrutura vazia)
- [ ] `appsettings.Development.json` criado e adicionado ao `.gitignore`
- [ ] Arquivo de regras para escrita de testes criado em `rules/`
- [ ] Workflow GitHub Actions configurado executando testes e Sonar Cloud
- [ ] Workflow de deploy EKS acionado automaticamente após qualidade validada
- [ ] Cobertura de testes alcançando 85% ou superior
- [ ] Todos os erros graves de Security resolvidos
- [ ] Todos os erros graves de Reliability resolvidos
- [ ] Todos os erros graves de Maintainability resolvidos
- [ ] Quality Gate do Sonar Cloud passando
- [ ] Pipeline CI/CD executando end-to-end sem erros
- [ ] Documentação atualizada com instruções de uso

## Metas de Qualidade

- **Cobertura de Testes**: ≥ 85%
- **Security**: 0 erros graves
- **Reliability**: 0 erros graves
- **Maintainability**: 0 erros graves (ou aceitos com justificativa)
- **Quality Gate**: Passando
- **Duplicação**: ≤ 3%

## Observações

- Por enquanto, não vamos configurar Sonar local para testes, apenas Sonar Cloud
- O pipeline deve bloquear merge se Quality Gate não passar
- Todos os Security Hotspots devem ser revisados e resolvidos ou marcados como seguros
- O arquivo de regras de testes será reutilizado em outros projetos da organização
