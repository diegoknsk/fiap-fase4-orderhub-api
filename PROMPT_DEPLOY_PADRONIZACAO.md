# Prompt para Padronizar Deploy nos Outros Projetos

Use este prompt para ajustar o workflow de deploy dos outros microserviços:

---

## Prompt

Ajuste o workflow de deploy do GitHub Actions (`.github/workflows/deploy-to-eks.yml`) para seguir o mesmo padrão do projeto orderhub, que funciona corretamente.

### Requisitos:

1. **Nome do workflow**: `Deploy [NomeDoProjeto] Application` (ex: `Deploy KitchenFlow Application`)

2. **Trigger**: `workflow_dispatch` com input opcional `skip_migrator` (boolean, default: false)

3. **Variáveis de ambiente** (ajustar conforme o projeto):
   - `AWS_REGION: us-east-1`
   - `EKS_CLUSTER_NAME: eks-fiap-fase4-infra`
   - `KUBERNETES_NAMESPACE: [nome-do-namespace]` (ex: `kitchenflow`, `payment`, etc)
   - `DEPLOYMENT_NAME: [nome-do-deployment]` (ex: `kitchenflow-api`, `payment-api`, etc)
   - `CONTAINER_NAME: api`
   - `ECR_REPOSITORY: fiap-fase4-infra-[nome-do-projeto]-api` (ex: `fiap-fase4-infra-kitchenflow-api`)

4. **Fluxo do workflow**:
   - Checkout do código
   - Configurar credenciais AWS
   - Login no ECR
   - Gerar TAG baseada no SHA curto do commit (`git rev-parse --short HEAD`)
   - Build e push da imagem da API usando `docker/build-push-action@v5`:
     - Tags: `api-${TAG}` e `api-latest`
     - Dockerfile: `./src/InterfacesExternas/FastFood.[NomeProjeto].Api/Dockerfile`
     - Cache habilitado
   - Build e push da imagem do Migrator (se não for skip):
     - Tags: `migrator-${TAG}` e `migrator-latest`
     - Dockerfile: `./src/InterfacesExternas/FastFood.[NomeProjeto].Migrator/Dockerfile`
     - Cache habilitado
   - Configurar kubectl para o cluster EKS
   - Atualizar deployment da API com a nova imagem
   - Aguardar rollout completar (timeout: 300s)
   - Recriar job do migrator:
     - Deletar job existente (nome fixo: `[nome-projeto]-migrator`)
     - Aguardar 10 segundos
     - Criar novo job com a nova imagem
     - Aguardar job completar (timeout: 600s)
   - Verificar deployment e pods

5. **Job do Migrator** deve ter:
   - Nome fixo: `[nome-projeto]-migrator` (ex: `kitchenflow-migrator`)
   - Labels: `app: [nome-projeto]-migrator` e `service: [nome-projeto]`
   - ConfigMap: `[nome-projeto]-config`
   - Secret: `[nome-projeto]-secrets`
   - Command: `["dotnet", "FastFood.[NomeProjeto].Migrator.dll"]`
   - Recursos: requests (cpu: 100m, memory: 256Mi), limits (cpu: 500m, memory: 512Mi)

6. **Verificação final** deve mostrar:
   - Status do deployment
   - Pods da API (label: `app=[nome-projeto]-api`)
   - Job e pods do migrator (se não for skip)

### Informações do projeto atual:
- Nome do projeto: [INFORMAR]
- Namespace Kubernetes: [INFORMAR]
- Nome do deployment: [INFORMAR]
- Nome do repositório ECR: [INFORMAR]
- Caminho do Dockerfile da API: [INFORMAR]
- Caminho do Dockerfile do Migrator: [INFORMAR]
- Nome da DLL do Migrator: [INFORMAR]

### Referência:
O workflow de referência está em: `.github/workflows/deploy-to-eks.yml` do projeto orderhub.

---

## Exemplo de uso:

Para o projeto KitchenFlow, você usaria:

```
Ajuste o workflow de deploy do GitHub Actions para seguir o mesmo padrão do projeto orderhub.

Informações do projeto:
- Nome do projeto: KitchenFlow
- Namespace Kubernetes: kitchenflow
- Nome do deployment: kitchenflow-api
- Nome do repositório ECR: fiap-fase4-infra-kitchenflow-api
- Caminho do Dockerfile da API: ./src/InterfacesExternas/FastFood.KitchenFlow.Api/Dockerfile
- Caminho do Dockerfile do Migrator: ./src/InterfacesExternas/FastFood.KitchenFlow.Migrator/Dockerfile
- Nome da DLL do Migrator: FastFood.KitchenFlow.Migrator.dll
```




