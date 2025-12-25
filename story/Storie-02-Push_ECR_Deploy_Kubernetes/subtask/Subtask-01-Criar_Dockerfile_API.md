# Subtask 01: Criar Dockerfile para API

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** [DD/MM/AAAA]

## Descrição
Criar Dockerfile multi-stage para a API OrderHub, otimizando o tamanho da imagem final e garantindo que a aplicação .NET 8 seja executada corretamente em container Docker.

## Passos de implementação
- [ ] Criar arquivo `Dockerfile` no diretório `src/InterfacesExternas/FastFood.OrderHub.Api/`
- [ ] Implementar estágio de build usando `mcr.microsoft.com/dotnet/sdk:8.0`
- [ ] Configurar WORKDIR e copiar arquivos do projeto
- [ ] Executar `dotnet restore` para restaurar dependências
- [ ] Executar `dotnet publish` com configurações de Release
- [ ] Implementar estágio de runtime usando `mcr.microsoft.com/dotnet/aspnet:8.0`
- [ ] Copiar artefatos publicados do estágio de build
- [ ] Configurar variáveis de ambiente (ASPNETCORE_URLS, ASPNETCORE_ENVIRONMENT)
- [ ] Expor porta 80
- [ ] Configurar ENTRYPOINT para executar a aplicação

## Como testar
- Executar `docker build -t orderhub-api -f src/InterfacesExternas/FastFood.OrderHub.Api/Dockerfile .` na raiz do projeto (deve completar sem erros)
- Verificar tamanho da imagem com `docker images orderhub-api` (deve ser menor que 200MB)
- Executar container localmente com `docker run -p 8080:80 orderhub-api`
- Validar que a API responde em `http://localhost:8080/api/hello`
- Verificar logs do container com `docker logs <container-id>`

## Critérios de aceite
- [ ] Arquivo `Dockerfile` criado em `src/InterfacesExternas/FastFood.OrderHub.Api/`
- [ ] Dockerfile usa multi-stage build (build + runtime)
- [ ] Build da imagem completa sem erros
- [ ] Imagem final baseada em `mcr.microsoft.com/dotnet/aspnet:8.0`
- [ ] Porta 80 exposta e configurada corretamente
- [ ] Container executa e API responde localmente
- [ ] Tamanho da imagem otimizado (sem SDK no estágio final)

