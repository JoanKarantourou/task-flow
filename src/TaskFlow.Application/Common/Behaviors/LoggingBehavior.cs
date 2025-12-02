using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace TaskFlow.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior that logs all requests and their execution time.
/// Useful for debugging, monitoring, and performance analysis.
/// Runs AROUND the handler - logs before and after execution.
/// </summary>
/// <remarks>
/// What gets logged:
/// - Request type and data (before handler)
/// - Execution time (after handler)
/// - Success or failure status
/// - Exception details (if any)
/// 
/// Benefits:
/// - See every request flowing through the system
/// - Identify slow operations
/// - Debug issues in production
/// - Track performance over time
/// - No logging code in handlers
/// </remarks>
/// <typeparam name="TRequest">The type of request being logged</typeparam>
/// <typeparam name="TResponse">The type of response from the handler</typeparam>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    /// <summary>
    /// Constructor - DI injects the logger.
    /// Logger is typed to include request/response types in log context.
    /// </summary>
    /// <param name="logger">Logger instance from DI</param>
    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Logs request details, executes handler, and logs result with timing.
    /// This wraps around the handler execution.
    /// </summary>
    /// <param name="request">The request being processed</param>
    /// <param name="next">Delegate to call the next behavior or handler</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>The response from the handler</returns>
    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        // Step 1: Get the request type name for logging
        // Example: "CreateTaskCommand" or "GetTaskByIdQuery"
        var requestName = typeof(TRequest).Name;

        // Step 2: Log the incoming request
        // Use Information level for normal operations
        _logger.LogInformation(
            "Handling {RequestName} with data: {@Request}", 
            requestName, 
            request);

        // Step 3: Start timing the execution
        // Stopwatch measures how long the handler takes
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Step 4: Execute the next behavior or handler
            // This is where the actual work happens
            var response = await next();

            // Step 5: Stop timing
            stopwatch.Stop();

            // Step 6: Log successful completion with execution time
            _logger.LogInformation(
                "Handled {RequestName} successfully in {ElapsedMilliseconds}ms", 
                requestName, 
                stopwatch.ElapsedMilliseconds);

            // Optional: Log warning if request took too long
            // This helps identify performance issues
            if (stopwatch.ElapsedMilliseconds > 3000) // 3 seconds
            {
                _logger.LogWarning(
                    "Long running request: {RequestName} took {ElapsedMilliseconds}ms", 
                    requestName, 
                    stopwatch.ElapsedMilliseconds);
            }

            return response;
        }
        catch (Exception ex)
        {
            // Step 7: If handler throws exception, log it
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Error handling {RequestName} after {ElapsedMilliseconds}ms: {ErrorMessage}", 
                requestName, 
                stopwatch.ElapsedMilliseconds,
                ex.Message);

            // Re-throw the exception so it can be handled by exception middleware
            throw;
        }
    }
}
