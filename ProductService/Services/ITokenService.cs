using System.Security.Claims;

namespace ProductService.Services;

public interface ITokenService
{
    string GenerateRefreshToken();

    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}