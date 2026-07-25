using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductService.Authentication;
using ProductService.Data;
using ProductService.DTOs;
using ProductService.Models;
using ProductService.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.RateLimiting;

namespace ProductService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ProductDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly IAuditService _auditService;

   public AuthController(
    ProductDbContext context,
    IJwtService jwtService,
    IEmailService emailService,
    IAuditService auditService)
{
    _context = context;
    _jwtService = jwtService;
    _emailService = emailService;
    _auditService = auditService;
}
    // POST api/auth/register
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (await _context.Users.AnyAsync(x => x.Email == dto.Email))
        {
            return BadRequest("Email already exists.");
        }

        var verificationToken =
            Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = dto.Role,

            EmailConfirmed = false,
            EmailVerificationToken = verificationToken,
            EmailVerificationTokenExpiry = DateTime.UtcNow.AddDays(1)
        };

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        await _auditService.LogAsync(
    user.Email,
    user.Role,
    "User Registered",
    "User",
    $"New user '{user.Email}' registered successfully.",
    HttpContext.Connection.RemoteIpAddress?.ToString());

        await _emailService.SendEmailAsync(
     user.Email,
     "Verify Your Email",
     $@"
    <h2>Welcome to Product Catalog API</h2>

    <p>Please use the following verification token:</p>

    <h3>{verificationToken}</h3>

    <p>This token expires in 24 hours.</p>");

        return Ok(new
        {
            Message = "Verification email sent successfully."
        });
    }

    // POST api/auth/login
    [AllowAnonymous]
    [HttpPost("login")]
    [EnableRateLimiting("FixedPolicy")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Email == dto.Email);

        if (user == null)
            return Unauthorized("Invalid email or password.");

        // Email verification
        if (!user.EmailConfirmed)
            return Unauthorized("Please verify your email before logging in.");

        // Account Lockout Check
        if (user.LockoutEnd.HasValue &&
            user.LockoutEnd.Value > DateTime.UtcNow)
        {
            return Unauthorized(
                $"Your account is locked until {user.LockoutEnd.Value:u}");
        }

        // Password verification
        bool verified = BCrypt.Net.BCrypt.Verify(
            dto.Password,
            user.PasswordHash);

        if (!verified)
        {
            user.AccessFailedCount++;

            if (user.AccessFailedCount >= 6)
            {
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
                user.AccessFailedCount = 0;

                await _context.SaveChangesAsync();
                await _auditService.LogAsync(
    user.Email,
     user.Role,
    "Login Success",
    "User",
    $"User '{user.Email}' logged in successfully.",
    HttpContext.Connection.RemoteIpAddress?.ToString());
            }

            await _context.SaveChangesAsync();
            await _auditService.LogAsync(
     user.Email,
      user.Role,
     "Login Failed",
     "User",
     $"Failed login attempt for '{user.Email}'.",
     HttpContext.Connection.RemoteIpAddress?.ToString());
            return Unauthorized(
                $"Invalid password. Attempts left: {3 - user.AccessFailedCount}");
        }

        // Successful login
        user.AccessFailedCount = 0;
        user.LockoutEnd = null;

        var token = _jwtService.GenerateToken(user);

        var refreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        await _context.SaveChangesAsync();
        await _auditService.LogAsync(
    user.Email,
     user.Role,
    "Login Success",
    "User",
    $"User '{user.Email}' logged in successfully.",
    HttpContext.Connection.RemoteIpAddress?.ToString());
        return Ok(new AuthResponseDto
        {
            AccessToken = token,
            RefreshToken = refreshToken,
            Email = user.Email,
            Role = user.Role,
            Expiration = DateTime.UtcNow.AddHours(2)
        });

    }
    // POST: api/Auth/forgot-password
    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Email == dto.Email);

        if (user == null)
        {
            return BadRequest("User not found.");
        }

        // Generate Reset Token
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        user.PasswordResetToken = token;
        user.ResetTokenExpires = DateTime.UtcNow.AddHours(1);

        await _context.SaveChangesAsync();

        await _emailService.SendEmailAsync(
     user.Email,

     "Reset Password",
     $@"
    <h2>Password Reset</h2>

    <p>Your password reset token is:</p>

    <h3>{token}</h3>

    <p>This token expires in 1 hour.</p>");

        return Ok(new
        {
            Message = "Password reset email sent successfully."
        });
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
     RefreshTokenRequestDto request)
    {
        var principal =
            _jwtService.GetPrincipalFromExpiredToken(request.Token);

        if (principal == null)
            return Unauthorized("Invalid Access Token.");

        var email =
            principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
            ??
            principal.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email)?.Value;

        if (string.IsNullOrWhiteSpace(email))
            return Unauthorized("Invalid token.");

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
            return Unauthorized();

        if (user.RefreshToken != request.RefreshToken)
            return Unauthorized("Invalid Refresh Token.");

        if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            return Unauthorized("Refresh Token expired.");

        var newAccessToken =
            _jwtService.GenerateToken(user);

        var newRefreshToken =
            _jwtService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime =
            DateTime.UtcNow.AddDays(7);

        await _context.SaveChangesAsync();

        return Ok(new AuthResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            Email = user.Email,
            Role = user.Role,
            Expiration = DateTime.UtcNow.AddHours(2)
        });
    }

    // POST: api/Auth/reset-password
    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.PasswordResetToken == dto.Token);

        if (user == null)
            return BadRequest("Invalid reset token.");

        if (user.ResetTokenExpires < DateTime.UtcNow)
            return BadRequest("Reset token has expired.");

        // Update password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

        // Remove reset token
        user.PasswordResetToken = null;
        user.ResetTokenExpires = null;

        await _context.SaveChangesAsync();
        await _auditService.LogAsync(
    user.Email,
     user.Role,
    "Password Reset",
    "User",
    $"Password reset completed for '{user.Email}'.",
    HttpContext.Connection.RemoteIpAddress?.ToString());

        return Ok(new
        {
            Message = "Password reset successfully."
        });
    }
    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
    {
        var email =
            User.FindFirst(ClaimTypes.Email)?.Value ??
            User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;

        if (string.IsNullOrEmpty(email))
            return Unauthorized();

        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Email == email);

        if (user == null)
            return Unauthorized();

        // Verify current password
        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
        {
            return BadRequest("Current password is incorrect.");
        }

        // Prevent using the same password again
        if (BCrypt.Net.BCrypt.Verify(dto.NewPassword, user.PasswordHash))
        {
            return BadRequest("New password cannot be the same as the current password.");
        }

        // Update password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

        await _context.SaveChangesAsync();
        await _auditService.LogAsync(
     user.Email,
      user.Role,
     "Password Changed",
     "User",
     $"Password changed successfully for '{user.Email}'.",
     HttpContext.Connection.RemoteIpAddress?.ToString());

        return Ok(new
        {
            Message = "Password changed successfully."
        });
    }

    [AllowAnonymous]
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail(VerifyEmailDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.EmailVerificationToken == dto.Token);

        if (user == null)
            return BadRequest("Invalid verification token.");

        if (user.EmailVerificationTokenExpiry < DateTime.UtcNow)
            return BadRequest("Verification token expired.");

        user.EmailConfirmed = true;
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpiry = null;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            Message = "Email verified successfully."
        });
    }
    //[Authorize]
    //[HttpPost("logout")]
    //public async Task<IActionResult> Logout()
    //{
    //    return Ok("Controller reached");
    //}
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var email =
    User.FindFirst(ClaimTypes.Email)?.Value ??
    User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
        if (string.IsNullOrEmpty(email))
            return Unauthorized();

        var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);

        if (user == null)
            return Unauthorized();

        // Remove Refresh Token
        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = DateTime.MinValue;

        await _context.SaveChangesAsync();
        await _auditService.LogAsync(
    user.Email,
     user.Role,
    "Logout",
    "User",
    $"User '{user.Email}' logged out successfully.",
    HttpContext.Connection.RemoteIpAddress?.ToString());
        return Ok(new
        {
            Message = "Logged out successfully."
        });
    }
    //[Authorize]
    //[HttpPost("logout")]
    //public IActionResult Logout()
    //{
    //    return Ok(new
    //    {
    //        Message = "Logout endpoint reached",
    //        Claims = User.Claims.Select(c => new
    //        {
    //            c.Type,
    //            c.Value
    //        })
    //    });
    //}

}