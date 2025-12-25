# Subtask 02: Criar Dockerfile para Migrator

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** [DD/MM/AAAA]

## Descrição
Criar Dockerfile multi-stage para o projeto Migrator, garantindo que o job de manutenção do DynamoDB possa ser executado como container Docker.

## Passos de implementação
- [ ] Criar arquivo `Dockerfile` no diretório `src/InterfacesExternas/FastFood.OrderHub.Migrator/`
- [ ] Implementar estágio de build usando `mcr.microsoft.com/dotnet/sdk:8.0`
- [ ] Configurar WORKDIR e copiar arquivos do projeto
- [ ] Executar `dotnet restore` para restaurar dependências
- [ ] Executar `dotnet publish` com configurações de Release
- [ ] Implementar estágio de runtime usando `mcr.microsoft.com/dotnet/aspnet:8.0` ou `mcr.microsoft.com/dotnet/runtime:8.0`
- [ ] Copiar artefatos publicados do estágio de build
- [ ] Configurar variáveis de ambiente necessárias (AWS_ACCESS_KEY_ID, AWS_SECRET_ACCESS_KEY, AWS_REGION)
- [ ] Configurar ENTRYPOINT para executar o migrator

## Como testar
- Executar `docker build -t orderhub-migrator -f src/InterfacesExternas/FastFood.OrderHub.Migrator/Dockerfile .` na raiz do projeto (deve completar sem erros)
- Verificar tamanho da imagem com `docker images orderhub-migrator`
- Executar container localmente com `docker run --env-file .env orderhub-migrator` (com credenciais AWS configuradas)
- Validar que o migrator executa e finaliza com sucesso
- Verificar logs do container para confirmar execução correta

## Critérios de aceite
- [ ] Arquivo `Dockerfile` criado em `src/InterfacesExternas/FastFood.OrderHub.Migrator/`
- [ ] Dockerfile usa multi-stage build (build + runtime)
- [ ] Build da imagem completa sem erros
- [ ] Imagem final baseada em runtime .NET 8
- [ ] Container executa o migrator corretamente
- [ ] Variáveis de ambiente AWS configuráveis via docker run
- [ ] Tamanho da imagem otimizado

