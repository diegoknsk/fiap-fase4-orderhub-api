using FastFood.OrderHub.Application.Ports;
using FastFood.OrderHub.Infra.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;
using Xunit;

namespace FastFood.OrderHub.Tests.Unit.Services;

/// <summary>
/// Testes unitários para RequestContext
/// </summary>
public class RequestContextTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly RequestContext _requestContext;

    public RequestContextTests()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _requestContext = new RequestContext(_httpContextAccessorMock.Object);
    }

    [Fact]
    public void IsAdmin_WhenUserIsCognitoAdmin_ShouldReturnTrue()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim("token_use", "access"),
            new Claim("scope", "aws.cognito.signin.user.admin"),
            new Claim("client_id", "test-client-id"),
            new Claim("username", "admin")
        };

        var identity = new ClaimsIdentity(claims, "Cognito");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext
        {
            User = principal
        };

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _requestContext.IsAdmin;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsAdmin_WhenUserIsNotCognitoAdmin_ShouldReturnFalse()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim("token_use", "id"),
            new Claim("scope", "openid"),
            new Claim("username", "user")
        };

        var identity = new ClaimsIdentity(claims, "Cognito");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext
        {
            User = principal
        };

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _requestContext.IsAdmin;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsAdmin_WhenUserIsCustomerBearer_ShouldReturnFalse()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new Claim("sub", customerId.ToString()),
            new Claim("customerId", customerId.ToString()),
            new Claim("jti", Guid.NewGuid().ToString())
        };

        var identity = new ClaimsIdentity(claims, "CustomerBearer");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext
        {
            User = principal
        };

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _requestContext.IsAdmin;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsAdmin_WhenUserIsNotAuthenticated_ShouldReturnFalse()
    {
        // Arrange
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal()
        };

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _requestContext.IsAdmin;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsAdmin_WhenHttpContextIsNull_ShouldReturnFalse()
    {
        // Arrange
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        // Act
        var result = _requestContext.IsAdmin;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsAdmin_WhenScopeDoesNotContainAdminScope_ShouldReturnFalse()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim("token_use", "access"),
            new Claim("scope", "openid profile"),
            new Claim("client_id", "test-client-id")
        };

        var identity = new ClaimsIdentity(claims, "Cognito");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext
        {
            User = principal
        };

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _requestContext.IsAdmin;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CustomerId_WhenCustomerIdClaimExists_ShouldReturnCustomerId()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new Claim("customerId", customerId.ToString()),
            new Claim("sub", Guid.NewGuid().ToString())
        };

        var identity = new ClaimsIdentity(claims, "CustomerBearer");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext
        {
            User = principal
        };

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _requestContext.CustomerId;

        // Assert
        Assert.Equal(customerId.ToString(), result);
    }

    [Fact]
    public void CustomerId_WhenOnlySubClaimExists_ShouldReturnSub()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, customerId.ToString())
        };

        var identity = new ClaimsIdentity(claims, "CustomerBearer");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext
        {
            User = principal
        };

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _requestContext.CustomerId;

        // Assert
        Assert.Equal(customerId.ToString(), result);
    }

    [Fact]
    public void CustomerId_WhenOnlySubStringClaimExists_ShouldReturnSub()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new Claim("sub", customerId.ToString())
        };

        var identity = new ClaimsIdentity(claims, "CustomerBearer");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext
        {
            User = principal
        };

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _requestContext.CustomerId;

        // Assert
        Assert.Equal(customerId.ToString(), result);
    }

    [Fact]
    public void CustomerId_WhenNoCustomerClaimsExist_ShouldReturnNull()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim("token_use", "access"),
            new Claim("scope", "aws.cognito.signin.user.admin")
        };

        var identity = new ClaimsIdentity(claims, "Cognito");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext
        {
            User = principal
        };

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _requestContext.CustomerId;

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void CustomerId_WhenUserIsNotAuthenticated_ShouldReturnNull()
    {
        // Arrange
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal()
        };

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _requestContext.CustomerId;

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void CustomerId_WhenHttpContextIsNull_ShouldReturnNull()
    {
        // Arrange
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        // Act
        var result = _requestContext.CustomerId;

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void CustomerId_WhenCustomerIdClaimTakesPrecedenceOverSub_ShouldReturnCustomerId()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var differentSub = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new Claim("customerId", customerId.ToString()),
            new Claim("sub", differentSub.ToString())
        };

        var identity = new ClaimsIdentity(claims, "CustomerBearer");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext
        {
            User = principal
        };

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _requestContext.CustomerId;

        // Assert
        Assert.Equal(customerId.ToString(), result);
        Assert.NotEqual(differentSub.ToString(), result);
    }

    [Fact]
    public void GetBearerToken_WhenHttpContextIsNull_ShouldReturnNull()
    {
        // Arrange
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        // Act
        var result = _requestContext.GetBearerToken();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetBearerToken_WhenAuthorizationHeaderIsMissing_ShouldReturnNull()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _requestContext.GetBearerToken();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetBearerToken_WhenAuthorizationHeaderHasBearerPrefix_ShouldReturnTokenWithoutPrefix()
    {
        // Arrange
        var token = "test-token-123";
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Authorization"] = $"Bearer {token}";
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _requestContext.GetBearerToken();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(token, result);
        Assert.DoesNotContain("Bearer", result);
    }

    [Fact]
    public void GetBearerToken_WhenAuthorizationHeaderHasBearerPrefixCaseInsensitive_ShouldReturnTokenWithoutPrefix()
    {
        // Arrange
        var token = "test-token-123";
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Authorization"] = $"bearer {token}";
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _requestContext.GetBearerToken();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(token, result);
        Assert.DoesNotContain("bearer", result);
    }

    [Fact]
    public void GetBearerToken_WhenAuthorizationHeaderDoesNotHaveBearerPrefix_ShouldReturnFullHeader()
    {
        // Arrange
        var token = "test-token-123";
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Authorization"] = token;
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _requestContext.GetBearerToken();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(token, result);
    }

    [Fact]
    public void GetBearerToken_WhenAuthorizationHeaderIsEmpty_ShouldReturnNull()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Authorization"] = string.Empty;
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _requestContext.GetBearerToken();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetBearerToken_WhenAuthorizationHeaderIsWhitespace_ShouldReturnNull()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Authorization"] = "   ";
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _requestContext.GetBearerToken();

        // Assert
        Assert.Null(result);
    }
}
