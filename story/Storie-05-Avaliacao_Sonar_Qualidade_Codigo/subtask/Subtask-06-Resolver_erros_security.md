# Subtask 06: Resolver erros graves de Security

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** [DD/MM/AAAA]

## Descrição
Identificar e resolver todos os erros graves de Security identificados pelo Sonar Cloud. Isso inclui Security Hotspots relacionados a hard-coded secrets, vulnerabilidades de segurança e outras questões críticas.

## Passos de implementação
- [ ] Analisar relatório de Security no Sonar Cloud
- [ ] Identificar todos os erros graves de Security
- [ ] Priorizar Security Hotspots de alta prioridade
- [ ] Remover hard-coded secrets de appsettings.json
- [ ] Mover secrets para variáveis de ambiente ou secrets do GitHub
- [ ] Resolver vulnerabilidades de dependências (se houver)
- [ ] Revisar e marcar como seguros Security Hotspots que não são riscos reais
- [ ] Validar que todos os erros graves foram resolvidos
- [ ] Documentar decisões sobre Security Hotspots marcados como seguros

## Estrutura esperada

As correções devem:
- Remover todos os hard-coded secrets
- Usar variáveis de ambiente ou secrets apropriados
- Seguir princípios de segurança (princípio do menor privilégio)
- Ser documentadas quando necessário
- Não introduzir novos problemas de segurança

## Como testar
- Executar análise Sonar Cloud
- Verificar que não há mais erros graves de Security
- Validar que Security Hotspots foram resolvidos ou marcados como seguros
- Testar que aplicação funciona corretamente após mudanças
- Verificar que secrets não estão mais no código versionado

## Critérios de aceite
- [ ] Todos os erros graves de Security resolvidos
- [ ] Hard-coded secrets removidos do código
- [ ] Secrets movidos para variáveis de ambiente ou GitHub Secrets
- [ ] Security Hotspots revisados e resolvidos ou marcados como seguros
- [ ] Vulnerabilidades de dependências resolvidas (se houver)
- [ ] Aplicação funciona corretamente após mudanças
- [ ] Documentação atualizada quando necessário
