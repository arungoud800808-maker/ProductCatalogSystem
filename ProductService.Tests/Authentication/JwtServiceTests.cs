using FluentAssertions;
using Microsoft.Extensions.Configuration;
using ProductService.Authentication;
using ProductService.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace ProductService.Tests.Authentication;

public class JwtServiceTests
{
    private readonly JwtService _jwtService;

    public JwtServiceTests()
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
    public void GenerateToken_ShouldReturnToken()
    {
        // Arrange
        var user = GetUser();

        // Act
        var token = _jwtService.GenerateToken(user);

        // Assert
        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GenerateToken_ShouldCreateValidJwtToken()
    {
        // Arrange
        var user = GetUser();

        // Act
        var token = _jwtService.GenerateToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();

        handler.CanReadToken(token).Should().BeTrue();

        var jwt = handler.ReadJwtToken(token);

        jwt.Should().NotBeNull();
    }

    [Fact]
    public void GenerateToken_ShouldContainCorrectEmail()
    {
        // Arrange
        var user = GetUser();

        // Act
        var token = _jwtService.GenerateToken(user);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // Assert
        jwt.Claims.First(x => x.Type == JwtRegisteredClaimNames.Email)
            .Value.Should().Be(user.Email);
    }

    [Fact]
    public void GenerateToken_ShouldContainCorrectRole()
    {
        // Arrange
        var user = GetUser();

        // Act
        var token = _jwtService.GenerateToken(user);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // Assert
        jwt.Claims.First(x => x.Type == ClaimTypes.Role)
            .Value.Should().Be("Admin");
    }

    [Fact]
    public void GenerateToken_ShouldContainCorrectName()
    {
        // Arrange
        var user = GetUser();

        // Act
        var token = _jwtService.GenerateToken(user);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // Assert
        jwt.Claims.First(x => x.Type == ClaimTypes.Name)
            .Value.Should().Be(user.FullName);
    }

    [Fact]
    public void GenerateToken_ShouldContainCorrectUserId()
    {
        // Arrange
        var user = GetUser();

        // Act
        var token = _jwtService.GenerateToken(user);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // Assert
        jwt.Claims.First(x => x.Type == JwtRegisteredClaimNames.Sub)
            .Value.Should().Be(user.Id.ToString());
    }

    [Fact]
    public void GenerateToken_ShouldHaveExpiration()
    {
        // Arrange
        var user = GetUser();

        // Act
        var token = _jwtService.GenerateToken(user);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // Assert
        jwt.ValidTo.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnToken()
    {
        // Act
        var refreshToken = _jwtService.GenerateRefreshToken();

        // Assert
        refreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnUniqueTokens()
    {
        // Act
        var token1 = _jwtService.GenerateRefreshToken();
        var token2 = _jwtService.GenerateRefreshToken();

        // Assert
        token1.Should().NotBe(token2);
    }

    [Fact]
    public void GenerateRefreshToken_ShouldBeLongEnough()
    {
        // Act
        var refreshToken = _jwtService.GenerateRefreshToken();

        // Assert
        refreshToken.Length.Should().BeGreaterThan(50);
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_ShouldReturnPrincipal_ForValidToken()
    {
        // Arrange
        var token = _jwtService.GenerateToken(GetUser());

        // Act
        var principal = _jwtService.GetPrincipalFromExpiredToken(token);

        // Assert
        principal.Should().NotBeNull();

        principal!.Identity!.IsAuthenticated.Should().BeTrue();
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_ShouldReturnNull_ForInvalidToken()
    {
        // Act
        var principal = _jwtService.GetPrincipalFromExpiredToken("InvalidToken");

        // Assert
        principal.Should().BeNull();
    }
    [Fact]
    public void GetPrincipalFromExpiredToken_ShouldContainCorrectClaims()
    {
        // Arrange
        var user = GetUser();

        var token = _jwtService.GenerateToken(user);

        // Act
        var principal = _jwtService.GetPrincipalFromExpiredToken(token);

        // Assert
        principal.Should().NotBeNull();

        principal!.Claims.Should().NotBeEmpty();

        foreach (var claim in principal.Claims)
        {
            Console.WriteLine($"{claim.Type} = {claim.Value}");
        }

        principal.FindFirst(ClaimTypes.Name)?.Value
            .Should().Be(user.FullName);

        principal.FindFirst(ClaimTypes.Role)?.Value
            .Should().Be(user.Role);

        principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value
            .Should().Be(user.Email);
    }
}