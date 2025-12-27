# Subtask 06: Configurar GitHub Actions para deploy no Kubernetes

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** [DD/MM/AAAA]

## Descrição
Criar workflow do GitHub Actions que realiza deploy automático no cluster Kubernetes (EKS), atualizando o Deployment da API usando `kubectl set image` e criando um Kubernetes Job do Migrator com nome único (timestamp) usando a nova imagem do ECR. **IMPORTANTE**: Os manifestos Kubernetes (Deployment, Service, Job, etc.) **NÃO ficam neste projeto**. Eles ficam no projeto de infraestrutura: `C:\Projetos\Fiap\fiap-fase4-infra\k8s\app\orderhub\`

## Passos de implementação
- [ ] Criar arquivo `.github/workflows/deploy-to-eks.yml`
- [ ] Configurar trigger `workflow_dispatch` e opcionalmente após push para ECR bem-sucedido
- [ ] Adicionar step de checkout do código
- [ ] Configurar variáveis de ambiente:
  - `AWS_REGION: us-east-1`
  - `EKS_CLUSTER_NAME: eks-paystream`
  - `KUBERNETES_NAMESPACE: orderhub`
  - `DEPLOYMENT_NAME: orderhub-api`
  - `CONTAINER_NAME: api`
  - `ECR_REPOSITORY: fiap-fase4-infra-orderhub-api`
- [ ] Configurar credenciais AWS usando `aws-actions/configure-aws-credentials@v4` (usar commit SHA completo)
- [ ] Obter kubeconfig do cluster EKS usando `aws eks update-kubeconfig --name ${EKS_CLUSTER_NAME} --region ${AWS_REGION}`
- [ ] Configurar kubectl usando `azure/setup-kubectl@v4` (usar commit SHA completo)
- [ ] Obter tag da imagem do commit atual: `IMAGE_TAG=$(git rev-parse --short HEAD)`
- [ ] **Validar imagens no ECR antes de deploy**:
  - Verificar que `api-${IMAGE_TAG}` existe no repositório
  - Verificar que `migrator-${IMAGE_TAG}` existe no repositório
- [ ] **Atualizar deployment da API**:
  - Construir URL completa da imagem: `FULL_IMAGE="${ECR_REGISTRY}/${ECR_REPOSITORY}:api-${IMAGE_TAG}"`
  - Atualizar usando: `kubectl set image deployment/${DEPLOYMENT_NAME} ${CONTAINER_NAME}=${FULL_IMAGE} -n ${KUBERNETES_NAMESPACE}`
  - Validar rollout: `kubectl rollout status deployment/${DEPLOYMENT_NAME} -n ${KUBERNETES_NAMESPACE} --timeout=5m`
- [ ] **Criar Job do Migrator com nome único**:
  - Gerar nome único: `JOB_NAME="orderhub-migrator-$(date +%s)"`
  - Construir URL completa da imagem: `MIGRATOR_IMAGE="${ECR_REGISTRY}/${ECR_REPOSITORY}:migrator-${IMAGE_TAG}"`
  - Criar job usando `kubectl create job` ou aplicar manifest temporário
  - Usar imagem: `${MIGRATOR_IMAGE}`
  - Configurar `restartPolicy: Never`
  - Adicionar labels apropriados
- [ ] Adicionar step de rollback automático em caso de falha no deployment:
  - Reverter para imagem anterior usando `kubectl rollout undo`
- [ ] Adicionar outputs do workflow (status do deploy, namespace, deployment name, job name, job status)

## Como testar
- Executar workflow manualmente via GitHub Actions UI após push bem-sucedido para ECR
- Verificar que o kubectl conecta ao cluster EKS corretamente
- Validar que as imagens existem no ECR antes do deploy
- Validar que o deployment da API é atualizado com a nova imagem: 
  ```bash
  kubectl get deployment orderhub-api -n orderhub -o jsonpath='{.spec.template.spec.containers[0].image}'
  ```
- Verificar que o rollout completa com sucesso: 
  ```bash
  kubectl rollout status deployment/orderhub-api -n orderhub
  ```
- Validar que os pods da API estão rodando: 
  ```bash
  kubectl get pods -n orderhub -l app=orderhub-api
  ```
- Verificar que o Job do Migrator foi criado com nome único: 
  ```bash
  kubectl get jobs -n orderhub -l app=orderhub-migrator
  ```
- Validar que o Job do Migrator executa e finaliza: 
  ```bash
  kubectl get pods -n orderhub -l job-name=orderhub-migrator-<timestamp>
  ```
- Testar rollback automático simulando falha no deployment
- Executar workflow em branch de teste antes de merge na main

## Critérios de aceite
- [ ] Arquivo `.github/workflows/deploy-to-eks.yml` criado
- [ ] Workflow configurado com trigger apropriado
- [ ] Variáveis de ambiente configuradas (`AWS_REGION`, `EKS_CLUSTER_NAME`, `KUBERNETES_NAMESPACE`, etc.)
- [ ] Credenciais AWS e kubeconfig configuradas corretamente
- [ ] Conexão com cluster EKS funcionando
- [ ] Validação de imagens no ECR antes de deploy implementada
- [ ] Deployment da API sendo atualizado usando `kubectl set image`
- [ ] Rollout status da API sendo validado com timeout
- [ ] Kubernetes Job do Migrator sendo criado com nome único (timestamp)
- [ ] Job do Migrator usando imagem correta do ECR (`migrator-${TAG}`)
- [ ] Rollback automático implementado em caso de falha no deployment
- [ ] Workflow usando commit SHA hash completo para actions (conforme ARCHITECTURE_RULES.md)
- [ ] Deployment e Job atualizados com sucesso no namespace correto (`orderhub`)

