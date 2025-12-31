# Subtask 08: Criar documentação do pipeline

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** [DD/MM/AAAA]

## Descrição
Criar documentação completa do pipeline CI/CD, incluindo configuração de secrets, variáveis de ambiente, como executar workflows, troubleshooting e referências importantes.

## Passos de implementação
- [ ] Criar diretório `docs/` se não existir
- [ ] Criar arquivo `docs/CI_CD_SETUP.md`
- [ ] Documentar estrutura do pipeline:
  - Workflow de push para ECR
  - Workflow de deploy para EKS
- [ ] Documentar secrets do GitHub necessárias:
  - `AWS_ACCESS_KEY_ID`
  - `AWS_SECRET_ACCESS_KEY`
  - `AWS_SESSION_TOKEN` (se necessário)
- [ ] Documentar variáveis de ambiente usadas nos workflows
- [ ] Documentar como executar workflows manualmente
- [ ] Documentar estrutura do repositório ECR:
  - Repositório único: `fiap-fase4-infra-orderhub-api`
  - Tags: `api-${TAG}`, `migrator-${TAG}`, `api-latest`, `migrator-latest`
- [ ] Documentar estrutura do Kubernetes:
  - Namespace: `orderhub`
  - Deployment: `orderhub-api`
  - Job: `orderhub-migrator-${TIMESTAMP}` (nome único)
- [ ] Documentar troubleshooting comum:
  - Erro de autenticação AWS
  - Erro de conexão com EKS
  - Erro de build do Dockerfile
  - Erro de push para ECR
  - Erro de deploy no Kubernetes
- [ ] Documentar referências:
  - Projeto de infraestrutura: `C:\Projetos\Fiap\fiap-fase4-infra`
  - Manifestos Kubernetes: `C:\Projetos\Fiap\fiap-fase4-infra\k8s\app\orderhub\`
  - Projeto de referência (PayStream): `C:\Projetos\Fiap\fiap-fase4-paystream-api`

## Estrutura esperada do documento

```markdown
# CI/CD Setup - OrderHub

## Visão Geral
...

## Secrets do GitHub
...

## Variáveis de Ambiente
...

## Workflows
...

## Estrutura ECR
...

## Estrutura Kubernetes
...

## Como Executar
...

## Troubleshooting
...

## Referências
...
```

## Como testar
- Validar que o arquivo `docs/CI_CD_SETUP.md` existe
- Validar que a documentação está completa e clara
- Validar que todas as secrets estão documentadas
- Validar que os comandos de exemplo estão corretos
- Validar que as referências estão corretas

## Critérios de aceite
- [ ] Arquivo `docs/CI_CD_SETUP.md` criado
- [ ] Documentação completa sobre secrets do GitHub
- [ ] Documentação sobre variáveis de ambiente
- [ ] Documentação sobre como executar workflows
- [ ] Documentação sobre estrutura ECR (repositório único com tags)
- [ ] Documentação sobre estrutura Kubernetes
- [ ] Seção de troubleshooting com problemas comuns
- [ ] Referências para projetos relacionados
- [ ] Documentação clara e fácil de seguir


