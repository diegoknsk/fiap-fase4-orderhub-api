# Subtask 07: Criar projeto Migrator básico

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** [DD/MM/AAAA]

## Descrição
Criar o projeto Migrator como Console Application .NET 8, que será usado futuramente para criar e configurar tabelas do DynamoDB. Por enquanto, apenas criar a estrutura básica com um Console.WriteLine simples para validar que o projeto funciona.

## Passos de implementação
- [ ] Criar projeto `FastFood.OrderHub.Migrator` em `src/InterfacesExternas/FastFood.OrderHub.Migrator/` como Console Application .NET 8
- [ ] Adicionar projeto à solução usando `dotnet sln add`
- [ ] Implementar `Program.cs` com um Console.WriteLine simples mostrando "migrator pedidos"
- [ ] Verificar compilação com `dotnet build`
- [ ] Testar execução do projeto com `dotnet run`

## Como testar
- Executar `dotnet build FastFood.OrderHub.sln` (deve compilar sem erros)
- Executar `dotnet sln list` e verificar que o projeto Migrator aparece na lista
- Executar `dotnet run --project src/InterfacesExternas/FastFood.OrderHub.Migrator/`
- Verificar que a saída mostra "migrator pedidos" no console
- Verificar que o projeto compila e executa sem erros

## Critérios de aceite
- [ ] Projeto `FastFood.OrderHub.Migrator` criado e compilando
- [ ] Projeto adicionado à solução
- [ ] `Program.cs` implementado com Console.WriteLine("migrator pedidos")
- [ ] `dotnet build` executa sem erros
- [ ] `dotnet run` executa e mostra "migrator pedidos" no console
- [ ] Projeto pronto para futuras implementações de migração DynamoDB




