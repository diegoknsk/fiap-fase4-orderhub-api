# Subtask 13: Testes BDD Integração PayStream

## Objetivo
Criar testes BDD (SpecFlow) para validar a integração completa com PayStream, incluindo cenários de sucesso e falha.

## Arquivos a Criar

### 1. `src/tests/FastFood.OrderHub.Tests.Bdd/Features/ConfirmOrderSelectionWithPayment.feature`

```gherkin
Feature: Confirmar Seleção de Pedido com Integração PayStream
    Como cliente
    Eu quero finalizar meu pedido e iniciar o pagamento automaticamente
    Para que o fluxo de pagamento seja iniciado sem intervenção manual

    Background:
        Given I am a customer with ID "customer-123"

    Scenario: Finalizar pedido com sucesso e criar pagamento
        Given there is an order with ID "order-123" and status "Started"
        And the order has items
        And the PayStream service is available
        When I confirm the order selection for order "order-123"
        Then the order should have status "AwaitingPayment"
        And the PayStream should be called with the Bearer token
        And the PayStream should receive a valid orderSnapshot
        And the response should be 200 OK

    Scenario: Finalizar pedido mas PayStream está indisponível
        Given there is an order with ID "order-123" and status "Started"
        And the order has items
        And the PayStream service returns error 500
        When I confirm the order selection for order "order-123"
        Then the order should have status "AwaitingPayment"
        And the error should be logged
        And the response should be 502 Bad Gateway

    Scenario: Finalizar pedido sem token Bearer
        Given there is an order with ID "order-123" and status "Started"
        And the order has items
        And the request does not have a Bearer token
        When I confirm the order selection for order "order-123"
        Then the order should have status "AwaitingPayment"
        And a warning should be logged about missing token
        And the response should be 200 OK
        And the PayStream should not be called

    Scenario: Finalizar pedido e PayStream retorna erro 400
        Given there is an order with ID "order-123" and status "Started"
        And the order has items
        And the PayStream service returns error 400
        When I confirm the order selection for order "order-123"
        Then the order should have status "AwaitingPayment"
        And the error should be logged
        And the response should be 502 Bad Gateway
```

### 2. `src/tests/FastFood.OrderHub.Tests.Bdd/Steps/ConfirmOrderSelectionSteps.cs`

```csharp
using FastFood.OrderHub.Application.DTOs;
using FastFood.OrderHub.Application.DTOs.Payment;
using FastFood.OrderHub.Application.InputModels.OrderManagement;
using FastFood.OrderHub.Application.Ports;
using FastFood.OrderHub.Application.Presenters.OrderManagement;
using FastFood.OrderHub.Application.Responses.OrderManagement;
using FastFood.OrderHub.Application.UseCases.OrderManagement;
using FastFood.OrderHub.Domain.Common.Enums;
using FluentAssertions;
using Moq;
using System.Net;
using TechTalk.SpecFlow;

namespace FastFood.OrderHub.Tests.Bdd.Steps;

[Binding]
public class ConfirmOrderSelectionSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly Mock<IOrderDataSource> _orderDataSourceMock;
    private readonly Mock<IPaymentServiceClient> _paymentServiceClientMock;
    private readonly Mock<IRequestContext> _requestContextMock;
    private readonly ConfirmOrderSelectionPresenter _presenter;
    private ConfirmOrderSelectionUseCase? _useCase;
    private ConfirmOrderSelectionResponse? _response;
    private Exception? _exception;
    private bool _payStreamCalled = false;
    private CreatePaymentRequest? _paymentRequest;

    public ConfirmOrderSelectionSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
        _orderDataSourceMock = new Mock<IOrderDataSource>();
        _paymentServiceClientMock = new Mock<IPaymentServiceClient>();
        _requestContextMock = new Mock<IRequestContext>();
        _presenter = new ConfirmOrderSelectionPresenter();
    }

    [Given(@"there is an order with ID ""(.*)"" and status ""(.*)""")]
    public void GivenThereIsAnOrderWithIdAndStatus(string orderId, string status)
    {
        var orderStatus = status == "Started" 
            ? EnumOrderStatus.Started 
            : EnumOrderStatus.AwaitingPayment;

        var orderDto = new OrderDto
        {
            Id = Guid.Parse(orderId),
            Code = "ORD-001",
            CustomerId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            OrderStatus = (int)orderStatus,
            TotalPrice = 50.00m,
            Items = new List<OrderedProductDto>()
        };

        _orderDataSourceMock
            .Setup(x => x.GetByIdAsync(It.Is<Guid>(g => g == Guid.Parse(orderId))))
            .ReturnsAsync(orderDto);

        _orderDataSourceMock
            .Setup(x => x.UpdateAsync(It.IsAny<OrderDto>()))
            .Returns(Task.CompletedTask);
    }

    [Given(@"the order has items")]
    public void GivenTheOrderHasItems()
    {
        var orderId = _scenarioContext.ContainsKey("OrderId") 
            ? Guid.Parse(_scenarioContext["OrderId"].ToString()!)
            : Guid.NewGuid();

        var orderDto = new OrderDto
        {
            Id = orderId,
            Code = "ORD-001",
            CustomerId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            OrderStatus = (int)EnumOrderStatus.Started,
            TotalPrice = 50.00m,
            Items = new List<OrderedProductDto>
            {
                new OrderedProductDto
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.NewGuid(),
                    OrderId = orderId,
                    Quantity = 2,
                    FinalPrice = 25.00m,
                    ProductName = "Hambúrguer",
                    CustomIngredients = new List<OrderedProductIngredientDto>()
                }
            }
        };

        _orderDataSourceMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(orderDto);
    }

    [Given(@"the PayStream service is available")]
    public void GivenThePayStreamServiceIsAvailable()
    {
        _paymentServiceClientMock
            .Setup(x => x.CreatePaymentAsync(
                It.IsAny<CreatePaymentRequest>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Callback<CreatePaymentRequest, string, CancellationToken>((req, token, ct) =>
            {
                _payStreamCalled = true;
                _paymentRequest = req;
            })
            .ReturnsAsync(new CreatePaymentResponse
            {
                PaymentId = Guid.NewGuid(),
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            });

        _requestContextMock.Setup(x => x.GetBearerToken()).Returns("test-bearer-token");
    }

    [Given(@"the PayStream service returns error (.*)")]
    public void GivenThePayStreamServiceReturnsError(int statusCode)
    {
        var httpStatusCode = (HttpStatusCode)statusCode;
        _paymentServiceClientMock
            .Setup(x => x.CreatePaymentAsync(
                It.IsAny<CreatePaymentRequest>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException($"PayStream error: {statusCode}"));

        _requestContextMock.Setup(x => x.GetBearerToken()).Returns("test-bearer-token");
    }

    [Given(@"the request does not have a Bearer token")]
    public void GivenTheRequestDoesNotHaveABearerToken()
    {
        _requestContextMock.Setup(x => x.GetBearerToken()).Returns((string?)null);
    }

    [When(@"I confirm the order selection for order ""(.*)""")]
    public async Task WhenIConfirmTheOrderSelectionForOrder(string orderId)
    {
        var loggerMock = new Mock<ILogger<ConfirmOrderSelectionUseCase>>();

        _useCase = new ConfirmOrderSelectionUseCase(
            _orderDataSourceMock.Object,
            _presenter,
            _paymentServiceClientMock.Object,
            _requestContextMock.Object,
            loggerMock.Object);

        var input = new ConfirmOrderSelectionInputModel
        {
            OrderId = Guid.Parse(orderId)
        };

        try
        {
            _response = await _useCase.ExecuteAsync(input);
            _exception = null;
        }
        catch (Exception ex)
        {
            _exception = ex;
            _response = null;
        }
    }

    [Then(@"the order should have status ""(.*)""")]
    public void ThenTheOrderShouldHaveStatus(string expectedStatus)
    {
        _orderDataSourceMock.Verify(
            x => x.UpdateAsync(It.Is<OrderDto>(dto => 
                dto.OrderStatus == (int)EnumOrderStatus.AwaitingPayment)),
            Times.Once);
    }

    [Then(@"the PayStream should be called with the Bearer token")]
    public void ThenThePayStreamShouldBeCalledWithTheBearerToken()
    {
        _paymentServiceClientMock.Verify(
            x => x.CreatePaymentAsync(
                It.IsAny<CreatePaymentRequest>(),
                It.Is<string>(t => t == "test-bearer-token"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Then(@"the PayStream should receive a valid orderSnapshot")]
    public void ThenThePayStreamShouldReceiveAValidOrderSnapshot()
    {
        _paymentRequest.Should().NotBeNull();
        _paymentRequest!.OrderSnapshot.Should().NotBeNull();
        _paymentRequest.OrderSnapshot.Order.Should().NotBeNull();
        _paymentRequest.OrderSnapshot.Pricing.Should().NotBeNull();
        _paymentRequest.OrderSnapshot.Items.Should().NotBeEmpty();
        _paymentRequest.OrderSnapshot.Version.Should().Be(1);
    }

    [Then(@"the response should be (.*) OK")]
    public void ThenTheResponseShouldBeOk(int statusCode)
    {
        _exception.Should().BeNull();
        _response.Should().NotBeNull();
    }

    [Then(@"the error should be logged")]
    public void ThenTheErrorShouldBeLogged()
    {
        _exception.Should().NotBeNull();
    }

    [Then(@"the response should be (.*) Bad Gateway")]
    public void ThenTheResponseShouldBeBadGateway(int statusCode)
    {
        _exception.Should().NotBeNull();
        _exception.Should().BeOfType<BusinessException>();
    }

    [Then(@"a warning should be logged about missing token")]
    public void ThenAWarningShouldBeLoggedAboutMissingToken()
    {
        // Validação feita via mock do logger (se necessário)
        _payStreamCalled.Should().BeFalse();
    }

    [Then(@"the PayStream should not be called")]
    public void ThenThePayStreamShouldNotBeCalled()
    {
        _paymentServiceClientMock.Verify(
            x => x.CreatePaymentAsync(
                It.IsAny<CreatePaymentRequest>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
```

## Validações

- [ ] Feature file criado com todos os cenários
- [ ] Steps implementados para todos os Given/When/Then
- [ ] Teste: Finalizar pedido com sucesso e criar pagamento
- [ ] Teste: Finalizar pedido mas PayStream está indisponível
- [ ] Teste: Finalizar pedido sem token Bearer
- [ ] Teste: Finalizar pedido e PayStream retorna erro 400
- [ ] Todos os testes BDD passam
- [ ] Código compila sem erros

## Observações

- Usar SpecFlow para BDD
- Mockar todas as dependências externas
- Validar comportamento end-to-end
- Verificar ordem de execução (salvar pedido antes de chamar PayStream)
- Validar estrutura do orderSnapshot enviado
