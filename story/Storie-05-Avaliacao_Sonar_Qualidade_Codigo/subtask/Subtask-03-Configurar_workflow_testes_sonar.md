# Subtask 03: Configurar workflow GitHub Actions com testes e Sonar Cloud

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** [DD/MM/AAAA]

## Descrição
Criar workflow GitHub Actions que execute build, testes com cobertura e análise Sonar Cloud. Este workflow deve ser acionado em PRs e pushes para a branch main.

## Passos de implementação
- [ ] Criar arquivo `.github/workflows/quality.yml`
- [ ] Configurar trigger para PRs e push na main
- [ ] Adicionar step de checkout
- [ ] Adicionar step de setup .NET 8
- [ ] Adicionar step de cache do Sonar
- [ ] Adicionar step de instalação do SonarScanner
- [ ] Adicionar step de início do Sonar (begin)
- [ ] Adicionar step de restore de dependências
- [ ] Adicionar step de build
- [ ] Adicionar step de testes com cobertura (Coverlet)
- [ ] Adicionar step de fim do Sonar (end)
- [ ] Configurar chave do projeto Sonar Cloud
- [ ] Configurar exclusões de cobertura (Program.cs, Startup.cs, DTOs, Migrations)

## Estrutura esperada

O workflow deve:
- Executar em PRs e push para main
- Usar commit SHA para actions (não tags)
- Gerar relatório de cobertura em formato OpenCover
- Enviar resultados para Sonar Cloud
- Configurar exclusões apropriadas de cobertura
- Usar cache para otimizar execução

## Configuração Sonar Cloud

- **Project Key**: `diegoknsk_fiap-fase4-orderhub-api` (ajustar conforme necessário)
- **Organization**: `diegoknsk` (ajustar conforme necessário)
- **Token**: Usar secret `SONAR_TOKEN`

### ⚠️ IMPORTANTE: Desabilitar Análise Automática no SonarCloud

**Erro comum**: `ERROR: You are running CI analysis while Automatic Analysis is enabled.`

Par a resolver este erro, é necessário **desabilitar a Análise Automática** no SonarCloud:

1. Acesse o SonarCloud: https://sonarcloud.io
2. Navegue até o projeto `diegoknsk_fiap-fase4-orderhub-api`
3. Vá em **Administration** → **Analysis Method**
4. Na seção **Automatic Analysis**, desative essa opção
5. Salve as alterações

Isso permitirá que a análise seja realizada exclusivamente pelo pipeline de CI/CD, evitando conflitos.

## Como testar
- Criar um PR e verificar que o workflow é acionado
- Verificar que os testes são executados
- Verificar que a cobertura é gerada
- Verificar que o Sonar Cloud recebe os dados
- Verificar que o Quality Gate é avaliado

## Critérios de aceite
- [ ] Workflow `.github/workflows/quality.yml` criado
- [ ] Workflow acionado em PRs e push para main
- [ ] Testes executados com cobertura
- [ ] Cobertura gerada em formato OpenCover
- [ ] Sonar Cloud configurado corretamente
- [ ] Exclusões de cobertura configuradas
- [ ] Cache do Sonar configurado
- [ ] Workflow usa commit SHA para actions
- [ ] Secret `SONAR_TOKEN` configurado no GitHub
