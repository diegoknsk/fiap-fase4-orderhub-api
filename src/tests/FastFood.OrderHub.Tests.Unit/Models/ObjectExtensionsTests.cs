using FastFood.OrderHub.Application.Models.Common;
using Xunit;

namespace FastFood.OrderHub.Tests.Unit.Models;

/// <summary>
/// Testes unitários para ObjectExtensions
/// </summary>
public class ObjectExtensionsTests
{
    [Fact]
    public void ToNamedContent_WhenObjectIsNotNull_ShouldReturnObject()
    {
        // Arrange
        var obj = new { Id = Guid.NewGuid(), Name = "Test" };

        // Act
        var result = obj.ToNamedContent();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(obj, result);
    }

    [Fact]
    public void ToNamedContent_WhenObjectIsNull_ShouldReturnNull()
    {
        // Arrange
        object? obj = null;

        // Act
        var result = obj.ToNamedContent();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ToNamedContent_WithDifferentTypes_ShouldReturnSameObject()
    {
        // Arrange
        var stringObj = "test";
        var intObj = 42;
        var guidObj = Guid.NewGuid();

        // Act
        var stringResult = stringObj.ToNamedContent();
        var intResult = intObj.ToNamedContent();
        var guidResult = guidObj.ToNamedContent();

        // Assert
        Assert.Equal(stringObj, stringResult);
        Assert.Equal(intObj, intResult);
        Assert.Equal(guidObj, guidResult);
    }
}
