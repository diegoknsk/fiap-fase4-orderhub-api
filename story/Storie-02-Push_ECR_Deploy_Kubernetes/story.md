# Storie-02: Push para ECR e Deploy no Kubernetes

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** [DD/MM/AAAA]

## Descrição
Como desenvolvedor DevOps, quero configurar o pipeline de CI/CD para fazer push das imagens Docker (API e Migrator) para o Amazon ECR e realizar o deploy automático no cluster Kubernetes (EKS), incluindo Deployment para a API e Kubernetes Job para o Migrator, para automatizar o processo de publicação e implantação da aplicação OrderHub.

## Objetivo
Criar os Dockerfiles para API e Migrator, configurar GitHub Actions para build e push das imagens para o ECR (usando um único repositório com tags diferentes), implementar endpoint de health check na API, e implementar workflow de deploy automático no Kubernetes (atualizando Deployment da API e criando Job do Migrator com nome único), garantindo que as imagens sejam publicadas e os recursos sejam atualizados automaticamente após validações de qualidade.

## Escopo Técnico
- Tecnologias: Docker, Amazon ECR, GitHub Actions, Kubernetes, kubectl
- Arquivos afetados:
  - `src/InterfacesExternas/FastFood.OrderHub.Api/Dockerfile` (Dockerfile da API)
  - `src/InterfacesExternas/FastFood.OrderHub.Migrator/Dockerfile` (Dockerfile do Migrator)
  - `src/InterfacesExternas/FastFood.OrderHub.Api/Controllers/HealthController.cs` (Health Check endpoint)
  - `src/InterfacesExternas/FastFood.OrderHub.Migrator/appsettings.json` (Configuração do Migrator)
  - `.github/workflows/push-to-ecr.yml` (Workflow de push para ECR - API e Migrator)
  - `.github/workflows/deploy-to-eks.yml` (Workflow de deploy no Kubernetes)
  - `docs/CI_CD_SETUP.md` (Documentação do pipeline)
- Recursos AWS: 
  - ECR: Repositório único `fiap-fase4-infra-orderhub-api` (tags: `api-${TAG}` e `migrator-${TAG}`)
  - EKS: Cluster `eks-paystream` (namespace: `orderhub`, deployment: `orderhub-api`, job: `orderhub-migrator-${TIMESTAMP}`)
- **IMPORTANTE**: Os manifestos Kubernetes (Deployment, Service, Job, ConfigMap, Secrets) **NÃO ficam neste projeto**. Eles ficam no projeto de infraestrutura: `C:\Projetos\Fiap\fiap-fase4-infra\k8s\app\orderhub\`

## Informações do Microserviço

- **Nome do Microserviço**: `orderhub`
- **Nome do ECR Repository**: `fiap-fase4-infra-orderhub-api`
- **Namespace Kubernetes**: `orderhub`
- **Nome do Deployment**: `orderhub-api`
- **Nome do Container**: `api`
- **Nome do Job**: `orderhub-migrator-${TIMESTAMP}` (nome único com timestamp)

## Lições Aprendidas e Padrões Obrigatórios

### 1. Estrutura de Repositório ECR
**CRÍTICO**: Usar **UM ÚNICO repositório ECR** para ambas as imagens (API e Migrator) do mesmo microserviço, diferenciando por **tags**:
- **Repositório único**: `fiap-fase4-infra-orderhub-api`
- **Tags da API**: `api-${TAG}` e `api-latest`
- **Tags do Migrator**: `migrator-${TAG}` e `migrator-latest`

### 2. Manifestos Kubernetes
**IMPORTANTE**: Os manifestos Kubernetes **NÃO ficam no projeto da aplicação**. Eles ficam no projeto de infraestrutura: `C:\Projetos\Fiap\fiap-fase4-infra\k8s\app\orderhub\`

O projeto da aplicação apenas:
- Faz push das imagens Docker para o ECR
- Atualiza a imagem do deployment existente no Kubernetes usando `kubectl set image`
- Cria e executa o Job do Migrator com nome único (timestamp)

### 3. Dockerfile do Migrator - Tratamento de Migrações Vazias
**PROBLEMA COMUM**: A pasta `Migrations/` pode estar vazia inicialmente, causando erro no build.
**SOLUÇÃO**: O Dockerfile do Migrator deve criar um arquivo `.keep` se a pasta estiver vazia.

### 4. Health Check Endpoint
**OBRIGATÓRIO**: A API deve ter um endpoint `/health` para os health checks do Kubernetes.

## Subtasks

- [ ] [Subtask 01: Criar Dockerfile para API](./subtask/Subtask-01-Criar_Dockerfile_API.md) - *Data de Conclusão: [DD/MM/AAAA]*
- [ ] [Subtask 02: Criar Dockerfile para Migrator](./subtask/Subtask-02-Criar_Dockerfile_Migrator.md) - *Data de Conclusão: [DD/MM/AAAA]*
- [ ] [Subtask 03: Criar Health Check Endpoint](./subtask/Subtask-03-Criar_Health_Check_Endpoint.md) - *Data de Conclusão: [DD/MM/AAAA]*
- [ ] [Subtask 04: Criar appsettings.json do Migrator](./subtask/Subtask-04-Criar_appsettings_Migrator.md) - *Data de Conclusão: [DD/MM/AAAA]*
- [ ] [Subtask 05: Configurar GitHub Actions para push no ECR](./subtask/Subtask-05-Configurar_Action_Push_ECR.md) - *Data de Conclusão: [DD/MM/AAAA]*
- [ ] [Subtask 06: Configurar GitHub Actions para deploy no Kubernetes](./subtask/Subtask-06-Configurar_Action_Deploy_Kubernetes.md) - *Data de Conclusão: [DD/MM/AAAA]*
- [ ] [Subtask 07: Configurar secrets e variáveis do GitHub](./subtask/Subtask-07-Configurar_Secrets_Variaveis_GitHub.md) - *Data de Conclusão: [DD/MM/AAAA]*
- [ ] [Subtask 08: Criar documentação do pipeline](./subtask/Subtask-08-Criar_Documentacao_Pipeline.md) - *Data de Conclusão: [DD/MM/AAAA]*
- [ ] [Subtask 09: Validar pipeline completo end-to-end](./subtask/Subtask-09-Validar_Pipeline_End_To_End.md) - *Data de Conclusão: [DD/MM/AAAA]*

## Critérios de Aceite da História

- [ ] Dockerfile da API criado e buildando imagem Docker com sucesso localmente
- [ ] Dockerfile do Migrator criado e buildando imagem Docker com sucesso localmente (mesmo sem migrações)
- [ ] Health Check endpoint `/health` criado e funcionando na API
- [ ] appsettings.json do Migrator criado
- [ ] GitHub Action de push para ECR configurada e executando com sucesso (push de ambas as imagens no mesmo repositório)
- [ ] Imagens da API e Migrator sendo publicadas no ECR com tags corretas (`api-${TAG}` e `migrator-${TAG}`) no repositório `fiap-fase4-infra-orderhub-api`
- [ ] GitHub Action de deploy no Kubernetes configurada e executando com sucesso
- [ ] Deployment da API no Kubernetes sendo atualizado automaticamente usando `kubectl set image`
- [ ] Kubernetes Job do Migrator sendo criado com nome único (timestamp) e executando quando necessário
- [ ] Secrets do GitHub configuradas (AWS_ACCESS_KEY_ID, AWS_SECRET_ACCESS_KEY, AWS_SESSION_TOKEN)
- [ ] Documentação de configuração de secrets e pipeline criada (`docs/CI_CD_SETUP.md`)
- [ ] Pipeline completo executando end-to-end sem erros (build → push → deploy)
- [ ] Imagens Docker otimizadas (multi-stage build) e com tamanho reduzido
- [ ] Workflows usando commit SHA hash completo ao invés de tags de versão (conforme ARCHITECTURE_RULES.md)
- [ ] Validação de imagens no ECR antes de deploy
- [ ] Rollback implementado em caso de falha no deployment

