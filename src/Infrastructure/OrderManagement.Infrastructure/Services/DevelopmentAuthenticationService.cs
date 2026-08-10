using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderManagement.Application.Interfaces;
using OrderManagement.Infrastructure.Configuration;

namespace OrderManagement.Infrastructure.Services;

public class DevelopmentAuthenticationService : IAuthenticationService
{
    private readonly DevelopmentAuthOptions _authOptions;
    private readonly ILogger<DevelopmentAuthenticationService> _logger;

    public DevelopmentAuthenticationService(
        IOptions<DevelopmentAuthOptions> authOptions,
        ILogger<DevelopmentAuthenticationService> logger)
    {
        _authOptions = authOptions.Value;
        _logger = logger;
    }

    public bool ValidateCredentials(string email, string password)
    {
        var isValid = _authOptions.FixedUser.Email.Equals(email, StringComparison.OrdinalIgnoreCase) &&
                      _authOptions.FixedUser.Password == password;

        if (isValid)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Credentials validated successfully for user: {Email}", email);
            }
        }
        else
        {
            _logger.LogWarning("Failed credential validation attempt for user: {Email}", email);
        }

        return isValid;
    }

    public string GetUserRole(string email)
    {
        if (_authOptions.FixedUser.Email.Equals(email, StringComparison.OrdinalIgnoreCase))
        {
            return _authOptions.FixedUser.Role;
        }

        return "User";
    }
}
