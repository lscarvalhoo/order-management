using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OrderManagement.API.Configuration;
using OrderManagement.API.Services;
using OrderManagement.Application.Commands.Login;
using System.Text;

namespace OrderManagement.API.Extensions;

public static class AuthenticationServiceExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtKey = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT Key is not configured");
        var jwtIssuer = configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("JWT Issuer is not configured");
        var jwtAudience = configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("JWT Audience is not configured");

        services.Configure<DevelopmentAuthOptions>(
            configuration.GetSection(DevelopmentAuthOptions.SectionName));

        services.AddScoped<IAuthenticationService, DevelopmentAuthenticationService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
            };
        });

        services.AddAuthorization();

        return services;
    }
}
