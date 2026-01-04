# Subtask 11: Testes Unitários PaymentServiceClient

## Objetivo
Criar testes unitários para a classe `PaymentServiceClient`, validando chamadas HTTP, tratamento de erros e repasse do token Bearer.

## Arquivo a Criar

### `src/tests/FastFood.OrderHub.Tests.Unit/Integrations/PaymentServiceClientTests.cs`

```csharp
using FastFood.OrderHub.Application.DTOs.Payment;
using FastFood.OrderHub.Application.Ports;
using FastFood.OrderHub.Infra.Configurations;
using FastFood.OrderHub.Infra.Integrations;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Xunit;

namespace FastFood.OrderHub.Tests.Unit.Integrations;

public class PaymentServiceClientTests
{
    private readonly Mock<IOptions<PaymentServiceOptions>> _optionsMock;
    private readonly Mock<ILogger<PaymentServiceClient>> _loggerMock;
    private readonly PaymentServiceOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;

    public PaymentServiceClientTests()
    {
        _options = new PaymentServiceOptions
        {
            BaseUrl = "http://test-paystream:8080",
            TimeoutSeconds = 30
        };

        _optionsMock = new Mock<IOptions<PaymentServiceOptions>>();
        _optionsMock.Setup(x => x.Value).Returns(_options);

        _loggerMock = new Mock<ILogger<PaymentServiceClient>>();

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    [Fact]
    public async Task CreatePaymentAsync_WhenRequestIsValid_ShouldReturnPaymentResponse()
    {
        // Arrange
        var expectedResponse = new CreatePaymentResponse
        {
            PaymentId = Guid.NewGuid(),
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        var httpClient = CreateHttpClient(HttpStatusCode.OK, expectedResponse);
        var client = new PaymentServiceClient(httpClient, _optionsMock.Object, _loggerMock.Object);

        var request = new CreatePaymentRequest
        {
            OrderId = Guid.NewGuid(),
            TotalAmount = 50.00m,
            OrderSnapshot = CreateValidSnapshot()
        };

        var bearerToken = "test-token-123";

        // Act
        var result = await client.CreatePaymentAsync(request, bearerToken);

        // Assert
        result.Should().NotBeNull();
        result.PaymentId.Should().Be(expectedResponse.PaymentId);
        result.Status.Should().Be(expectedResponse.Status);
    }

    [Fact]
    public async Task CreatePaymentAsync_WhenRequestIsValid_ShouldIncludeBearerTokenInHeader()
    {
        // Arrange
        var httpClient = CreateHttpClient(HttpStatusCode.OK, new CreatePaymentResponse());
        var client = new PaymentServiceClient(httpClient, _optionsMock.Object, _loggerMock.Object);

        var request = new CreatePaymentRequest
        {
            OrderId = Guid.NewGuid(),
            TotalAmount = 50.00m,
            OrderSnapshot = CreateValidSnapshot()
        };

        var bearerToken = "test-token-123";

        // Act
        await client.CreatePaymentAsync(request, bearerToken);

        // Assert
        httpClient.DefaultRequestHeaders.Authorization.Should().BeNull(); // Limpo no finally
    }

    [Fact]
    public async Task CreatePaymentAsync_WhenRequestIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var httpClient = CreateHttpClient(HttpStatusCode.OK, new CreatePaymentResponse());
        var client = new PaymentServiceClient(httpClient, _optionsMock.Object, _loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => client.CreatePaymentAsync(null!, "token"));
    }

    [Fact]
    public async Task CreatePaymentAsync_WhenBearerTokenIsEmpty_ShouldThrowArgumentException()
    {
        // Arrange
        var httpClient = CreateHttpClient(HttpStatusCode.OK, new CreatePaymentResponse());
        var client = new PaymentServiceClient(httpClient, _optionsMock.Object, _loggerMock.Object);

        var request = new CreatePaymentRequest
        {
            OrderId = Guid.NewGuid(),
            TotalAmount = 50.00m,
            OrderSnapshot = CreateValidSnapshot()
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => client.CreatePaymentAsync(request, string.Empty));
    }

    [Fact]
    public async Task CreatePaymentAsync_WhenPayStreamReturns400_ShouldThrowHttpRequestException()
    {
        // Arrange
        var httpClient = CreateHttpClient(HttpStatusCode.BadRequest, null);
        var client = new PaymentServiceClient(httpClient, _optionsMock.Object, _loggerMock.Object);

        var request = new CreatePaymentRequest
        {
            OrderId = Guid.NewGuid(),
            TotalAmount = 50.00m,
            OrderSnapshot = CreateValidSnapshot()
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => client.CreatePaymentAsync(request, "token"));
        
        exception.Data["StatusCode"].Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreatePaymentAsync_WhenPayStreamReturns500_ShouldThrowHttpRequestException()
    {
        // Arrange
        var httpClient = CreateHttpClient(HttpStatusCode.InternalServerError, null);
        var client = new PaymentServiceClient(httpClient, _optionsMock.Object, _loggerMock.Object);

        var request = new CreatePaymentRequest
        {
            OrderId = Guid.NewGuid(),
            TotalAmount = 50.00m,
            OrderSnapshot = CreateValidSnapshot()
        };

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            () => client.CreatePaymentAsync(request, "token"));
    }

    [Fact]
    public async Task CreatePaymentAsync_WhenResponseIsNull_ShouldThrowHttpRequestException()
    {
        // Arrange
        var httpClient = CreateHttpClient(HttpStatusCode.OK, null);
        var client = new PaymentServiceClient(httpClient, _optionsMock.Object, _loggerMock.Object);

        var request = new CreatePaymentRequest
        {
            OrderId = Guid.NewGuid(),
            TotalAmount = 50.00m,
            OrderSnapshot = CreateValidSnapshot()
        };

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            () => client.CreatePaymentAsync(request, "token"));
    }

    private HttpClient CreateHttpClient(HttpStatusCode statusCode, object? responseContent)
    {
        var handler = new Mock<HttpMessageHandler>();
        
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = responseContent != null
                    ? new StringContent(JsonSerializer.Serialize(responseContent, _jsonOptions), Encoding.UTF8, "application/json")
                    : new StringContent("Error", Encoding.UTF8, "application/json")
            });

        return new HttpClient(handler.Object)
        {
            BaseAddress = new Uri(_options.BaseUrl)
        };
    }

    private OrderSnapshot CreateValidSnapshot()
    {
        return new OrderSnapshot
        {
            Order = new OrderInfo
            {
                OrderId = Guid.NewGuid(),
                Code = "ORD-001",
                CreatedAt = DateTime.UtcNow
            },
            Pricing = new PricingInfo
            {
                TotalPrice = 50.00m,
                Currency = "BRL"
            },
            Items = new List<ItemInfo>
            {
                new ItemInfo
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Hambúrguer",
                    Quantity = 1,
                    FinalPrice = 50.00m
                }
            },
            Version = 1
        };
    }
}
```

## Validações

- [ ] Teste: CreatePaymentAsync retorna PaymentResponse quando sucesso
- [ ] Teste: CreatePaymentAsync inclui Bearer Token no header
- [ ] Teste: CreatePaymentAsync valida request não null
- [ ] Teste: CreatePaymentAsync valida bearerToken não vazio
- [ ] Teste: CreatePaymentAsync trata erro 400
- [ ] Teste: CreatePaymentAsync trata erro 500
- [ ] Teste: CreatePaymentAsync trata response null
- [ ] Todos os testes passam
- [ ] Código compila sem erros

## Observações

- Usar `Moq.Protected` para mockar `HttpMessageHandler`
- Validar que token Bearer é incluído no header
- Testar todos os cenários de erro HTTP
- Mockar `ILogger` para evitar logs nos testes
