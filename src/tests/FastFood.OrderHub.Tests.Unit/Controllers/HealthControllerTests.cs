using FastFood.OrderHub.Api.Controllers;
using FastFood.OrderHub.Application.Models.Common;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace FastFood.OrderHub.Tests.Unit.Controllers;

/// <summary>
/// Testes unitários para HealthController
/// </summary>
public class HealthControllerTests
{
    private readonly HealthController _controller;

    public HealthControllerTests()
    {
        _controller = new HealthController();
    }

    [Fact]
    public void Get_WhenCalled_ShouldReturnOkWithHealthStatus()
    {
        // Arrange
        // Act
        var result = _controller.Get();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<object>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Content);
        
        // Verificar que o conteúdo contém status e timestamp
        var content = apiResponse.Content;
        var statusProperty = content.GetType().GetProperty("status");
        var timestampProperty = content.GetType().GetProperty("timestamp");
        
        Assert.NotNull(statusProperty);
        Assert.NotNull(timestampProperty);
        
        var statusValue = statusProperty.GetValue(content);
        var timestampValue = timestampProperty.GetValue(content);
        
        Assert.Equal("healthy", statusValue);
        Assert.NotNull(timestampValue);
        Assert.NotEmpty(timestampValue?.ToString() ?? string.Empty);
    }
}
