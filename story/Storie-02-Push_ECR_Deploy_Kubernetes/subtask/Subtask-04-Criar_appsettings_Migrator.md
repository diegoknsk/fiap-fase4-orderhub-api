# Subtask 04: Criar appsettings.json do Migrator

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** [DD/MM/AAAA]

## Descrição
Criar arquivo `appsettings.json` no projeto Migrator. Este arquivo é **OBRIGATÓRIO** para o Dockerfile funcionar corretamente, pois ele copia este arquivo para o container.

## Passos de implementação
- [ ] Criar arquivo `appsettings.json` no diretório `src/InterfacesExternas/FastFood.OrderHub.Migrator/`
- [ ] Configurar seção `Logging` com níveis de log:
  - `Default`: `Information`
  - `Microsoft.AspNetCore`: `Warning`
- [ ] Adicionar estrutura básica do JSON
- [ ] Validar sintaxe JSON

## Estrutura esperada

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

## Como testar
- Validar que o arquivo existe em `src/InterfacesExternas/FastFood.OrderHub.Migrator/appsettings.json`
- Validar sintaxe JSON (sem erros de parsing)
- Executar build do Dockerfile e validar que o arquivo é copiado corretamente
- Verificar que o arquivo está presente no container após build

## Critérios de aceite
- [ ] Arquivo `appsettings.json` criado em `src/InterfacesExternas/FastFood.OrderHub.Migrator/`
- [ ] Arquivo contém seção `Logging` configurada
- [ ] Sintaxe JSON válida
- [ ] Arquivo é copiado corretamente pelo Dockerfile
- [ ] Arquivo presente no container após build


