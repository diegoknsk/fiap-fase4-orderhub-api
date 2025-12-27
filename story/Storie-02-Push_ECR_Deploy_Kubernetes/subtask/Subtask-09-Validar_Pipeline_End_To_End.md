# Subtask 09: Validar pipeline completo end-to-end

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** [DD/MM/AAAA]

## Descrição
Validar que todo o pipeline CI/CD funciona corretamente do início ao fim: build das imagens, push para ECR (ambas no mesmo repositório), e deploy no Kubernetes, garantindo que não há erros em nenhuma etapa.

## Passos de implementação
- [ ] Executar workflow de push para ECR (push de ambas as imagens)
- [ ] Validar que ambas as imagens foram criadas no ECR no mesmo repositório `fiap-fase4-infra-orderhub-api`:
  - Tag `api-${TAG}` e `api-latest`
  - Tag `migrator-${TAG}` e `migrator-latest`
- [ ] Executar workflow de deploy no Kubernetes
- [ ] Validar que o deployment da API foi atualizado com a nova imagem usando `kubectl set image`
- [ ] Validar que o Kubernetes Job do Migrator foi criado com nome único (timestamp)
- [ ] Verificar que os pods da API estão rodando corretamente
- [ ] Validar health check endpoint `/health` da API
- [ ] Testar execução do Job do Migrator
- [ ] Validar que o Job do Migrator executa e finaliza com sucesso
- [ ] Testar acesso à API após deploy
- [ ] Validar logs dos pods da API para confirmar que a aplicação iniciou corretamente
- [ ] Validar logs do Job do Migrator para confirmar execução correta
- [ ] Testar rollback em caso de falha no deployment
- [ ] Validar que o pipeline completo executa em tempo aceitável (< 15 minutos)

## Como testar
- Executar sequência completa: push (ambas imagens) → deploy
- Verificar imagens no ECR no mesmo repositório:
  ```bash
  aws ecr list-images --repository-name fiap-fase4-infra-orderhub-api --region us-east-1
  ```
- Validar que ambas as tags existem: `api-${TAG}` e `migrator-${TAG}`
- Validar deployment da API: 
  ```bash
  kubectl get deployment orderhub-api -n orderhub
  ```
- Verificar pods da API: 
  ```bash
  kubectl get pods -n orderhub -l app=orderhub-api
  ```
- Verificar Job do Migrator (nome único): 
  ```bash
  kubectl get jobs -n orderhub -l app=orderhub-migrator
  ```
- Validar execução do Job: 
  ```bash
  kubectl get pods -n orderhub -l job-name=orderhub-migrator-<timestamp>
  ```
- Testar endpoint da API: 
  ```bash
  curl http://<load-balancer-url>/api/hello
  ```
- Testar health check: 
  ```bash
  curl http://<load-balancer-url>/health
  ```
- Verificar logs da API: 
  ```bash
  kubectl logs -n orderhub -l app=orderhub-api --tail=50
  ```
- Verificar logs do Migrator: 
  ```bash
  kubectl logs -n orderhub -l job-name=orderhub-migrator-<timestamp>
  ```
- Validar que a tag da imagem no deployment corresponde à tag do ECR (`api-${TAG}`)
- Validar que a tag da imagem no job corresponde à tag do ECR (`migrator-${TAG}`)
- Testar cenário de falha e rollback automático
- Executar pipeline completo em ambiente de teste antes de produção

## Critérios de aceite
- [ ] Pipeline completo executando sem erros do início ao fim
- [ ] Imagens da API e Migrator sendo criadas no **mesmo repositório ECR** `fiap-fase4-infra-orderhub-api`
- [ ] Tags das imagens correspondendo ao SHA do commit (`api-${TAG}` e `migrator-${TAG}`)
- [ ] Deployment da API no Kubernetes sendo atualizado automaticamente usando `kubectl set image`
- [ ] Kubernetes Job do Migrator sendo criado com nome único (timestamp)
- [ ] Pods do deployment da API rodando e saudáveis
- [ ] Health check endpoint `/health` funcionando
- [ ] Job do Migrator executando e finalizando com sucesso quando acionado
- [ ] API respondendo corretamente após deploy
- [ ] Logs dos pods da API mostrando aplicação iniciada corretamente
- [ ] Logs do Job do Migrator mostrando execução bem-sucedida
- [ ] Rollback automático funcionando em caso de falha no deployment
- [ ] Pipeline validado em ambiente de teste
- [ ] Tempo total do pipeline dentro de limites aceitáveis (< 15 minutos)

