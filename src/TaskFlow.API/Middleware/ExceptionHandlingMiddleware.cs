using System.Net;
using System.Text.Json;
using FluentValidation;

namespace TaskFlow.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, response) = exception switch
        {
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                new ErrorResponse
                {
                    Message = "One or more validation errors occurred.",
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Errors = validationEx.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray()
                        )
                }
            ),

            UnauthorizedAccessException unauthorizedEx => (
                HttpStatusCode.Unauthorized,
                new ErrorResponse
                {
                    Message = unauthorizedEx.Message.Length > 0
                        ? unauthorizedEx.Message
                        : "You are not authorized to perform this action.",
                    StatusCode = (int)HttpStatusCode.Unauthorized
                }
            ),

            KeyNotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                new ErrorResponse
                {
                    Message = notFoundEx.Message.Length > 0
                        ? notFoundEx.Message
                        : "The requested resource was not found.",
                    StatusCode = (int)HttpStatusCode.NotFound
                }
            ),

            ArgumentException argumentEx => (
                HttpStatusCode.BadRequest,
                new ErrorResponse
                {
                    Message = argumentEx.Message,
                    StatusCode = (int)HttpStatusCode.BadRequest
                }
            ),

            InvalidOperationException invalidOpEx => (
                HttpStatusCode.Conflict,
                new ErrorResponse
                {
                    Message = invalidOpEx.Message,
                    StatusCode = (int)HttpStatusCode.Conflict
                }
            ),

            _ => (
                HttpStatusCode.InternalServerError,
                new ErrorResponse
                {
                    Message = "An unexpected error occurred. Please try again later.",
                    StatusCode = (int)HttpStatusCode.InternalServerError
                }
            )
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception occurred");
        }
        else
        {
            _logger.LogWarning(exception, "Handled exception: {ExceptionType}", exception.GetType().Name);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}

public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
}
