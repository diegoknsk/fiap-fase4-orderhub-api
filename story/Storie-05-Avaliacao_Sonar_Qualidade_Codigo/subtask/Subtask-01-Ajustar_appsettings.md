# Subtask 01: Ajustar appsettings.json e criar appsettings.Development.json

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** [DD/MM/AAAA]

## Descrição
Ajustar os arquivos `appsettings.json` para remover valores sensíveis, deixando apenas a estrutura vazia. Criar arquivos `appsettings.Development.json` com valores de desenvolvimento e adicionar ao `.gitignore` para não versionar.

## Passos de implementação
- [ ] Limpar valores sensíveis de `src/InterfacesExternas/FastFood.OrderHub.Api/appsettings.json`
- [ ] Limpar valores sensíveis de `src/InterfacesExternas/FastFood.OrderHub.Migrator/appsettings.json`
- [ ] Criar `src/InterfacesExternas/FastFood.OrderHub.Api/appsettings.Development.json` com valores de desenvolvimento
- [ ] Criar `src/InterfacesExternas/FastFood.OrderHub.Migrator/appsettings.Development.json` com valores de desenvolvimento
- [ ] Adicionar `**/appsettings.Development.json` ao `.gitignore`
- [ ] Verificar que os arquivos Development não são versionados

## Estrutura esperada

### appsettings.json (API)
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "DynamoDb": {
    "AccessKey": "",
    "SecretKey": "",
    "SessionToken": "",
    "Region": "us-east-1",
    "ServiceUrl": ""
  },
  "Authentication": {
    "Cognito": {
      "Region": "us-east-1",
      "UserPoolId": "",
      "ClientId": "",
      "ClockSkewMinutes": 5
    }
  },
  "JwtCustomer": {
    "Issuer": "FastFood.Auth",
    "Audience": "FastFood.API",
    "SecretKey": ""
  }
}
```

### appsettings.Development.json (API)
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "DynamoDb": {
    "AccessKey": "local",
    "SecretKey": "local",
    "SessionToken": "",
    "Region": "us-east-1",
    "ServiceUrl": "http://localhost:8000"
  },
  "Authentication": {
    "Cognito": {
      "Region": "us-east-1",
      "UserPoolId": "us-east-1_dQGPDjWJR",
      "ClientId": "967cigqih5kvdbi53hsafbaru",
      "ClockSkewMinutes": 5
    }
  },
  "JwtCustomer": {
    "Issuer": "FastFood.Auth",
    "Audience": "FastFood.API",
    "SecretKey": "your-super-secret-key-minimum-32-characters-long-for-hmac-sha256-development"
  }
}
```

### appsettings.json (Migrator)
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "DynamoDb": {
    "AccessKey": "",
    "SecretKey": "",
    "SessionToken": "",
    "Region": "us-east-1",
    "ServiceUrl": ""
  }
}
```

### appsettings.Development.json (Migrator)
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "DynamoDb": {
    "AccessKey": "local",
    "SecretKey": "local",
    "SessionToken": "",
    "Region": "us-east-1",
    "ServiceUrl": "http://localhost:8000"
  }
}
```

## Como testar
- Executar `git status` e verificar que `appsettings.Development.json` não aparece nos arquivos rastreados
- Executar `dotnet build` e verificar que compila sem erros
- Executar a API localmente e verificar que carrega valores do `appsettings.Development.json`
- Verificar que `appsettings.json` não contém valores sensíveis no repositório

## Critérios de aceite
- [ ] `appsettings.json` (API) não contém valores sensíveis
- [ ] `appsettings.json` (Migrator) não contém valores sensíveis
- [ ] `appsettings.Development.json` (API) criado com valores de desenvolvimento
- [ ] `appsettings.Development.json` (Migrator) criado com valores de desenvolvimento
- [ ] `**/appsettings.Development.json` adicionado ao `.gitignore`
- [ ] Arquivos Development não são versionados (verificado com `git status`)
- [ ] Aplicação compila e executa corretamente usando Development.json localmente
