# Subtask 09: Validar Quality Gate do Sonar Cloud

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** [DD/MM/AAAA]

## Descrição
Validar que o Quality Gate do Sonar Cloud está configurado corretamente e está passando. Garantir que o pipeline bloqueia merges quando o Quality Gate não passa.

## Passos de implementação
- [ ] Verificar configuração do Quality Gate no Sonar Cloud
- [ ] Validar que cobertura mínima está configurada (80%)
- [ ] Validar que erros graves bloqueiam o Quality Gate
- [ ] Executar pipeline completo e verificar Quality Gate
- [ ] Testar cenário onde Quality Gate falha (verificar bloqueio)
- [ ] Testar cenário onde Quality Gate passa (verificar continuidade)
- [ ] Documentar configuração do Quality Gate
- [ ] Validar que pipeline bloqueia merge quando Quality Gate falha

## Como Configurar Quality Gate com Cobertura Mínima de 80%

### Passo 1: Acessar Configuração do Quality Gate

1. Acesse o SonarCloud: https://sonarcloud.io
2. Navegue até o projeto `diegoknsk_fiap-fase4-orderhub-api`
3. Vá em **Quality Gates** (no menu superior)
4. Clique em **Create** para criar um novo Quality Gate OU edite o Quality Gate existente

### Passo 2: Configurar Condições de Cobertura

No Quality Gate, adicione as seguintes condições:

#### Cobertura de Linhas (Line Coverage)
- **Métrica**: `Coverage on New Code`
- **Operador**: `is greater than`
- **Valor**: `80`
- **Ação**: `Error` (bloqueia o Quality Gate se não atender)

#### Cobertura de Branches (Branch Coverage)
- **Métrica**: `Coverage on New Code`
- **Operador**: `is greater than`
- **Valor**: `80` (opcional, mas recomendado)
- **Ação**: `Error` ou `Warning`

#### Cobertura Geral (Overall Coverage)
- **Métrica**: `Coverage`
- **Operador**: `is greater than`
- **Valor**: `80`
- **Ação**: `Error`

### Passo 3: Configurar Outras Condições Importantes

#### Security
- **Métrica**: `New Security Hotspots`
- **Operador**: `is greater than`
- **Valor**: `0`
- **Ação**: `Error`

- **Métrica**: `Security Rating on New Code`
- **Operador**: `is worse than`
- **Valor**: `A`
- **Ação**: `Error`

#### Reliability
- **Métrica**: `New Bugs`
- **Operador**: `is greater than`
- **Valor**: `0`
- **Ação**: `Error`

- **Métrica**: `Reliability Rating on New Code`
- **Operador**: `is worse than`
- **Valor**: `A`
- **Ação**: `Error`

#### Maintainability
- **Métrica**: `New Code Smells`
- **Operador**: `is greater than`
- **Valor**: `0` (ou um valor aceitável, como 10)
- **Ação**: `Warning` ou `Error`

- **Métrica**: `Maintainability Rating on New Code`
- **Operador**: `is worse than`
- **Valor**: `A`
- **Ação**: `Error`

#### Duplicação
- **Métrica**: `Duplicated Lines on New Code`
- **Operador**: `is greater than`
- **Valor**: `3%` (ou valor em linhas)
- **Ação**: `Warning` ou `Error`

### Passo 4: Aplicar Quality Gate ao Projeto

1. Vá em **Project Settings** → **Quality Gates**
2. Selecione o Quality Gate criado/editado
3. Salve as alterações

### Passo 5: Configurar Quality Gate como Padrão (Opcional)

1. Vá em **Quality Gates** (menu superior)
2. Selecione o Quality Gate
3. Clique em **Set as default** (se quiser usar como padrão para todos os projetos)

## Configuração via sonar-project.properties (Alternativa)

Você também pode configurar algumas condições diretamente no workflow:

```yaml
/d:sonar.qualitygate.wait=true \
/d:sonar.coverage.exclusions="**/*Program.cs,**/*Startup.cs,**/Migrations/**,**/*Dto.cs" \
/d:sonar.coverage.minimum=80
```

**Nota**: A propriedade `sonar.coverage.minimum` pode não estar disponível em todas as versões do SonarCloud. O método recomendado é configurar via interface web.

## Estrutura esperada

O Quality Gate deve:
- Bloquear merges quando não passa
- Validar cobertura mínima de 80% (tanto new code quanto overall)
- Validar que não há erros graves
- Validar que não há Security Hotspots críticos não resolvidos
- Permitir merge apenas quando todos os critérios são atendidos

## Como testar
- Criar um PR com código que não atende Quality Gate (cobertura < 80%)
- Verificar que o pipeline falha e bloqueia merge
- Corrigir código para atender Quality Gate
- Verificar que o pipeline passa e permite merge
- Validar que cobertura mínima é verificada
- Validar que erros graves bloqueiam merge

## Critérios de aceite
- [ ] Quality Gate configurado corretamente no Sonar Cloud
- [ ] Cobertura mínima configurada (80%)
- [ ] Cobertura de new code configurada (80%)
- [ ] Erros graves bloqueiam Quality Gate
- [ ] Security Rating bloqueia se pior que A
- [ ] Reliability Rating bloqueia se pior que A
- [ ] Pipeline bloqueia merge quando Quality Gate falha
- [ ] Pipeline permite merge quando Quality Gate passa
- [ ] Configuração do Quality Gate documentada
- [ ] Cenários de falha e sucesso testados

## Troubleshooting

### Problema: Quality Gate passa mesmo com cobertura baixa

**Causa**: Não há condições de cobertura configuradas no Quality Gate.

**Solução**: 
1. Verifique se as condições de cobertura estão configuradas
2. Verifique se a ação está definida como `Error` (não `Warning`)
3. Verifique se o Quality Gate correto está aplicado ao projeto

### Problema: Quality Gate não bloqueia merges

**Causa**: O workflow não está verificando o status do Quality Gate.

**Solução**: 
- Adicione `sonar.qualitygate.wait=true` no Sonar Begin
- Configure o GitHub para bloquear merges quando o status check falha
- Use a integração do SonarCloud com GitHub (Status Checks)

### Problema: Cobertura não está sendo calculada para new code

**Causa**: O SonarCloud precisa de uma análise anterior para comparar.

**Solução**: 
- Execute uma análise completa na branch main primeiro
- Aguarde algumas análises para o SonarCloud estabelecer baseline
- Verifique se está usando `sonar.pullrequest.*` corretamente para PRs

## Referências

- [SonarCloud Quality Gates Documentation](https://docs.sonarcloud.io/user-guide/quality-gates/)
- [Configuring Quality Gates](https://docs.sonarcloud.io/user-guide/quality-gates/#configuring-quality-gates)
