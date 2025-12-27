# Subtask 05: Implementar rota "olá mundo" e configurar Swagger

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** [DD/MM/AAAA]

## Descrição
Criar um controller simples com uma rota GET `/api/hello` que retorna uma mensagem "olá mundo" em JSON. Configurar Swagger/OpenAPI para documentação da API e validação do funcionamento básico.

## Passos de implementação
- [ ] Instalar pacote NuGet `Swashbuckle.AspNetCore` no projeto API
- [ ] Criar controller `HelloController` em `Controllers/HelloController.cs`
- [ ] Implementar endpoint GET `/api/hello` que retorna JSON com mensagem
- [ ] Configurar Swagger no `Program.cs` (AddSwaggerGen, UseSwagger, UseSwaggerUI)
- [ ] Configurar roteamento de API controllers no `Program.cs`
- [ ] Adicionar XML comments no controller para documentação Swagger
- [ ] Testar execução local da API e acesso ao Swagger
- [ ] Validar resposta da rota `/api/hello` via Swagger ou navegador

## Como testar
- Executar `dotnet run --project src/InterfacesExternas/FastFood.OrderHub.Api/`
- Acessar `https://localhost:5001/swagger` ou `http://localhost:5000/swagger` (portas podem variar)
- Verificar que Swagger UI carrega e mostra o endpoint `/api/hello`
- Testar endpoint via Swagger UI (clicar em "Try it out" e "Execute")
- Testar endpoint diretamente via navegador: `http://localhost:5000/api/hello`
- Verificar que a resposta é JSON válido com mensagem "olá mundo"
- Executar `dotnet build` para garantir que compila sem erros

## Critérios de aceite
- [ ] Pacote `Swashbuckle.AspNetCore` instalado
- [ ] Controller `HelloController` criado com endpoint GET `/api/hello`
- [ ] Endpoint retorna JSON válido com mensagem "olá mundo"
- [ ] Swagger configurado e acessível em `/swagger`
- [ ] Endpoint aparece na documentação Swagger
- [ ] Teste manual via Swagger UI funciona corretamente
- [ ] Teste direto via navegador funciona corretamente
- [ ] `dotnet build` executa sem erros


