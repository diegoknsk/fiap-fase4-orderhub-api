# Subtask 06: Implementar AuthorizeBySchemeOperationFilter

## Objetivo
Implementar filtro para Swagger que detecta automaticamente os esquemas de autenticação dos atributos `[Authorize]` e adiciona os requisitos de segurança corretos na documentação OpenAPI.

## Arquivo Criado na Subtask 05

O arquivo `AuthorizeBySchemeOperationFilter.cs` já foi criado na Subtask 05. Esta subtask é apenas para validar que está funcionando corretamente.

## Funcionamento

O filtro:
1. Detecta atributos `[Authorize]` nos métodos e classes
2. Extrai os esquemas de autenticação do atributo `AuthenticationSchemes`
3. Adiciona respostas 401 e 403 na documentação
4. Adiciona requisitos de segurança baseados nos esquemas detectados

## Validações
- [ ] Filtro implementado corretamente
- [ ] Filtro registrado no AddSwaggerGen
- [ ] Swagger UI mostra botões de autorização corretos
- [ ] Endpoints com `[Authorize]` aparecem com requisitos de segurança
- [ ] Endpoints sem `[Authorize]` não aparecem com requisitos de segurança

## Teste Manual

1. Abrir Swagger UI
2. Verificar que endpoints com `[Authorize(AuthenticationSchemes = "Cognito")]` mostram botão "Authorize" com esquema Cognito
3. Verificar que endpoints com `[Authorize(AuthenticationSchemes = "CustomerBearer")]` mostram botão "Authorize" com esquema CustomerBearer
4. Verificar que respostas 401 e 403 estão documentadas

