# Subtask 07: Configurar secrets e variáveis do GitHub

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** [DD/MM/AAAA]

## Descrição
Configurar todas as secrets e variáveis necessárias no GitHub para que os workflows de push para ECR e deploy no Kubernetes funcionem corretamente.

## Passos de implementação
- [ ] Identificar todas as secrets necessárias:
  - `AWS_ACCESS_KEY_ID` (credencial AWS)
  - `AWS_SECRET_ACCESS_KEY` (credencial AWS)
  - `AWS_SESSION_TOKEN` (token de sessão AWS, se necessário para credenciais temporárias)
- [ ] Acessar Settings → Secrets and variables → Actions no repositório GitHub
- [ ] Adicionar cada secret individualmente
- [ ] **NÃO configurar** variáveis hardcoded nos workflows (usar variáveis de ambiente inline ou secrets)
- [ ] Validar que as secrets estão sendo referenciadas corretamente nos workflows:
  - `push-to-ecr.yml` deve usar `AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY`, `AWS_SESSION_TOKEN`
  - `deploy-to-eks.yml` deve usar as mesmas secrets AWS
- [ ] Documentar todas as secrets necessárias em `docs/CI_CD_SETUP.md` (será criado na Subtask 08)

## Como testar
- Verificar que todas as secrets estão configuradas no GitHub: Settings → Secrets and variables → Actions
- Executar workflow de push para ECR e validar que as credenciais AWS funcionam
- Executar workflow de deploy e validar que o acesso ao EKS funciona
- Verificar logs dos workflows para confirmar que não há erros de autenticação
- Testar com credenciais inválidas para validar tratamento de erro
- Validar que variáveis de ambiente estão sendo usadas corretamente nos workflows (inline, não como secrets)

## Critérios de aceite
- [ ] Todas as secrets necessárias configuradas no GitHub
- [ ] `AWS_ACCESS_KEY_ID` configurada e funcionando
- [ ] `AWS_SECRET_ACCESS_KEY` configurada e funcionando
- [ ] `AWS_SESSION_TOKEN` configurada (se necessário para credenciais temporárias)
- [ ] Variáveis de ambiente (não sensíveis) configuradas inline nos workflows:
  - `AWS_REGION: us-east-1`
  - `ECR_REPOSITORY: fiap-fase4-infra-orderhub-api`
  - `EKS_CLUSTER_NAME: eks-paystream`
  - `KUBERNETES_NAMESPACE: orderhub`
  - `DEPLOYMENT_NAME: orderhub-api`
  - `CONTAINER_NAME: api`
- [ ] Workflows conseguem acessar as secrets sem erros
- [ ] Nenhuma secret hardcoded nos arquivos de workflow
- [ ] Secrets documentadas em `docs/CI_CD_SETUP.md`


