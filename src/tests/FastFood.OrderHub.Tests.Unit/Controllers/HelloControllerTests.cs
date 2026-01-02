using FastFood.OrderHub.Api.Controllers;
using FastFood.OrderHub.Application.Models.Common;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace FastFood.OrderHub.Tests.Unit.Controllers;

/// <summary>
/// Testes unitários para HelloController
/// </summary>
public class HelloControllerTests
{
    private readonly HelloController _controller;

    public HelloControllerTests()
    {
        _controller = new HelloController();
    }

    [Fact]
    public void Get_WhenCalled_ShouldReturnOkWithMessage()
    {
        // Arrange
        // Act
        var result = _controller.Get();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<object>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Content);
        
        // Verificar que o conteúdo contém a mensagem esperada
        var content = apiResponse.Content;
        var property = content.GetType().GetProperty("message");
        Assert.NotNull(property);
        var messageValue = property.GetValue(content);
        Assert.Equal("olá mundo", messageValue);
    }
}
