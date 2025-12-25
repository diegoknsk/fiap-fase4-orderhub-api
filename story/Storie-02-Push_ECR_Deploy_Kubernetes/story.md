# Storie-02: Push para ECR e Deploy no Kubernetes

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** [DD/MM/AAAA]

## Descrição
Como desenvolvedor DevOps, quero configurar o pipeline de CI/CD para fazer push das imagens Docker (API e Migrator) para o Amazon ECR e realizar o deploy automático no cluster Kubernetes (EKS), incluindo Deployment para a API e Kubernetes Job para o Migrator, para automatizar o processo de publicação e implantação da aplicação OrderHub.

## Objetivo
Criar os Dockerfiles para API e Migrator, configurar GitHub Actions para build e push das imagens para o ECR, e implementar workflow de deploy automático no Kubernetes (Deployment para API e Job para Migrator), garantindo que as imagens sejam publicadas e os recursos sejam atualizados automaticamente após validações de qualidade.

## Escopo Técnico
- Tecnologias: Docker, Amazon ECR, GitHub Actions, Kubernetes, kubectl
- Arquivos afetados:
  - `src/InterfacesExternas/FastFood.OrderHub.Api/Dockerfile` (Dockerfile da API)
  - `src/InterfacesExternas/FastFood.OrderHub.Migrator/Dockerfile` (Dockerfile do Migrator)
  - `.github/workflows/push-to-ecr.yml` (Workflow de push para ECR)
  - `.github/workflows/deploy-to-eks.yml` (Workflow de deploy no Kubernetes)
- Recursos AWS: ECR (repositórios de imagens), EKS (cluster Kubernetes - já existente)

## Subtasks

- [ ] [Subtask 01: Criar Dockerfile para API](./subtask/Subtask-01-Criar_Dockerfile_API.md) - *Data de Conclusão: [DD/MM/AAAA]*
- [ ] [Subtask 02: Criar Dockerfile para Migrator](./subtask/Subtask-02-Criar_Dockerfile_Migrator.md) - *Data de Conclusão: [DD/MM/AAAA]*
- [ ] [Subtask 03: Configurar GitHub Actions para push da API no ECR](./subtask/Subtask-03-Configurar_Action_Push_API_ECR.md) - *Data de Conclusão: [DD/MM/AAAA]*
- [ ] [Subtask 04: Configurar GitHub Actions para push do Migrator no ECR](./subtask/Subtask-04-Configurar_Action_Push_Migrator_ECR.md) - *Data de Conclusão: [DD/MM/AAAA]*
- [ ] [Subtask 05: Configurar GitHub Actions para deploy no Kubernetes](./subtask/Subtask-05-Configurar_Action_Deploy_Kubernetes.md) - *Data de Conclusão: [DD/MM/AAAA]*
- [ ] [Subtask 06: Configurar secrets e variáveis do GitHub](./subtask/Subtask-06-Configurar_Secrets_Variaveis_GitHub.md) - *Data de Conclusão: [DD/MM/AAAA]*
- [ ] [Subtask 07: Validar pipeline completo end-to-end](./subtask/Subtask-07-Validar_Pipeline_End_To_End.md) - *Data de Conclusão: [DD/MM/AAAA]*

## Critérios de Aceite da História

- [ ] Dockerfile da API criado e buildando imagem Docker com sucesso localmente
- [ ] Dockerfile do Migrator criado e buildando imagem Docker com sucesso localmente
- [ ] GitHub Action de push para ECR configurada e executando com sucesso
- [ ] Imagens da API e Migrator sendo publicadas no ECR com tags baseadas no SHA do commit
- [ ] GitHub Action de deploy no Kubernetes configurada e executando com sucesso
- [ ] Deployment da API no Kubernetes sendo atualizado automaticamente após push bem-sucedido para ECR
- [ ] Kubernetes Job do Migrator configurado e executando quando necessário (manutenção do DynamoDB)
- [ ] Secrets do GitHub configuradas (AWS_ACCESS_KEY_ID, AWS_SECRET_ACCESS_KEY, AWS_SESSION_TOKEN, KUBECONFIG)
- [ ] Workflows validados com `--dry-run` antes de execução real
- [ ] Documentação de configuração de secrets criada
- [ ] Pipeline completo executando end-to-end sem erros (build → push → deploy)
- [ ] Imagens Docker otimizadas (multi-stage build) e com tamanho reduzido
- [ ] Workflows usando commit SHA hash completo ao invés de tags de versão (conforme ARCHITECTURE_RULES.md)

