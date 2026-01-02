using FastFood.OrderHub.Application.Models.Common;
using Xunit;

namespace FastFood.OrderHub.Tests.Unit.Models;

/// <summary>
/// Testes unitários para ApiResponse
/// </summary>
public class ApiResponseTests
{
    [Fact]
    public void ApiResponse_WhenCreatedWithContent_ShouldSetProperties()
    {
        // Arrange
        var content = new { Id = Guid.NewGuid(), Name = "Test" };
        var message = "Test message";

        // Act
        var response = new ApiResponse<object>(content, message, true);

        // Assert
        Assert.True(response.Success);
        Assert.Equal(message, response.Message);
        Assert.NotNull(response.Content);
    }

    [Fact]
    public void ApiResponse_Ok_WithData_ShouldReturnSuccessResponse()
    {
        // Arrange
        var data = new { Id = Guid.NewGuid(), Name = "Test" };

        // Act
        var response = ApiResponse<object>.Ok(data, "Success message");

        // Assert
        Assert.True(response.Success);
        Assert.Equal("Success message", response.Message);
        Assert.NotNull(response.Content);
    }

    [Fact]
    public void ApiResponse_Ok_WithDefaultMessage_ShouldUseDefaultMessage()
    {
        // Arrange
        var data = new { Id = Guid.NewGuid() };

        // Act
        var response = ApiResponse<object>.Ok(data);

        // Assert
        Assert.True(response.Success);
        Assert.Equal("Requisição bem-sucedida.", response.Message);
        Assert.NotNull(response.Content);
    }

    [Fact]
    public void ApiResponse_Ok_WithoutData_ShouldReturnSuccessResponse()
    {
        // Act
        var response = ApiResponse<object>.Ok("Success message");

        // Assert
        Assert.True(response.Success);
        Assert.Equal("Success message", response.Message);
        Assert.Null(response.Content);
    }

    [Fact]
    public void ApiResponse_Fail_ShouldReturnFailureResponse()
    {
        // Arrange
        var errorMessage = "Error occurred";

        // Act
        var response = ApiResponse<object>.Fail(errorMessage);

        // Assert
        Assert.False(response.Success);
        Assert.Equal(errorMessage, response.Message);
        Assert.Null(response.Content);
    }

    [Fact]
    public void ApiResponse_WhenCreatedWithFailure_ShouldSetSuccessToFalse()
    {
        // Arrange
        var message = "Error message";

        // Act
        var response = new ApiResponse<object>(null, message, false);

        // Assert
        Assert.False(response.Success);
        Assert.Equal(message, response.Message);
        Assert.Null(response.Content);
    }
}
