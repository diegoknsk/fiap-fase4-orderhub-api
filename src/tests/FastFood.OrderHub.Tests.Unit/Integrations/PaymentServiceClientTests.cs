using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using FastFood.OrderHub.Application.DTOs.Payment;
using FastFood.OrderHub.Application.Ports;
using FastFood.OrderHub.Infra.Configurations;
using FastFood.OrderHub.Infra.Integrations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace FastFood.OrderHub.Tests.Unit.Integrations;

/// <summary>
/// Testes unitários para PaymentServiceClient
/// </summary>
public class PaymentServiceClientTests
{
    private readonly Mock<IOptions<PaymentServiceOptions>> _optionsMock;
    private readonly Mock<ILogger<PaymentServiceClient>> _loggerMock;
    private readonly PaymentServiceOptions _options;

    public PaymentServiceClientTests()
    {
        _options = new PaymentServiceOptions
        {
            BaseUrl = "http://test-paystream.com",
            TimeoutSeconds = 30,
            RetryEnabled = false,
            RetryCount = 1
        };

        _optionsMock = new Mock<IOptions<PaymentServiceOptions>>();
        _optionsMock.Setup(x => x.Value).Returns(_options);
        _loggerMock = new Mock<ILogger<PaymentServiceClient>>();
    }

    [Fact]
    public async Task CreatePaymentAsync_WhenSuccess_ShouldReturnPaymentResponse()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var bearerToken = "test-bearer-token";

        var responseContent = new CreatePaymentResponse
        {
            PaymentId = paymentId,
            Status = "Created",
            CreatedAt = DateTime.UtcNow
        };

        var jsonResponse = JsonSerializer.Serialize(responseContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        var httpClient = CreateHttpClient(httpResponse);
        var client = new PaymentServiceClient(httpClient, _optionsMock.Object, _loggerMock.Object);

        var request = new CreatePaymentRequest
        {
            OrderId = orderId,
            TotalAmount = 50.00m,
            OrderSnapshot = new OrderSnapshot
            {
                Order = new OrderInfo
                {
                    OrderId = orderId,
                    Code = "ORD-001",
                    CreatedAt = DateTime.UtcNow
                },
                Pricing = new PricingInfo
                {
                    TotalPrice = 50.00m,
                    Currency = "BRL"
                },
                Items = new List<ItemInfo>(),
                Version = 1
            }
        };

        // Act
        var result = await client.CreatePaymentAsync(request, bearerToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(paymentId, result.PaymentId);
        Assert.Equal("Created", result.Status);
    }

    [Fact]
    public async Task CreatePaymentAsync_ShouldSerializeOrderSnapshotAsString()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var bearerToken = "test-bearer-token";

        var responseContent = new CreatePaymentResponse
        {
            PaymentId = Guid.NewGuid(),
            Status = "Created",
            CreatedAt = DateTime.UtcNow
        };

        var jsonResponse = JsonSerializer.Serialize(responseContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        string? capturedRequestBody = null;
        HttpRequestMessage? capturedRequest = null;
        
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, token) =>
            {
                capturedRequest = request;
            })
            .ReturnsAsync(httpResponse);

        var httpClient = new HttpClient(handler.Object)
        {
            BaseAddress = new Uri(_options.BaseUrl)
        };

        var client = new PaymentServiceClient(httpClient, _optionsMock.Object, _loggerMock.Object);

        var request = new CreatePaymentRequest
        {
            OrderId = orderId,
            TotalAmount = 50.00m,
            OrderSnapshot = new OrderSnapshot
            {
                Order = new OrderInfo
                {
                    OrderId = orderId,
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
                        Quantity = 2,
                        FinalPrice = 25.00m
                    }
                },
                Version = 1
            }
        };

        // Act
        await client.CreatePaymentAsync(request, bearerToken);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.NotNull(capturedRequest.Content);
        capturedRequestBody = await capturedRequest.Content.ReadAsStringAsync();
        Assert.NotNull(capturedRequestBody);
        
        // Verificar que orderSnapshot é uma string JSON, não um objeto
        var jsonDoc = JsonDocument.Parse(capturedRequestBody);
        var orderSnapshotElement = jsonDoc.RootElement.GetProperty("orderSnapshot");
        Assert.Equal(JsonValueKind.String, orderSnapshotElement.ValueKind);
        
        // Verificar que a string JSON pode ser deserializada
        var snapshotString = orderSnapshotElement.GetString();
        Assert.NotNull(snapshotString);
        var deserializedSnapshot = JsonSerializer.Deserialize<OrderSnapshot>(snapshotString!);
        Assert.NotNull(deserializedSnapshot);
        Assert.Equal(orderId, deserializedSnapshot.Order.OrderId);
    }

    [Fact]
    public async Task CreatePaymentAsync_ShouldIncludeBearerTokenInAuthorizationHeader()
    {
        // Arrange
        var bearerToken = "test-bearer-token-123";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(new CreatePaymentResponse
            {
                PaymentId = Guid.NewGuid(),
                Status = "Created",
                CreatedAt = DateTime.UtcNow
            }), Encoding.UTF8, "application/json")
        };

        string? capturedAuthHeader = null;
        HttpRequestMessage? capturedRequest = null;
        
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, token) =>
            {
                capturedRequest = request;
            })
            .ReturnsAsync(httpResponse);

        var httpClient = new HttpClient(handler.Object)
        {
            BaseAddress = new Uri(_options.BaseUrl)
        };

        var client = new PaymentServiceClient(httpClient, _optionsMock.Object, _loggerMock.Object);

        var request = new CreatePaymentRequest
        {
            OrderId = Guid.NewGuid(),
            TotalAmount = 50.00m,
            OrderSnapshot = new OrderSnapshot
            {
                Order = new OrderInfo { OrderId = Guid.NewGuid(), Code = "ORD-001", CreatedAt = DateTime.UtcNow },
                Pricing = new PricingInfo { TotalPrice = 50.00m, Currency = "BRL" },
                Items = new List<ItemInfo>(),
                Version = 1
            }
        };

        // Act
        await client.CreatePaymentAsync(request, bearerToken);

        // Assert
        Assert.NotNull(capturedRequest);
        capturedAuthHeader = capturedRequest.Headers.Authorization?.ToString();
        Assert.NotNull(capturedAuthHeader);
        Assert.Equal($"Bearer {bearerToken}", capturedAuthHeader);
    }

    [Fact]
    public async Task CreatePaymentAsync_WhenBadRequest_ShouldThrowHttpRequestException()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("{\"error\":\"Invalid request\"}", Encoding.UTF8, "application/json")
        };

        var httpClient = CreateHttpClient(httpResponse);
        var client = new PaymentServiceClient(httpClient, _optionsMock.Object, _loggerMock.Object);

        var request = new CreatePaymentRequest
        {
            OrderId = Guid.NewGuid(),
            TotalAmount = 50.00m,
            OrderSnapshot = new OrderSnapshot
            {
                Order = new OrderInfo { OrderId = Guid.NewGuid(), Code = "ORD-001", CreatedAt = DateTime.UtcNow },
                Pricing = new PricingInfo { TotalPrice = 50.00m, Currency = "BRL" },
                Items = new List<ItemInfo>(),
                Version = 1
            }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => client.CreatePaymentAsync(request, "token"));

        Assert.Contains("BadRequest", exception.Message);
    }

    [Fact]
    public async Task CreatePaymentAsync_WhenUnauthorized_ShouldThrowHttpRequestException()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent("Unauthorized", Encoding.UTF8, "text/plain")
        };

        var httpClient = CreateHttpClient(httpResponse);
        var client = new PaymentServiceClient(httpClient, _optionsMock.Object, _loggerMock.Object);

        var request = new CreatePaymentRequest
        {
            OrderId = Guid.NewGuid(),
            TotalAmount = 50.00m,
            OrderSnapshot = new OrderSnapshot
            {
                Order = new OrderInfo { OrderId = Guid.NewGuid(), Code = "ORD-001", CreatedAt = DateTime.UtcNow },
                Pricing = new PricingInfo { TotalPrice = 50.00m, Currency = "BRL" },
                Items = new List<ItemInfo>(),
                Version = 1
            }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => client.CreatePaymentAsync(request, "invalid-token"));

        Assert.Contains("Unauthorized", exception.Message);
    }

    [Fact]
    public async Task CreatePaymentAsync_WhenInternalServerError_ShouldThrowHttpRequestException()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("Internal Server Error", Encoding.UTF8, "text/plain")
        };

        var httpClient = CreateHttpClient(httpResponse);
        var client = new PaymentServiceClient(httpClient, _optionsMock.Object, _loggerMock.Object);

        var request = new CreatePaymentRequest
        {
            OrderId = Guid.NewGuid(),
            TotalAmount = 50.00m,
            OrderSnapshot = new OrderSnapshot
            {
                Order = new OrderInfo { OrderId = Guid.NewGuid(), Code = "ORD-001", CreatedAt = DateTime.UtcNow },
                Pricing = new PricingInfo { TotalPrice = 50.00m, Currency = "BRL" },
                Items = new List<ItemInfo>(),
                Version = 1
            }
        };

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            () => client.CreatePaymentAsync(request, "token"));
    }

    private HttpClient CreateHttpClient(HttpResponseMessage response, Func<HttpRequestMessage, Task>? onRequest = null)
    {
        var handler = new Mock<HttpMessageHandler>();
        var setup = handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());

        if (onRequest != null)
        {
            setup.Callback<HttpRequestMessage, CancellationToken>(async (request, token) =>
            {
                // Processar a requisição de forma assíncrona
                await onRequest(request);
            });
        }

        setup.ReturnsAsync(response);

        return new HttpClient(handler.Object)
        {
            BaseAddress = new Uri(_options.BaseUrl)
        };
    }
}
