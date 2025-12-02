using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace TaskFlow.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior that monitors request performance and logs warnings for slow operations.
/// This is OPTIONAL but highly recommended for production monitoring.
/// Focuses specifically on performance, while LoggingBehavior handles general logging.
/// </summary>
/// <remarks>
/// Use cases:
/// - Identify performance bottlenecks
/// - Monitor API response times
/// - Alert on slow operations
/// - Track performance degradation over time
/// - Optimize based on real metrics
/// 
/// Difference from LoggingBehavior:
/// - LoggingBehavior: Logs ALL requests (info level)
/// - PerformanceBehavior: Only logs SLOW requests (warning level)
/// 
/// You can use both together for comprehensive monitoring.
/// </remarks>
/// <typeparam name="TRequest">The type of request being monitored</typeparam>
/// <typeparam name="TResponse">The type of response from the handler</typeparam>
public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
    private readonly Stopwatch _timer;

    /// <summary>
    /// Performance threshold in milliseconds.
    /// Requests taking longer than this will trigger a warning.
    /// Default: 500ms (half a second)
    /// </summary>
    private const int PerformanceThresholdMs = 500;

    /// <summary>
    /// Constructor - DI injects the logger.
    /// </summary>
    /// <param name="logger">Logger instance from DI</param>
    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
        _timer = new Stopwatch();
    }

    /// <summary>
    /// Measures execution time and logs warning if it exceeds threshold.
    /// Only logs when performance is below expectations.
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
        // Step 1: Start timing
        _timer.Start();

        // Step 2: Execute the handler
        var response = await next();

        // Step 3: Stop timing
        _timer.Stop();

        // Step 4: Check if execution time exceeds threshold
        var elapsedMilliseconds = _timer.ElapsedMilliseconds;

        if (elapsedMilliseconds > PerformanceThresholdMs)
        {
            // Step 5: Log warning for slow operation
            var requestName = typeof(TRequest).Name;

            _logger.LogWarning(
                "Long Running Request: {RequestName} took {ElapsedMilliseconds}ms (threshold: {Threshold}ms). Request data: {@Request}",
                requestName,
                elapsedMilliseconds,
                PerformanceThresholdMs,
                request);

            // Optional: You could also:
            // - Send metrics to Application Insights
            // - Trigger alerts in monitoring system
            // - Log to separate performance log file
            // - Increment a performance counter
            
            // Example with structured logging for metrics systems:
            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["RequestType"] = requestName,
                ["Duration"] = elapsedMilliseconds,
                ["Threshold"] = PerformanceThresholdMs
            }))
            {
                // Metrics systems can parse this structured data
            }
        }

        return response;
    }
}
