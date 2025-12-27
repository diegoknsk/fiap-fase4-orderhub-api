# Subtask 02: Criar Dockerfile para Migrator

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** [DD/MM/AAAA]

## Descrição
Criar Dockerfile multi-stage para o projeto Migrator, garantindo que o job de manutenção do DynamoDB possa ser executado como container Docker. **CRÍTICO**: Implementar tratamento para pasta `Migrations/` vazia, criando arquivo `.keep` se necessário.

## Passos de implementação
- [ ] Criar arquivo `Dockerfile` no diretório `src/InterfacesExternas/FastFood.OrderHub.Migrator/`
- [ ] Implementar estágio de build usando `mcr.microsoft.com/dotnet/sdk:8.0`
- [ ] Configurar WORKDIR e copiar arquivos .csproj primeiro (otimização de cache):
  - `src/Core/FastFood.OrderHub.Domain/FastFood.OrderHub.Domain.csproj`
  - `src/Core/FastFood.OrderHub.Application/FastFood.OrderHub.Application.csproj`
  - `src/Core/FastFood.OrderHub.CrossCutting/FastFood.OrderHub.CrossCutting.csproj`
  - `src/Infra/FastFood.OrderHub.Infra/FastFood.OrderHub.Infra.csproj`
  - `src/Infra/FastFood.OrderHub.Infra.Persistence/FastFood.OrderHub.Infra.Persistence.csproj`
  - `src/InterfacesExternas/FastFood.OrderHub.Migrator/FastFood.OrderHub.Migrator.csproj`
- [ ] Executar `dotnet restore` no projeto Migrator
- [ ] Copiar todo o código fonte
- [ ] Executar `dotnet publish` com configurações de Release e flags de otimização
- [ ] **CRÍTICO**: Preparar migrações no build stage:
  - Criar pasta `/migrations` temporária
  - Copiar migrações de `src/Infra/FastFood.OrderHub.Infra.Persistence/Migrations` se existirem
  - Se a pasta estiver vazia, criar arquivo `.keep` para garantir que não esteja vazia
- [ ] Implementar estágio de runtime usando `mcr.microsoft.com/dotnet/aspnet:8.0`
- [ ] Copiar artefatos publicados do estágio de build
- [ ] Copiar `appsettings.json` do Migrator
- [ ] Copiar migrações do build stage (sempre terá pelo menos o arquivo `.keep`)
- [ ] Configurar ENTRYPOINT para executar `FastFood.OrderHub.Migrator.dll`

## Como testar
- Executar `docker build -t orderhub-migrator -f src/InterfacesExternas/FastFood.OrderHub.Migrator/Dockerfile .` na raiz do projeto (deve completar sem erros mesmo sem migrações)
- **Testar cenário sem migrações**: Remover/renomear pasta `Migrations/` e validar que o build ainda funciona
- Verificar tamanho da imagem com `docker images orderhub-migrator`
- Executar container localmente com `docker run --env-file .env orderhub-migrator` (com credenciais AWS configuradas)
- Validar que o migrator executa e finaliza com sucesso
- Verificar logs do container para confirmar execução correta
- Validar que a pasta `Migrations/` existe no container (mesmo que apenas com `.keep`)

## Critérios de aceite
- [ ] Arquivo `Dockerfile` criado em `src/InterfacesExternas/FastFood.OrderHub.Migrator/`
- [ ] Dockerfile usa multi-stage build (build + runtime)
- [ ] Build da imagem completa sem erros **mesmo quando a pasta Migrations está vazia**
- [ ] Tratamento de pasta Migrations vazia implementado (criação de `.keep`)
- [ ] Imagem final baseada em `mcr.microsoft.com/dotnet/aspnet:8.0`
- [ ] Container executa o migrator corretamente
- [ ] appsettings.json copiado corretamente
- [ ] Pasta Migrations sempre presente no container (mesmo que apenas com `.keep`)
- [ ] Tamanho da imagem otimizado


