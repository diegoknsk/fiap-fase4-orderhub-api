# Subtask 04: Integrar workflow de deploy EKS com pipeline de qualidade

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** [DD/MM/AAAA]

## Descrição
Modificar o workflow de deploy para EKS para que seja acionado automaticamente após a validação de qualidade (testes e Sonar Cloud). O deploy só deve ocorrer se o Quality Gate passar.

## Passos de implementação
- [ ] Modificar `.github/workflows/deploy-to-eks.yml`
- [ ] Adicionar dependência do job de qualidade
- [ ] Configurar para acionar automaticamente após merge na main
- [ ] Adicionar validação de Quality Gate antes do deploy
- [ ] Manter opção de execução manual (workflow_dispatch)
- [ ] Garantir que deploy só ocorre se qualidade passar
- [ ] Adicionar step de verificação de cobertura mínima (85%)

## Estrutura esperada

O workflow deve:
- Ser acionado automaticamente após merge na main (se Quality Gate passar)
- Manter opção de execução manual
- Depender do job de qualidade
- Validar cobertura mínima antes do deploy
- Executar build, push para ECR e deploy no EKS
- Executar migrator job se necessário

## Como testar
- Fazer merge na main e verificar que o workflow é acionado
- Verificar que o deploy só ocorre se Quality Gate passar
- Testar execução manual do workflow
- Verificar que o deploy no EKS ocorre corretamente
- Verificar que as imagens são atualizadas no EKS

## Critérios de aceite
- [ ] Workflow modificado para depender do job de qualidade
- [ ] Workflow acionado automaticamente após merge na main
- [ ] Deploy só ocorre se Quality Gate passar
- [ ] Validação de cobertura mínima (85%) implementada
- [ ] Execução manual ainda disponível
- [ ] Deploy no EKS funcionando corretamente
- [ ] Migrator job executado quando necessário
