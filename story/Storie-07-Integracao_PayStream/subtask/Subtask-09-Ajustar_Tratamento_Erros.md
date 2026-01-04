# Subtask 09: Ajustar Tratamento de Erros no OrderController

## Objetivo
Ajustar o endpoint `ConfirmSelection` no `OrderController` para tratar adequadamente os erros de integração com PayStream, retornando 502 Bad Gateway quando o PayStream falhar.

## Arquivo a Modificar

### `src/InterfacesExternas/FastFood.OrderHub.Api/Controllers/OrderController.cs`

Modificar o método `ConfirmSelection`:

```csharp
/// <summary>
/// Confirmar seleção do pedido
/// </summary>
[Authorize(AuthenticationSchemes = "CustomerBearer", Policy = "Customer")]
[HttpPost("{id:guid}/confirm-selection")]
[ProducesResponseType(typeof(ApiResponse<ConfirmOrderSelectionResponse>), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ApiResponse<ConfirmOrderSelectionResponse>), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ApiResponse<ConfirmOrderSelectionResponse>), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ApiResponse<ConfirmOrderSelectionResponse>), StatusCodes.Status502BadGateway)]
public async Task<IActionResult> ConfirmSelection(Guid id)
{
    try
    {
        var input = new ConfirmOrderSelectionInputModel { OrderId = id };
        var response = await confirmOrderSelectionUseCase.ExecuteAsync(input);
        return Ok(ApiResponse<ConfirmOrderSelectionResponse>.Ok(response, "Seleção do pedido confirmada com sucesso."));
    }
    catch (BusinessException ex)
    {
        // Verificar se é erro de integração com PayStream
        if (ex.Message.Contains("pagamento") || ex.Message.Contains("PayStream"))
        {
            // Retornar 502 Bad Gateway para erros de serviço externo
            return StatusCode(
                StatusCodes.Status502BadGateway,
                ApiResponse<ConfirmOrderSelectionResponse>.Fail(
                    "O pedido foi finalizado, mas ocorreu um erro ao iniciar o pagamento. Tente novamente mais tarde."));
        }

        // Erros de negócio normais (pedido não encontrado, status inválido, etc.)
        if (ex.Message.Contains("não encontrado"))
            return NotFound(ApiResponse<ConfirmOrderSelectionResponse>.Fail(ex.Message));
        
        return BadRequest(ApiResponse<ConfirmOrderSelectionResponse>.Fail(ex.Message));
    }
    catch (Exception ex)
    {
        // Logar erro inesperado
        _logger.LogError(ex, "Erro inesperado ao confirmar seleção do pedido {OrderId}", id);
        
        return StatusCode(
            StatusCodes.Status500InternalServerError,
            ApiResponse<ConfirmOrderSelectionResponse>.Fail(
                "Ocorreu um erro inesperado. Tente novamente mais tarde."));
    }
}
```

**Nota:** Se o controller já tiver um `ILogger`, usar. Caso contrário, adicionar:

```csharp
private readonly ILogger<OrderController> _logger;

public OrderController(
    // ... outras dependências ...
    ILogger<OrderController> logger)
{
    // ... inicializações ...
    _logger = logger;
}
```

## Validações

- [ ] Método `ConfirmSelection` trata `BusinessException` relacionada a pagamento
- [ ] Retorna 502 Bad Gateway quando PayStream falha
- [ ] Mensagem de erro é genérica (não expõe detalhes técnicos)
- [ ] Erros de negócio normais continuam retornando 400/404
- [ ] Erros inesperados são logados e retornam 500
- [ ] `ProducesResponseType` inclui 502 Bad Gateway
- [ ] Código compila sem erros

## Observações

- 502 Bad Gateway é apropriado para erros de serviço externo (PayStream)
- Mensagem de erro deve ser amigável ao usuário
- Detalhes técnicos devem estar apenas nos logs
- Pedido já está finalizado quando chega neste tratamento de erro
