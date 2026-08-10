using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OrderManagement.Application.Commands.Login;
using OrderManagement.Infrastructure.Configuration;
using OrderManagement.Infrastructure.Services;
using System.Security.Cryptography;
using System.Text;

namespace OrderManagement.API.Extensions;

public static class AuthenticationServiceExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        jwtOptions.Key = ResolveSigningKey(jwtOptions.Key, environment);
        jwtOptions.Issuer = RequireValue(jwtOptions.Issuer, "JWT issuer");
        jwtOptions.Audience = RequireValue(jwtOptions.Audience, "JWT audience");

        services.AddSingleton(Options.Create(jwtOptions));
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
                ValidIssuer = jwtOptions.Issuer,
                ValidAudience = jwtOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key))
            };
        });

        services.AddAuthorization();

        return services;
    }

    private static string RequireValue(string? value, string settingName) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new InvalidOperationException($"{settingName} is not configured")
            : value;

    private static string ResolveSigningKey(string? configuredKey, IHostEnvironment environment)
    {
        if (!string.IsNullOrWhiteSpace(configuredKey))
        {
            return configuredKey;
        }

        if (environment.IsDevelopment() || environment.IsEnvironment("Testing"))
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }

        throw new InvalidOperationException("JWT signing key is not configured");
    }
}
