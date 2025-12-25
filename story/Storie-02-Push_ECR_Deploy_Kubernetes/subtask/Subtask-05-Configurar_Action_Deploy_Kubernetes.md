# Subtask 05: Configurar GitHub Actions para deploy no Kubernetes

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** [DD/MM/AAAA]

## Descrição
Criar workflow do GitHub Actions que realiza deploy automático no cluster Kubernetes (EKS), atualizando o Deployment da API e configurando o Kubernetes Job do Migrator com as novas imagens do ECR após push bem-sucedido.

## Passos de implementação
- [ ] Criar arquivo `.github/workflows/deploy-to-eks.yml`
- [ ] Configurar trigger `workflow_dispatch` e opcionalmente após push para ECR bem-sucedido
- [ ] Adicionar step de checkout do código
- [ ] Configurar credenciais AWS usando `aws-actions/configure-aws-credentials` (usar commit SHA completo)
- [ ] Configurar kubectl usando `aws-actions/configure-kubectl` ou `azure/setup-kubectl` (usar commit SHA completo)
- [ ] Obter kubeconfig do cluster EKS usando `aws eks update-kubeconfig`
- [ ] Obter tag da imagem do commit atual (SHA)
- [ ] Atualizar deployment da API do Kubernetes com nova tag da imagem usando `kubectl set image`
- [ ] Validar deployment da API com `kubectl rollout status`
- [ ] Atualizar Kubernetes Job do Migrator com nova tag da imagem (ou criar job se não existir)
- [ ] Configurar opção de executar o Job do Migrator manualmente via workflow_dispatch
- [ ] Adicionar step de rollback automático em caso de falha no deployment
- [ ] Adicionar outputs do workflow (status do deploy, namespace, deployment name, job status)

## Como testar
- Executar workflow manualmente via GitHub Actions UI após push bem-sucedido para ECR
- Verificar que o kubectl conecta ao cluster EKS corretamente
- Validar que o deployment da API é atualizado com a nova imagem: `kubectl get deployment orderhub-api -n orderhub -o jsonpath='{.spec.template.spec.containers[0].image}'`
- Verificar que o rollout completa com sucesso: `kubectl rollout status deployment/orderhub-api -n orderhub`
- Validar que os pods da API estão rodando: `kubectl get pods -n orderhub -l app=orderhub-api`
- Verificar que o Job do Migrator foi atualizado/criado: `kubectl get job orderhub-migrator -n orderhub`
- Validar que o Job do Migrator pode ser executado manualmente quando necessário
- Testar rollback automático simulando falha no deployment
- Executar workflow em branch de teste antes de merge na main

## Critérios de aceite
- [ ] Arquivo `.github/workflows/deploy-to-eks.yml` criado
- [ ] Workflow configurado com trigger apropriado
- [ ] Credenciais AWS e kubeconfig configuradas corretamente
- [ ] Conexão com cluster EKS funcionando
- [ ] Deployment da API sendo atualizado com nova imagem do ECR
- [ ] Kubernetes Job do Migrator sendo atualizado/criado com nova imagem do ECR
- [ ] Rollout status da API sendo validado
- [ ] Job do Migrator configurado para execução manual quando necessário
- [ ] Rollback automático implementado em caso de falha no deployment
- [ ] Workflow usando commit SHA hash completo para actions (conforme ARCHITECTURE_RULES.md)
- [ ] Deployment e Job atualizados com sucesso no namespace correto

