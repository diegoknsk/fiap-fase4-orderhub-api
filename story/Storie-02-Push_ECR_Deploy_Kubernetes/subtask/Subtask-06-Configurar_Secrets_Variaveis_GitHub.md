# Subtask 06: Configurar secrets e variáveis do GitHub

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** [DD/MM/AAAA]

## Descrição
Configurar todas as secrets e variáveis necessárias no GitHub para que os workflows de push para ECR e deploy no Kubernetes funcionem corretamente.

## Passos de implementação
- [ ] Identificar todas as secrets necessárias:
  - `AWS_ACCESS_KEY_ID` (credencial AWS)
  - `AWS_SECRET_ACCESS_KEY` (credencial AWS)
  - `AWS_SESSION_TOKEN` (token de sessão AWS, se necessário)
  - `AWS_REGION` (região AWS, pode ser variável)
  - `ECR_REPOSITORY_API` (URL do repositório ECR da API)
  - `ECR_REPOSITORY_MIGRATOR` (URL do repositório ECR do Migrator)
  - `EKS_CLUSTER_NAME` (nome do cluster EKS)
  - `KUBECONFIG` ou configuração alternativa para acesso ao cluster
- [ ] Acessar Settings → Secrets and variables → Actions no repositório GitHub
- [ ] Adicionar cada secret individualmente
- [ ] Configurar variáveis de ambiente (não sensíveis) se necessário
- [ ] Documentar todas as secrets necessárias em arquivo de documentação
- [ ] Validar que as secrets estão sendo referenciadas corretamente nos workflows

## Como testar
- Verificar que todas as secrets estão configuradas no GitHub: Settings → Secrets and variables → Actions
- Executar workflow de push para ECR e validar que as credenciais AWS funcionam
- Executar workflow de deploy e validar que o acesso ao EKS funciona
- Verificar logs dos workflows para confirmar que não há erros de autenticação
- Testar com credenciais inválidas para validar tratamento de erro
- Validar que variáveis de ambiente estão sendo usadas corretamente nos workflows

## Critérios de aceite
- [ ] Todas as secrets necessárias configuradas no GitHub
- [ ] `AWS_ACCESS_KEY_ID` configurada e funcionando
- [ ] `AWS_SECRET_ACCESS_KEY` configurada e funcionando
- [ ] `AWS_SESSION_TOKEN` configurada (se necessário para credenciais temporárias)
- [ ] `AWS_REGION` configurada (como secret ou variável)
- [ ] URLs dos repositórios ECR configuradas
- [ ] Nome do cluster EKS configurado
- [ ] Kubeconfig ou método de acesso ao cluster configurado
- [ ] Documentação das secrets criada
- [ ] Workflows conseguem acessar as secrets sem erros
- [ ] Nenhuma secret hardcoded nos arquivos de workflow

