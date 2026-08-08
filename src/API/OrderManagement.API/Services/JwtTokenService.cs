using Microsoft.IdentityModel.Tokens;
using OrderManagement.Application.Commands.Login;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OrderManagement.API.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(string email, string role = "User")
    {
        var jwtKey = _configuration["Jwt:Key"] 
            ?? throw new InvalidOperationException("JWT Key is not configured");
        var jwtIssuer = _configuration["Jwt:Issuer"] 
            ?? throw new InvalidOperationException("JWT Issuer is not configured");
        var jwtAudience = _configuration["Jwt:Audience"] 
            ?? throw new InvalidOperationException("JWT Audience is not configured");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, email),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, email)
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}