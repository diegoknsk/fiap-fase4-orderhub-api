using FastFood.OrderHub.Application.InfraServices;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using Xunit;

namespace FastFood.OrderHub.Tests.Unit.InfraServices;

/// <summary>
/// Testes unitários para JwtService
/// </summary>
public class JwtServiceTests
{
    private readonly Mock<IConfiguration> _configMock;
    private readonly JwtService _jwtService;

    public JwtServiceTests()
    {
        _configMock = new Mock<IConfiguration>();
        SetupDefaultConfiguration();
        _jwtService = new JwtService(_configMock.Object);
    }

    private void SetupDefaultConfiguration()
    {
        _configMock.Setup(x => x["JwtSettings:SecretKey"]).Returns("TestSecretKey123456789012345678901234567890");
        _configMock.Setup(x => x["JwtSettings:Issuer"]).Returns("TestIssuer");
        _configMock.Setup(x => x["JwtSettings:Audience"]).Returns("TestAudience");
        _configMock.Setup(x => x["JwtSettings:ExpiresInMinutes"]).Returns("60");
    }

    [Fact]
    public void GenerateToken_Should_Return_Valid_Token()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var username = "testuser";

        // Act
        var token = _jwtService.GenerateToken(userId, username);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);

        // Validar que é um JWT válido
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);
        
        Assert.Equal(userId.ToString(), jsonToken.Subject);
        Assert.Equal(username, jsonToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.UniqueName)?.Value);
        Assert.Equal("TestIssuer", jsonToken.Issuer);
        Assert.Contains("TestAudience", jsonToken.Audiences);
    }

    [Fact]
    public void GenerateToken_Should_Include_Expiration()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var username = "testuser";

        // Act
        var token = _jwtService.GenerateToken(userId, username);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);
        
        Assert.True(jsonToken.ValidTo > DateTime.UtcNow);
        Assert.True(jsonToken.ValidTo <= DateTime.UtcNow.AddMinutes(61)); // 60 minutos + margem
    }

    [Fact]
    public void GenerateToken_Should_Use_Different_Expiration_When_Configured()
    {
        // Arrange
        _configMock.Setup(x => x["JwtSettings:ExpiresInMinutes"]).Returns("30");
        var service = new JwtService(_configMock.Object);
        var userId = Guid.NewGuid();
        var username = "testuser";

        // Act
        var token = service.GenerateToken(userId, username);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);
        
        Assert.True(jsonToken.ValidTo <= DateTime.UtcNow.AddMinutes(31));
        Assert.True(jsonToken.ValidTo > DateTime.UtcNow.AddMinutes(29));
    }

    [Fact]
    public void GenerateTokenV2_Should_Return_Valid_Token_For_Admin_Audience()
    {
        // Arrange
        _configMock.Setup(x => x["JwtAdmin:Issuer"]).Returns("AdminIssuer");
        _configMock.Setup(x => x["JwtAdmin:Audience"]).Returns("AdminAudience");
        _configMock.Setup(x => x["JwtAdmin:SecretKey"]).Returns("AdminSecretKey123456789012345678901234567890");
        
        var service = new JwtService(_configMock.Object);
        var subjectId = Guid.NewGuid();
        var role = "Admin";
        var scope = "read:write";
        var amr = new[] { "password" };
        var audience = "AdminAudience";
        var expiresMinutes = 120;

        // Act
        var token = service.GenerateTokenV2(subjectId, role, scope, amr, audience, expiresMinutes);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);

        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);
        
        Assert.Equal(subjectId.ToString(), jsonToken.Subject);
        Assert.Equal(role, jsonToken.Claims.FirstOrDefault(c => c.Type == "role")?.Value);
        Assert.Equal(scope, jsonToken.Claims.FirstOrDefault(c => c.Type == "scope")?.Value);
        Assert.Equal("password", jsonToken.Claims.FirstOrDefault(c => c.Type == "amr")?.Value);
        Assert.Equal("AdminIssuer", jsonToken.Issuer);
        Assert.Contains("AdminAudience", jsonToken.Audiences);
    }

    [Fact]
    public void GenerateTokenV2_Should_Return_Valid_Token_For_Customer_Audience()
    {
        // Arrange
        _configMock.Setup(x => x["JwtCustomer:Issuer"]).Returns("CustomerIssuer");
        _configMock.Setup(x => x["JwtCustomer:Audience"]).Returns("CustomerAudience");
        _configMock.Setup(x => x["JwtCustomer:SecretKey"]).Returns("CustomerSecretKey123456789012345678901234567890");
        
        var service = new JwtService(_configMock.Object);
        var subjectId = Guid.NewGuid();
        var role = "Customer";
        var scope = "read";
        var amr = new[] { "password" };
        var audience = "CustomerAudience";
        var expiresMinutes = 60;

        // Act
        var token = service.GenerateTokenV2(subjectId, role, scope, amr, audience, expiresMinutes);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);

        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);
        
        Assert.Equal(subjectId.ToString(), jsonToken.Subject);
        Assert.Equal(role, jsonToken.Claims.FirstOrDefault(c => c.Type == "role")?.Value);
        Assert.Equal(scope, jsonToken.Claims.FirstOrDefault(c => c.Type == "scope")?.Value);
        Assert.Equal("CustomerIssuer", jsonToken.Issuer);
        Assert.Contains("CustomerAudience", jsonToken.Audiences);
    }

    [Fact]
    public void GenerateTokenV2_Should_Use_Admin_Issuer_When_Customer_Issuer_Not_Set()
    {
        // Arrange
        _configMock.Setup(x => x["JwtAdmin:Issuer"]).Returns("AdminIssuer");
        _configMock.Setup(x => x["JwtAdmin:Audience"]).Returns("AdminAudience");
        _configMock.Setup(x => x["JwtAdmin:SecretKey"]).Returns("AdminSecretKey123456789012345678901234567890");
        _configMock.Setup(x => x["JwtCustomer:Issuer"]).Returns((string?)null);
        _configMock.Setup(x => x["JwtCustomer:Audience"]).Returns("CustomerAudience");
        _configMock.Setup(x => x["JwtCustomer:SecretKey"]).Returns("CustomerSecretKey123456789012345678901234567890");
        
        var service = new JwtService(_configMock.Object);
        var subjectId = Guid.NewGuid();
        var audience = "CustomerAudience";

        // Act
        var token = service.GenerateTokenV2(subjectId, "Customer", "read", (string[]?)null, audience, 60);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);
        
        Assert.Equal("AdminIssuer", jsonToken.Issuer);
    }

    [Fact]
    public void GenerateTokenV2_Should_Handle_Empty_Amr_Array()
    {
        // Arrange
        _configMock.Setup(x => x["JwtAdmin:Issuer"]).Returns("AdminIssuer");
        _configMock.Setup(x => x["JwtAdmin:Audience"]).Returns("AdminAudience");
        _configMock.Setup(x => x["JwtAdmin:SecretKey"]).Returns("AdminSecretKey123456789012345678901234567890");
        
        var service = new JwtService(_configMock.Object);
        var subjectId = Guid.NewGuid();

        // Act
        var token = service.GenerateTokenV2(subjectId, "Admin", "read:write", Array.Empty<string>(), "AdminAudience", 60);

        // Assert
        Assert.NotNull(token);
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);
        
        var amrClaims = jsonToken.Claims.Where(c => c.Type == "amr").ToList();
        Assert.Empty(amrClaims);
    }

    [Fact]
    public void GenerateTokenV2_Should_Handle_Null_Amr()
    {
        // Arrange
        _configMock.Setup(x => x["JwtAdmin:Issuer"]).Returns("AdminIssuer");
        _configMock.Setup(x => x["JwtAdmin:Audience"]).Returns("AdminAudience");
        _configMock.Setup(x => x["JwtAdmin:SecretKey"]).Returns("AdminSecretKey123456789012345678901234567890");
        
        var service = new JwtService(_configMock.Object);
        var subjectId = Guid.NewGuid();

        // Act
        var token = service.GenerateTokenV2(subjectId, "Admin", "read:write", (string[]?)null, "AdminAudience", 60);

        // Assert
        Assert.NotNull(token);
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);
        
        var amrClaims = jsonToken.Claims.Where(c => c.Type == "amr").ToList();
        Assert.Empty(amrClaims);
    }

    [Fact]
    public void GenerateTokenV2_Should_Handle_Multiple_Amr_Values()
    {
        // Arrange
        _configMock.Setup(x => x["JwtAdmin:Issuer"]).Returns("AdminIssuer");
        _configMock.Setup(x => x["JwtAdmin:Audience"]).Returns("AdminAudience");
        _configMock.Setup(x => x["JwtAdmin:SecretKey"]).Returns("AdminSecretKey123456789012345678901234567890");
        
        var service = new JwtService(_configMock.Object);
        var subjectId = Guid.NewGuid();
        var amr = new[] { "password", "mfa", "sms" };

        // Act
        var token = service.GenerateTokenV2(subjectId, "Admin", "read:write", amr, "AdminAudience", 60);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);
        
        var amrClaims = jsonToken.Claims.Where(c => c.Type == "amr").ToList();
        Assert.Equal(3, amrClaims.Count);
        Assert.Contains(amrClaims, c => c.Value == "password");
        Assert.Contains(amrClaims, c => c.Value == "mfa");
        Assert.Contains(amrClaims, c => c.Value == "sms");
    }
}
