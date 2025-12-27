# Subtask 03: Criar Health Check Endpoint

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** [DD/MM/AAAA]

## Descrição
Criar endpoint `/health` na API OrderHub para permitir que o Kubernetes realize health checks da aplicação. Este endpoint é **OBRIGATÓRIO** para o funcionamento correto dos health checks do Kubernetes.

## Passos de implementação
- [ ] Criar arquivo `HealthController.cs` no diretório `src/InterfacesExternas/FastFood.OrderHub.Api/Controllers/`
- [ ] Implementar controller com namespace `FastFood.OrderHub.Api.Controllers`
- [ ] Adicionar atributo `[ApiController]`
- [ ] Adicionar rota `[Route("[controller]")]` (resultará em `/health`)
- [ ] Criar método `Get()` com atributo `[HttpGet]`
- [ ] Retornar `IActionResult` com status `200 OK`
- [ ] Retornar JSON com estrutura:
  ```json
  {
    "status": "healthy",
    "timestamp": "2025-01-01T00:00:00Z"
  }
  ```
- [ ] Usar `DateTime.UtcNow` para o timestamp

## Como testar
- Executar a API localmente: `dotnet run --project src/InterfacesExternas/FastFood.OrderHub.Api`
- Fazer requisição GET para `http://localhost:5000/health` (ou porta configurada)
- Validar que retorna status HTTP 200
- Validar que o JSON retornado contém `status: "healthy"` e `timestamp`
- Testar via curl: `curl http://localhost:5000/health`
- Validar que o endpoint funciona após deploy no Kubernetes

## Critérios de aceite
- [ ] Arquivo `HealthController.cs` criado em `src/InterfacesExternas/FastFood.OrderHub.Api/Controllers/`
- [ ] Controller implementado com `[ApiController]` e `[Route("[controller]")]`
- [ ] Endpoint `/health` respondendo com status HTTP 200
- [ ] Resposta JSON contém `status: "healthy"`
- [ ] Resposta JSON contém `timestamp` em formato ISO 8601
- [ ] Endpoint testado localmente e funcionando
- [ ] Endpoint acessível após deploy no Kubernetes

