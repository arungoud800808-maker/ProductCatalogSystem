using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ProductService.Authentication;
using ProductService.Models;
using ProductService.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace ProductService.Tests.Authentication;

public class TokenServiceTests
{
    private readonly TokenService _tokenService;
    private readonly JwtService _jwtService;

    public TokenServiceTests()
    {
        var settings = new Dictionary<string, string?>
        {
            { "Jwt:Key", "ThisIsMyVeryStrongSecretKey123456789012345" },
            { "Jwt:Issuer", "ProductService" },
            { "Jwt:Audience", "ProductServiceUsers" }
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        _tokenService = new TokenService(configuration);
        _jwtService = new JwtService(configuration);
    }

    private static User GetUser()
    {
        return new User
        {
            Id = 1,
            FullName = "Arun Kumar",
            Email = "arun@test.com",
            Role = "Admin"
        };
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnToken()
    {
        // Act

        var token = _tokenService.GenerateRefreshToken();

        // Assert

        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnUniqueTokens()
    {
        // Act

        var token1 = _tokenService.GenerateRefreshToken();
        var token2 = _tokenService.GenerateRefreshToken();

        // Assert

        token1.Should().NotBe(token2);
    }

    [Fact]
    public void GenerateRefreshToken_ShouldHaveExpectedLength()
    {
        // Act

        var token = _tokenService.GenerateRefreshToken();

        // Assert

        token.Length.Should().BeGreaterThan(50);
    }

    [Fact]
    public void GenerateRefreshToken_ShouldBeBase64String()
    {
        // Act

        var token = _tokenService.GenerateRefreshToken();

        // Assert

        Action action = () => Convert.FromBase64String(token);

        action.Should().NotThrow();
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_ShouldReturnPrincipal_ForValidToken()
    {
        // Arrange

        var jwt = _jwtService.GenerateToken(GetUser());

        // Act

        var principal = _tokenService.GetPrincipalFromExpiredToken(jwt);

        // Assert

        principal.Should().NotBeNull();

        principal.Identity!.IsAuthenticated.Should().BeTrue();
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_ShouldContainCorrectClaims()
    {
        // Arrange

        var user = GetUser();

        var jwt = _jwtService.GenerateToken(user);

        // Act

        var principal = _tokenService.GetPrincipalFromExpiredToken(jwt);

        // Assert

        principal.FindFirst(ClaimTypes.Name)?.Value
            .Should().Be(user.FullName);

        principal.FindFirst(ClaimTypes.Role)?.Value
            .Should().Be(user.Role);
    }

    
    [Fact]
    public void GetPrincipalFromExpiredToken_ShouldThrow_ForInvalidToken()
    {
        // Arrange
        const string invalidToken = "InvalidToken";

        // Act
        Action action = () =>
            _tokenService.GetPrincipalFromExpiredToken(invalidToken);

        // Assert
        action.Should()
              .Throw<SecurityTokenMalformedException>();
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_ShouldContainEmailClaim()
    {
        // Arrange

        var user = GetUser();

        var jwt = _jwtService.GenerateToken(user);

        // Act

        var principal = _tokenService.GetPrincipalFromExpiredToken(jwt);

        // Assert

        var email =
            principal.FindFirst(ClaimTypes.Email)?.Value ??
            principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value;

        email.Should().Be(user.Email);
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_ShouldContainNameIdentifier()
    {
        // Arrange

        var user = GetUser();

        var jwt = _jwtService.GenerateToken(user);

        // Act

        var principal = _tokenService.GetPrincipalFromExpiredToken(jwt);

        // Assert

        var id =
            principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
            principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        id.Should().Be(user.Id.ToString());
    }
}