# Subtask 07: Validar pipeline completo end-to-end

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** [DD/MM/AAAA]

## Descrição
Validar que todo o pipeline CI/CD funciona corretamente do início ao fim: build das imagens, push para ECR, e deploy no Kubernetes, garantindo que não há erros em nenhuma etapa.

## Passos de implementação
- [ ] Executar workflow de push da API para ECR
- [ ] Validar que a imagem foi criada no ECR com a tag correta
- [ ] Executar workflow de push do Migrator para ECR
- [ ] Validar que a imagem foi criada no ECR com a tag correta
- [ ] Executar workflow de deploy no Kubernetes
- [ ] Validar que o deployment da API foi atualizado com a nova imagem
- [ ] Validar que o Kubernetes Job do Migrator foi atualizado/criado
- [ ] Verificar que os pods da API estão rodando corretamente
- [ ] Testar execução do Job do Migrator manualmente
- [ ] Validar que o Job do Migrator executa e finaliza com sucesso
- [ ] Testar acesso à API após deploy
- [ ] Validar logs dos pods da API para confirmar que a aplicação iniciou corretamente
- [ ] Validar logs do Job do Migrator para confirmar execução correta
- [ ] Testar rollback em caso de falha
- [ ] Documentar processo completo de deploy (API e Migrator)

## Como testar
- Executar sequência completa: push API → push Migrator → deploy
- Verificar imagens no ECR: `aws ecr list-images --repository-name orderhub-api` e `orderhub-migrator`
- Validar deployment da API: `kubectl get deployment orderhub-api -n orderhub`
- Verificar pods da API: `kubectl get pods -n orderhub -l app=orderhub-api`
- Verificar Job do Migrator: `kubectl get job orderhub-migrator -n orderhub`
- Executar Job do Migrator manualmente: `kubectl create job --from=job/orderhub-migrator orderhub-migrator-<timestamp> -n orderhub`
- Validar execução do Job: `kubectl get pods -n orderhub -l job-name=orderhub-migrator-<timestamp>`
- Testar endpoint da API: `curl http://<load-balancer-url>/api/hello`
- Verificar logs da API: `kubectl logs -n orderhub -l app=orderhub-api --tail=50`
- Verificar logs do Migrator: `kubectl logs -n orderhub -l job-name=orderhub-migrator-<timestamp>`
- Validar que a tag da imagem no deployment e job corresponde à tag do ECR
- Testar cenário de falha e rollback automático
- Executar pipeline completo em ambiente de teste antes de produção

## Critérios de aceite
- [ ] Pipeline completo executando sem erros do início ao fim
- [ ] Imagens da API e Migrator sendo criadas no ECR corretamente
- [ ] Tags das imagens correspondendo ao SHA do commit
- [ ] Deployment da API no Kubernetes sendo atualizado automaticamente
- [ ] Kubernetes Job do Migrator sendo atualizado/criado automaticamente
- [ ] Pods do deployment da API rodando e saudáveis
- [ ] Job do Migrator executando e finalizando com sucesso quando acionado
- [ ] API respondendo corretamente após deploy
- [ ] Logs dos pods da API mostrando aplicação iniciada corretamente
- [ ] Logs do Job do Migrator mostrando execução bem-sucedida
- [ ] Rollback automático funcionando em caso de falha no deployment
- [ ] Documentação do processo de deploy criada (incluindo API e Migrator)
- [ ] Pipeline validado em ambiente de teste
- [ ] Tempo total do pipeline dentro de limites aceitáveis (< 15 minutos)

