using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OrderManagement.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OrderManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // Usuário fixo em memória para demonstração
        if (request.Username == "admin" && request.Password == "admin123")
        {
            var token = GenerateJwtToken(request.Username);
            var expiresAt = DateTime.UtcNow.AddHours(8);

            return Ok(new LoginResponse
            {
                Token = token,
                ExpiresAt = expiresAt
            });
        }

        return Unauthorized(new { message = "Invalid username or password" });
    }

    private string GenerateJwtToken(string username)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "YourSuperSecretKeyForJWTTokenGenerationWithMinimum32Characters"));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "OrderManagementAPI",
            audience: _configuration["Jwt:Audience"] ?? "OrderManagementClient",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
