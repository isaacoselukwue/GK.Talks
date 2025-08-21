using MediatR;
using Microsoft.Extensions.Logging;

namespace GK.Talks.Application.Behaviours;
public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger = logger;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        _logger.LogInformation("Handling {RequestName} {@Request}", requestName, request);

        var response = await next(cancellationToken);

        _logger.LogInformation("Handled {RequestName} {@Response}", requestName, response);
        return response;
    }
}