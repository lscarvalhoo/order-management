using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OrderManagement.Application.Commands.Login;
using OrderManagement.Application.Interfaces;

namespace OrderManagement.UnitTests.Handlers;

public class LoginCommandHandlerTests
{
    private readonly Mock<IAuthenticationService> _mockAuthService;
    private readonly Mock<IJwtTokenService> _mockJwtService;
    private readonly Mock<ILogger<LoginCommandHandler>> _mockLogger;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _mockAuthService = new Mock<IAuthenticationService>();
        _mockJwtService = new Mock<IJwtTokenService>();
        _mockLogger = new Mock<ILogger<LoginCommandHandler>>();
        _handler = new LoginCommandHandler(_mockAuthService.Object, _mockJwtService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsTokenAndExpirationDate()
    {
        var command = new LoginCommand("dev@martech.com", "Senha@123");
        _mockAuthService
            .Setup(x => x.ValidateCredentials(command.Email, command.Password))
            .Returns(true);
        _mockAuthService
            .Setup(x => x.GetUserRole(command.Email))
            .Returns("Admin");
        _mockJwtService
            .Setup(x => x.GenerateToken(command.Email, "Admin"))
            .Returns("fake-jwt-token");
        var result = await _handler.Handle(command, CancellationToken.None);
        result.Should().NotBeNull();
        result.Token.Should().Be("fake-jwt-token");
        result.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddHours(8), TimeSpan.FromSeconds(5));

        _mockAuthService.Verify(x => x.ValidateCredentials(command.Email, command.Password), Times.Once);
        _mockAuthService.Verify(x => x.GetUserRole(command.Email), Times.Once);
        _mockJwtService.Verify(x => x.GenerateToken(command.Email, "Admin"), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidCredentials_ThrowsUnauthorizedAccessException()
    {
        var command = new LoginCommand("invalid@email.com", "wrongpassword");
        _mockAuthService
            .Setup(x => x.ValidateCredentials(command.Email, command.Password))
            .Returns(false);
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid username or password");

        _mockAuthService.Verify(x => x.ValidateCredentials(command.Email, command.Password), Times.Once);
        _mockAuthService.Verify(x => x.GetUserRole(It.IsAny<string>()), Times.Never);
        _mockJwtService.Verify(x => x.GenerateToken(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ValidCredentials_UserRole_ReturnsTokenWithUserRole()
    {
        var command = new LoginCommand("user@martech.com", "UserPass@123");
        _mockAuthService
            .Setup(x => x.ValidateCredentials(command.Email, command.Password))
            .Returns(true);
        _mockAuthService
            .Setup(x => x.GetUserRole(command.Email))
            .Returns("User");
        _mockJwtService
            .Setup(x => x.GenerateToken(command.Email, "User"))
            .Returns("user-jwt-token");
        var result = await _handler.Handle(command, CancellationToken.None);
        result.Should().NotBeNull();
        result.Token.Should().Be("user-jwt-token");
        _mockJwtService.Verify(x => x.GenerateToken(command.Email, "User"), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCredentials_LogsSuccessfulLogin()
    {
        var command = new LoginCommand("dev@martech.com", "Senha@123");
        _mockLogger.Setup(x => x.IsEnabled(LogLevel.Information)).Returns(true);
        _mockAuthService.Setup(x => x.ValidateCredentials(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        _mockAuthService.Setup(x => x.GetUserRole(It.IsAny<string>())).Returns("Admin");
        _mockJwtService.Setup(x => x.GenerateToken(It.IsAny<string>(), It.IsAny<string>())).Returns("token");

        await _handler.Handle(command, CancellationToken.None);

        var matchingInvocations = _mockLogger.Invocations
            .Where(invocation => invocation.Method.Name == nameof(ILogger.Log))
            .Where(invocation => invocation.Arguments.Count == 5)
            .Where(invocation => invocation.Arguments[0] is LogLevel && (LogLevel)invocation.Arguments[0] == LogLevel.Information)
            .Where(invocation => invocation.Arguments[2] is not null)
            .Where(invocation => invocation.Arguments[2]!.ToString()!.Contains("Login successful", StringComparison.Ordinal))
            .ToList();

        matchingInvocations.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_InvalidCredentials_LogsWarning()
    {
        var command = new LoginCommand("invalid@email.com", "wrong");
        _mockLogger.Setup(x => x.IsEnabled(LogLevel.Warning)).Returns(true);
        _mockAuthService.Setup(x => x.ValidateCredentials(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        try
        {
            await _handler.Handle(command, CancellationToken.None);
        }
        catch (UnauthorizedAccessException)
        {
        }

        var matchingInvocations = _mockLogger.Invocations
            .Where(invocation => invocation.Method.Name == nameof(ILogger.Log))
            .Where(invocation => invocation.Arguments.Count == 5)
            .Where(invocation => invocation.Arguments[0] is LogLevel && (LogLevel)invocation.Arguments[0] == LogLevel.Warning)
            .Where(invocation => invocation.Arguments[2] is not null)
            .Where(invocation => invocation.Arguments[2]!.ToString()!.Contains("Invalid credentials", StringComparison.Ordinal))
            .ToList();

        matchingInvocations.Should().ContainSingle();
    }
}
