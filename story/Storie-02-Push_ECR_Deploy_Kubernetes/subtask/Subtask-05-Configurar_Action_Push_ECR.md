# Subtask 05: Configurar GitHub Actions para push no ECR

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** [DD/MM/AAAA]

## Descrição
Criar workflow do GitHub Actions que faz build das imagens Docker da API e do Migrator e realiza push para o Amazon ECR usando **UM ÚNICO repositório** com tags diferentes. O workflow deve fazer push de ambas as imagens no mesmo repositório `fiap-fase4-infra-orderhub-api`, usando tags `api-${TAG}` e `migrator-${TAG}`.

## Passos de implementação
- [ ] Criar diretório `.github/workflows/` se não existir
- [ ] Criar arquivo `.github/workflows/push-to-ecr.yml`
- [ ] Configurar trigger `workflow_dispatch` para execução manual
- [ ] Adicionar step de checkout do código
- [ ] Configurar variáveis de ambiente:
  - `AWS_REGION: us-east-1`
  - `ECR_REPOSITORY: fiap-fase4-infra-orderhub-api`
- [ ] Configurar credenciais AWS usando `aws-actions/configure-aws-credentials@v4` (usar commit SHA completo conforme ARCHITECTURE_RULES.md)
- [ ] Fazer login no ECR usando `aws-actions/amazon-ecr-login@v2`
- [ ] Configurar Docker Buildx para build otimizado
- [ ] Gerar tag baseada no SHA do commit: `IMAGE_TAG=$(git rev-parse --short HEAD)`
- [ ] **Build e push da imagem da API**:
  - Build usando Dockerfile: `src/InterfacesExternas/FastFood.OrderHub.Api/Dockerfile`
  - Tag: `${ECR_REGISTRY}/${ECR_REPOSITORY}:api-${IMAGE_TAG}`
  - Tag adicional: `${ECR_REGISTRY}/${ECR_REPOSITORY}:api-latest`
  - Push ambas as tags
- [ ] **Build e push da imagem do Migrator**:
  - Build usando Dockerfile: `src/InterfacesExternas/FastFood.OrderHub.Migrator/Dockerfile`
  - Tag: `${ECR_REGISTRY}/${ECR_REPOSITORY}:migrator-${IMAGE_TAG}`
  - Tag adicional: `${ECR_REGISTRY}/${ECR_REPOSITORY}:migrator-latest`
  - Push ambas as tags
- [ ] Validar imagens após push:
  - Verificar tag `api-${IMAGE_TAG}` no repositório
  - Verificar tag `migrator-${IMAGE_TAG}` no repositório
- [ ] Adicionar outputs do workflow (URLs das imagens, tags)

## Como testar
- Executar workflow manualmente via GitHub Actions UI (workflow_dispatch)
- Verificar que o build de ambas as imagens completa sem erros
- Validar que as imagens foram criadas no ECR usando AWS Console ou CLI:
  ```bash
  aws ecr describe-images --repository-name fiap-fase4-infra-orderhub-api --region us-east-1
  ```
- Verificar que ambas as tags existem no mesmo repositório:
  - `api-${IMAGE_TAG}` e `api-latest`
  - `migrator-${IMAGE_TAG}` e `migrator-latest`
- Validar que a tag da imagem corresponde ao SHA do commit
- Executar workflow em branch de teste antes de merge na main
- Validar logs do workflow para confirmar cada etapa

## Critérios de aceite
- [ ] Arquivo `.github/workflows/push-to-ecr.yml` criado
- [ ] Workflow configurado com trigger `workflow_dispatch`
- [ ] Variáveis de ambiente configuradas (`AWS_REGION`, `ECR_REPOSITORY`)
- [ ] Credenciais AWS configuradas corretamente (usando secrets)
- [ ] Login no ECR funcionando
- [ ] Build da imagem da API completando com sucesso
- [ ] Build da imagem do Migrator completando com sucesso
- [ ] Push para ECR executando sem erros para ambas as imagens
- [ ] Tags corretas sendo aplicadas (`api-${TAG}`, `migrator-${TAG}`, `api-latest`, `migrator-latest`)
- [ ] Ambas as imagens visíveis no **mesmo repositório** `fiap-fase4-infra-orderhub-api`
- [ ] Validação de imagens após push funcionando
- [ ] Workflow usando commit SHA hash completo para actions (conforme ARCHITECTURE_RULES.md)

