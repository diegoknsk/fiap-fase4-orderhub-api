using FastFood.OrderHub.Application.DTOs;
using FastFood.OrderHub.Application.InputModels.OrderManagement;
using FastFood.OrderHub.Application.Ports;
using FastFood.OrderHub.Application.Presenters.OrderManagement;
using FastFood.OrderHub.Application.Responses.OrderManagement;
using FastFood.OrderHub.Application.UseCases.OrderManagement;
using FluentAssertions;
using Moq;
using TechTalk.SpecFlow;

namespace FastFood.OrderHub.Tests.Bdd.Steps;

[Binding]
public class GetOrderByIdSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly Mock<IOrderDataSource> _orderDataSourceMock;
    private readonly Mock<IRequestContext> _requestContextMock;
    private readonly GetOrderByIdPresenter _presenter;
    private GetOrderByIdUseCase? _useCase;
    private GetOrderByIdResponse? _response;
    private Exception? _exception;

    public GetOrderByIdSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
        _orderDataSourceMock = new Mock<IOrderDataSource>();
        _requestContextMock = new Mock<IRequestContext>();
        _presenter = new GetOrderByIdPresenter();
    }

    [Given(@"I am an admin user")]
    public void GivenIAmAnAdminUser()
    {
        _requestContextMock.Setup(x => x.IsAdmin).Returns(true);
        _requestContextMock.Setup(x => x.CustomerId).Returns((string?)null);
    }

    [Given(@"I am a customer with ID ""(.*)""")]
    public void GivenIAmACustomerWithId(string customerId)
    {
        _requestContextMock.Setup(x => x.IsAdmin).Returns(false);
        _requestContextMock.Setup(x => x.CustomerId).Returns(customerId);
        _scenarioContext["CustomerId"] = customerId;
    }

    [Given(@"there is an order with ID ""(.*)"" and code ""(.*)""")]
    public void GivenThereIsAnOrderWithIdAndCode(string orderId, string orderCode)
    {
        var orderDto = new OrderDto
        {
            Id = Guid.Parse(orderId),
            Code = orderCode,
            CustomerId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            OrderStatus = 1,
            TotalPrice = 0,
            Items = new List<OrderedProductDto>()
        };

        _orderDataSourceMock
            .Setup(x => x.GetByIdAsync(It.Is<Guid>(g => g == Guid.Parse(orderId))))
            .ReturnsAsync(orderDto);
    }

    [Given(@"there is an order with ID ""(.*)"" belonging to customer ""(.*)""")]
    public void GivenThereIsAnOrderWithIdBelongingToCustomer(string orderId, string customerId)
    {
        var orderDto = new OrderDto
        {
            Id = Guid.Parse(orderId),
            Code = "ORD-001",
            CustomerId = Guid.Parse(customerId),
            CreatedAt = DateTime.UtcNow,
            OrderStatus = 1,
            TotalPrice = 0,
            Items = new List<OrderedProductDto>()
        };

        _orderDataSourceMock
            .Setup(x => x.GetByIdAsync(It.Is<Guid>(g => g == Guid.Parse(orderId))))
            .ReturnsAsync(orderDto);

        _orderDataSourceMock
            .Setup(x => x.GetByIdForCustomerAsync(
                It.Is<Guid>(g => g == Guid.Parse(orderId)),
                It.Is<Guid>(g => g == Guid.Parse(customerId))))
            .ReturnsAsync(orderDto);

        // Para quando o customerId não corresponde, retorna null
        _orderDataSourceMock
            .Setup(x => x.GetByIdForCustomerAsync(
                It.Is<Guid>(g => g == Guid.Parse(orderId)),
                It.Is<Guid>(g => g != Guid.Parse(customerId))))
            .ReturnsAsync((OrderDto?)null);
    }

    [Given(@"there is no order with ID ""(.*)""")]
    public void GivenThereIsNoOrderWithId(string orderId)
    {
        _orderDataSourceMock
            .Setup(x => x.GetByIdAsync(It.Is<Guid>(g => g == Guid.Parse(orderId))))
            .ReturnsAsync((OrderDto?)null);
    }

    [When(@"I request to get the order with ID ""(.*)""")]
    public async Task WhenIRequestToGetTheOrderWithId(string orderId)
    {
        _useCase = new GetOrderByIdUseCase(
            _orderDataSourceMock.Object,
            _presenter,
            _requestContextMock.Object);

        var input = new GetOrderByIdInputModel
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

    [Then(@"the order should be returned successfully")]
    public void ThenTheOrderShouldBeReturnedSuccessfully()
    {
        _exception.Should().BeNull("no exception should be thrown");
        _response.Should().NotBeNull("the order should be returned");
        _response!.OrderId.Should().NotBeEmpty("order ID should not be empty");
    }

    [Then(@"the order code should be ""(.*)""")]
    public void ThenTheOrderCodeShouldBe(string expectedCode)
    {
        _response.Should().NotBeNull();
        _response!.Code.Should().Be(expectedCode);
    }

    [Then(@"the order customer ID should be ""(.*)""")]
    public void ThenTheOrderCustomerIdShouldBe(string expectedCustomerId)
    {
        _response.Should().NotBeNull();
        _response!.CustomerId.Should().Be(Guid.Parse(expectedCustomerId));
    }

    [Then(@"I should receive an error that the order was not found")]
    public void ThenIShouldReceiveAnErrorThatTheOrderWasNotFound()
    {
        _response.Should().BeNull("no response should be returned");
        _exception.Should().NotBeNull("an exception should be thrown");
        _exception!.Message.Should().Contain("não encontrado", "error message should indicate order not found");
    }
}
