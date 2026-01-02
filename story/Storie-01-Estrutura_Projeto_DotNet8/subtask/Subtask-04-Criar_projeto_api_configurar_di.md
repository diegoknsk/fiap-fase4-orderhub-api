# Subtask 04: Criar projeto API e configurar Dependency Injection

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** [DD/MM/AAAA]

## Descrição
Criar o projeto ASP.NET Core Web API, configurar Dependency Injection básica, e preparar a estrutura para receber controllers e endpoints. Adicionar o projeto à solução.

## Passos de implementação
- [ ] Criar projeto `FastFood.OrderHub.Api` em `src/InterfacesExternas/FastFood.OrderHub.Api/` como Web API .NET 8
- [ ] Adicionar referência da API para Application e CrossCutting
- [ ] Adicionar projeto à solução usando `dotnet sln add`
- [ ] Configurar `Program.cs` com builder padrão do ASP.NET Core
- [ ] Criar diretório `Controllers/` para futuros controllers
- [ ] Configurar serviços básicos no `Program.cs` (AddControllers, AddEndpointsApiExplorer)
- [ ] Configurar appsettings.json com configurações básicas
- [ ] Verificar compilação com `dotnet build`

## Como testar
- Executar `dotnet build FastFood.OrderHub.sln` (deve compilar sem erros)
- Executar `dotnet sln list` e verificar que o projeto API aparece na lista
- Verificar que `FastFood.OrderHub.Api.csproj` tem referências a Application e CrossCutting
- Verificar que `Program.cs` existe e tem configuração básica do ASP.NET Core
- Executar `dotnet run --project src/InterfacesExternas/FastFood.OrderHub.Api/` (deve iniciar sem erros, mesmo que não tenha endpoints ainda)

## Critérios de aceite
- [ ] Projeto `FastFood.OrderHub.Api` criado e compilando
- [ ] Referências para Application e CrossCutting configuradas
- [ ] Projeto adicionado à solução
- [ ] `Program.cs` configurado com builder padrão
- [ ] Diretório `Controllers/` criado
- [ ] `appsettings.json` criado com configurações básicas
- [ ] `dotnet build` executa sem erros
- [ ] API inicia sem erros (mesmo sem endpoints)




