using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace OrderManagement.Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation(
                "Handling {RequestName} - Request: {@Request}",
                requestName,
                request);
        }

        var response = await next(cancellationToken);

        stopwatch.Stop();

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation(
                "Handled {RequestName} in {ElapsedMilliseconds}ms - Response: {@Response}",
                requestName,
                stopwatch.ElapsedMilliseconds,
                response);
        }

        return response;
    }
}
