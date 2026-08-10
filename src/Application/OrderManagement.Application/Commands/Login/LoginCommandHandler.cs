using MediatR;
using Microsoft.Extensions.Logging;
using OrderManagement.Application.Interfaces;

namespace OrderManagement.Application.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginCommandResult>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IAuthenticationService authenticationService,
        IJwtTokenService jwtTokenService,
        ILogger<LoginCommandHandler> logger)
    {
        _authenticationService = authenticationService;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public Task<LoginCommandResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Processing login command for user: {Email}", request.Email);
        }

        if (!_authenticationService.ValidateCredentials(request.Email, request.Password))
        {
            _logger.LogWarning("Invalid credentials for user: {Email}", request.Email);
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        var role = _authenticationService.GetUserRole(request.Email);
        var token = _jwtTokenService.GenerateToken(request.Email, role);
        var expiresAt = DateTime.UtcNow.AddHours(8);

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Login successful for user: {Email}", request.Email);
        }

        return Task.FromResult(new LoginCommandResult(token, expiresAt));
    }
}
