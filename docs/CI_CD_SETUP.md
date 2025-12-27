# CI/CD Setup - OrderHub

## Visão Geral

Este documento descreve a configuração do pipeline CI/CD para o microserviço OrderHub, incluindo build de imagens Docker, push para Amazon ECR e deploy automático no cluster Kubernetes (EKS).

O pipeline é composto por dois workflows principais:
1. **Push to ECR**: Build e push das imagens Docker (API e Migrator) para o Amazon ECR
2. **Deploy to EKS**: Deploy automático no cluster Kubernetes, atualizando o Deployment da API e criando Job do Migrator

## Secrets do GitHub

As seguintes secrets devem ser configuradas no repositório GitHub em **Settings → Secrets and variables → Actions**:

### Secrets Obrigatórias

- **`AWS_ACCESS_KEY_ID`**: Credencial AWS Access Key ID com permissões para:
  - ECR: `ecr:GetAuthorizationToken`, `ecr:BatchCheckLayerAvailability`, `ecr:GetDownloadUrlForLayer`, `ecr:BatchGetImage`, `ecr:PutImage`, `ecr:InitiateLayerUpload`, `ecr:UploadLayerPart`, `ecr:CompleteLayerUpload`
  - EKS: `eks:DescribeCluster`, `eks:ListClusters`
  - IAM: Permissões para assumir role (se necessário)

- **`AWS_SECRET_ACCESS_KEY`**: Credencial AWS Secret Access Key correspondente

- **`AWS_SESSION_TOKEN`**: Token de sessão AWS (obrigatório se estiver usando credenciais temporárias, opcional para credenciais permanentes)

### Como Configurar

1. Acesse o repositório no GitHub
2. Vá em **Settings → Secrets and variables → Actions**
3. Clique em **New repository secret**
4. Adicione cada secret individualmente com o nome e valor correspondentes
5. Clique em **Add secret**

**⚠️ IMPORTANTE**: Nunca commite secrets ou credenciais no código. Sempre use GitHub Secrets.

## Variáveis de Ambiente

As seguintes variáveis de ambiente são configuradas inline nos workflows (não são secrets):

- **`AWS_REGION`**: `us-east-1`
- **`ECR_REPOSITORY`**: `fiap-fase4-infra-orderhub-api`
- **`EKS_CLUSTER_NAME`**: `eks-paystream`
- **`KUBERNETES_NAMESPACE`**: `orderhub`
- **`DEPLOYMENT_NAME`**: `orderhub-api`
- **`CONTAINER_NAME`**: `api`

Essas variáveis podem ser modificadas diretamente nos arquivos de workflow se necessário.

## Workflows

### 1. Push to ECR (`push-to-ecr.yml`)

**Trigger**: Execução manual via `workflow_dispatch`

**Funcionalidades**:
- Build das imagens Docker da API e do Migrator usando multi-stage build
- Push para o repositório ECR `fiap-fase4-infra-orderhub-api`
- Tags aplicadas:
  - API: `api-${COMMIT_SHA}` e `api-latest`
  - Migrator: `migrator-${COMMIT_SHA}` e `migrator-latest`
- Validação das imagens após push

**Como Executar**:
1. Acesse a aba **Actions** no GitHub
2. Selecione o workflow **Push to ECR**
3. Clique em **Run workflow**
4. Selecione a branch desejada
5. Clique em **Run workflow**

### 2. Deploy to EKS (`deploy-to-eks.yml`)

**Trigger**: 
- Execução manual via `workflow_dispatch`
- Automaticamente após conclusão bem-sucedida do workflow **Push to ECR**

**Funcionalidades**:
- Validação das imagens no ECR antes do deploy
- Atualização do Deployment da API usando `kubectl set image`
- Validação do rollout com timeout de 5 minutos
- Rollback automático em caso de falha no deployment
- Criação de Kubernetes Job do Migrator com nome único (timestamp)
- Aguarda conclusão do Job do Migrator

**Como Executar**:
1. Acesse a aba **Actions** no GitHub
2. Selecione o workflow **Deploy to EKS**
3. Clique em **Run workflow**
4. Selecione a branch desejada
5. Clique em **Run workflow**

**⚠️ IMPORTANTE**: O workflow de deploy só será executado automaticamente se o workflow **Push to ECR** for concluído com sucesso.

## Estrutura ECR

### Repositório Único

O projeto usa **um único repositório ECR** para ambas as imagens (API e Migrator), diferenciando por tags:

- **Repositório**: `fiap-fase4-infra-orderhub-api`
- **Região**: `us-east-1`

### Tags das Imagens

- **API**:
  - `api-${COMMIT_SHA}`: Tag específica do commit (ex: `api-a1b2c3d`)
  - `api-latest`: Tag sempre apontando para a última versão

- **Migrator**:
  - `migrator-${COMMIT_SHA}`: Tag específica do commit (ex: `migrator-a1b2c3d`)
  - `migrator-latest`: Tag sempre apontando para a última versão

### Exemplo de URLs de Imagem

```
123456789012.dkr.ecr.us-east-1.amazonaws.com/fiap-fase4-infra-orderhub-api:api-a1b2c3d
123456789012.dkr.ecr.us-east-1.amazonaws.com/fiap-fase4-infra-orderhub-api:migrator-a1b2c3d
```

## Estrutura Kubernetes

### Namespace

- **Namespace**: `orderhub`

### Deployment da API

- **Nome**: `orderhub-api`
- **Container**: `api`
- **Atualização**: Usa `kubectl set image` para atualizar a imagem do deployment existente

### Job do Migrator

- **Nome**: `orderhub-migrator-${TIMESTAMP}` (nome único com timestamp Unix)
- **Tipo**: Kubernetes Job
- **Restart Policy**: `Never`
- **Criação**: Criado a cada deploy usando `kubectl create job`

### Manifestos Kubernetes

**⚠️ IMPORTANTE**: Os manifestos Kubernetes (Deployment, Service, Job, ConfigMap, Secrets) **NÃO ficam neste projeto**. Eles ficam no projeto de infraestrutura:

```
C:\Projetos\Fiap\fiap-fase4-infra\k8s\app\orderhub\
```

O projeto da aplicação apenas:
- Faz push das imagens Docker para o ECR
- Atualiza a imagem do deployment existente no Kubernetes usando `kubectl set image`
- Cria e executa o Job do Migrator com nome único (timestamp)

## Como Executar

### Execução Manual

1. **Push para ECR**:
   - Acesse **Actions → Push to ECR → Run workflow**
   - Aguarde a conclusão do workflow

2. **Deploy no Kubernetes**:
   - Acesse **Actions → Deploy to EKS → Run workflow**
   - Ou aguarde a execução automática após o push para ECR

### Execução Automática

O workflow de deploy é executado automaticamente após a conclusão bem-sucedida do workflow de push para ECR.

## Troubleshooting

### Erro de Autenticação AWS

**Sintoma**: Erro `Unable to locate credentials` ou `Access Denied`

**Solução**:
1. Verificar se as secrets `AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY` e `AWS_SESSION_TOKEN` estão configuradas corretamente
2. Verificar se as credenciais têm as permissões necessárias (ECR e EKS)
3. Verificar se o `AWS_SESSION_TOKEN` está configurado (obrigatório para credenciais temporárias)

### Erro de Conexão com EKS

**Sintoma**: Erro `Unable to connect to the server` ou `context deadline exceeded`

**Solução**:
1. Verificar se o cluster EKS existe e está acessível: `aws eks describe-cluster --name eks-paystream --region us-east-1`
2. Verificar se as credenciais AWS têm permissão para acessar o cluster
3. Verificar se o kubeconfig está sendo configurado corretamente no workflow

### Erro de Build do Dockerfile

**Sintoma**: Erro durante o build da imagem Docker

**Solução**:
1. Testar o build localmente: `docker build -t orderhub-api -f src/InterfacesExternas/FastFood.OrderHub.Api/Dockerfile .`
2. Verificar se todos os arquivos necessários estão presentes
3. Verificar se as dependências do projeto estão corretas

### Erro de Push para ECR

**Sintoma**: Erro `no basic auth credentials` ou `denied: Your Authorization Token has expired`

**Solução**:
1. Verificar se o login no ECR está sendo executado corretamente
2. Verificar se as credenciais AWS têm permissão para push no ECR
3. Verificar se o repositório ECR existe: `aws ecr describe-repositories --repository-names fiap-fase4-infra-orderhub-api --region us-east-1`

### Erro de Deploy no Kubernetes

**Sintoma**: Erro `deployment not found` ou `rollout failed`

**Solução**:
1. Verificar se o deployment existe no namespace: `kubectl get deployment orderhub-api -n orderhub`
2. Verificar se o namespace existe: `kubectl get namespace orderhub`
3. Verificar se a imagem existe no ECR antes do deploy
4. Verificar os logs do rollout: `kubectl rollout status deployment/orderhub-api -n orderhub`
5. Em caso de falha, o workflow tenta fazer rollback automaticamente

### Erro no Job do Migrator

**Sintoma**: Job do Migrator falha ou não completa

**Solução**:
1. Verificar os logs do job: `kubectl logs job/orderhub-migrator-<timestamp> -n orderhub`
2. Verificar se a imagem do migrator existe no ECR
3. Verificar se o `appsettings.json` está presente no container
4. Verificar se as credenciais AWS estão configuradas no cluster (se necessário)

## Referências

### Projetos Relacionados

- **Projeto de Infraestrutura**: `C:\Projetos\Fiap\fiap-fase4-infra`
- **Manifestos Kubernetes**: `C:\Projetos\Fiap\fiap-fase4-infra\k8s\app\orderhub\`
- **Projeto de Referência (PayStream)**: `C:\Projetos\Fiap\fiap-fase4-paystream-api`

### Documentação AWS

- [Amazon ECR User Guide](https://docs.aws.amazon.com/ecr/)
- [Amazon EKS User Guide](https://docs.aws.amazon.com/eks/)
- [kubectl Documentation](https://kubernetes.io/docs/reference/kubectl/)

### GitHub Actions

- [aws-actions/configure-aws-credentials](https://github.com/aws-actions/configure-aws-credentials)
- [aws-actions/amazon-ecr-login](https://github.com/aws-actions/amazon-ecr-login)
- [docker/build-push-action](https://github.com/docker/build-push-action)
- [azure/setup-kubectl](https://github.com/azure/setup-kubectl)

**⚠️ IMPORTANTE**: Sempre use commit SHA hash completo ao invés de tags de versão nas GitHub Actions (conforme ARCHITECTURE_RULES.md).

## Notas Importantes

1. **Commit SHA**: Os workflows usam commit SHA hash completo (`git rev-parse --short HEAD`) para taggar as imagens, garantindo rastreabilidade
2. **Multi-stage Build**: Os Dockerfiles usam multi-stage build para otimizar o tamanho das imagens
3. **Segurança**: Os containers rodam como usuário não-root (UID/GID 1001:1001)
4. **Rollback Automático**: O workflow de deploy tenta fazer rollback automaticamente em caso de falha no deployment
5. **Validação de Imagens**: As imagens são validadas no ECR antes do deploy para garantir que existem

