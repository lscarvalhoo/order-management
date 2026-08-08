using MediatR;

namespace OrderManagement.Application.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<LoginCommandResult>;

public record LoginCommandResult(string Token, DateTime ExpiresAt);
