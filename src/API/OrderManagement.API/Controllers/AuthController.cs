using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.API.Models;
using OrderManagement.Application.Commands.Login;

namespace OrderManagement.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Endpoint de autenticação - Credenciais configuradas em appsettings.Development.json
    /// </summary>
    /// <param name="request">Credenciais de login</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Token JWT e data de expiração</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new LoginCommand(request.Username, request.Password);
            var result = await _mediator.Send(command, cancellationToken);

            return Ok(new LoginResponse
            {
                Token = result.Token,
                ExpiresAt = result.ExpiresAt
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized login attempt");
            return Unauthorized(new { message = ex.Message });
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error during login");
            return BadRequest(new { message = ex.Message, errors = ex.Errors });
        }
    }
}
