using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace OpsFlow.Api.Middleware;

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
        var (statusCode, title) = exception switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, "Validation failed."),
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request."),
            UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "Forbidden."),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource not found."),
            InvalidOperationException => (StatusCodes.Status400BadRequest, "Invalid operation."),
            _ => (StatusCodes.Status500InternalServerError, "Unexpected error.")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception while processing request.");
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var payload = new
        {
            title,
            detail = exception.Message
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
