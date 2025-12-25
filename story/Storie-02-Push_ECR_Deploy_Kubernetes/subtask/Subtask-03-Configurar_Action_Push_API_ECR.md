# Subtask 03: Configurar GitHub Actions para push da API no ECR

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** [DD/MM/AAAA]

## Descrição
Criar workflow do GitHub Actions que faz build da imagem Docker da API e realiza push para o Amazon ECR, usando commit SHA como tag da imagem.

## Passos de implementação
- [ ] Criar diretório `.github/workflows/` se não existir
- [ ] Criar arquivo `.github/workflows/push-to-ecr-api.yml`
- [ ] Configurar trigger `workflow_dispatch` para execução manual
- [ ] Adicionar step de checkout do código
- [ ] Configurar credenciais AWS usando `aws-actions/configure-aws-credentials` (usar commit SHA completo conforme ARCHITECTURE_RULES.md)
- [ ] Fazer login no ECR usando `aws-actions/amazon-ecr-login` (usar commit SHA completo)
- [ ] Configurar Docker Buildx para build otimizado
- [ ] Gerar tag baseada no SHA do commit (`git rev-parse --short HEAD`)
- [ ] Executar build da imagem da API usando o Dockerfile
- [ ] Fazer tag da imagem com URL completa do ECR
- [ ] Executar push da imagem para o ECR
- [ ] Adicionar outputs do workflow (URL da imagem, tag)

## Como testar
- Executar workflow manualmente via GitHub Actions UI (workflow_dispatch)
- Verificar que o build completa sem erros
- Validar que a imagem foi criada no ECR usando AWS Console ou CLI: `aws ecr describe-images --repository-name orderhub-api`
- Verificar que a tag da imagem corresponde ao SHA do commit
- Executar workflow em branch de teste antes de merge na main
- Validar logs do workflow para confirmar cada etapa

## Critérios de aceite
- [ ] Arquivo `.github/workflows/push-to-ecr-api.yml` criado
- [ ] Workflow configurado com trigger `workflow_dispatch`
- [ ] Credenciais AWS configuradas corretamente (usando secrets)
- [ ] Login no ECR funcionando
- [ ] Build da imagem completando com sucesso
- [ ] Push para ECR executando sem erros
- [ ] Tag da imagem baseada no SHA do commit
- [ ] Imagem visível no repositório ECR `orderhub-api`
- [ ] Workflow usando commit SHA hash completo para actions (conforme ARCHITECTURE_RULES.md)

