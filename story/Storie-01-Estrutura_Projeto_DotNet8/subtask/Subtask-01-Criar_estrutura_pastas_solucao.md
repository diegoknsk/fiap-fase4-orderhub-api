# Subtask 01: Criar estrutura de pastas e solução .NET

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** [DD/MM/AAAA]

## Descrição
Criar a estrutura de diretórios conforme definido nas regras do projeto e criar o arquivo de solução (.sln) do .NET 8 para organizar todos os projetos que serão criados nas próximas subtasks.

## Passos de implementação
- [ ] Criar diretório `src/Core/` para projetos core
- [ ] Criar diretório `src/InterfacesExternas/` para projetos de interfaces externas
- [ ] Criar diretório `src/tests/` para projetos de testes
- [ ] Criar arquivo de solução `FastFood.OrderHub.sln` na raiz do projeto
- [ ] Verificar que a solução foi criada corretamente com `dotnet sln list`

## Como testar
- Executar `dotnet sln FastFood.OrderHub.sln list` (deve retornar lista vazia inicialmente)
- Verificar que os diretórios `src/Core/`, `src/InterfacesExternas/` e `src/tests/` existem
- Verificar que o arquivo `FastFood.OrderHub.sln` existe na raiz do projeto
- Executar `dotnet --version` para confirmar que .NET 8 está instalado

## Critérios de aceite
- [ ] Diretório `src/Core/` criado
- [ ] Diretório `src/InterfacesExternas/` criado
- [ ] Diretório `src/tests/` criado
- [ ] Arquivo `FastFood.OrderHub.sln` criado na raiz
- [ ] Comando `dotnet sln list` executa sem erros
- [ ] Estrutura de pastas segue o padrão definido em `orderhub-context.mdc`

