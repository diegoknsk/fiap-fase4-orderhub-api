namespace FastFood.OrderHub.Tests.Unit;

/// <summary>
/// Testes básicos para validar que a estrutura de testes está funcionando corretamente
/// </summary>
public class HelloWorldTests
{
    [Fact]
    public void Should_Return_True_When_Validating_Test_Structure()
    {
        // Arrange
        var expected = true;

        // Act
        var result = true;

        // Assert
        Assert.Equal(expected, result);
    }
}

