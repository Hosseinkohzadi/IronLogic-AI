using System.Net;
using System.Text.Json;

namespace IronLogic.Api.Middleware;

/// <summary>
/// Global exception handler middleware that catches unhandled exceptions and returns consistent JSON error responses
/// </summary>
public class GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger)
{
    /// <summary>
    /// Processes the HTTP request and handles any unhandled exceptions
    /// </summary>
    /// <param name="context">The HTTP context</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// Converts exceptions into consistent JSON error responses for Angular interceptor
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <param name="exception">The exception that occurred</param>
    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = exception switch
        {
            ArgumentException or ArgumentNullException => 
                (HttpStatusCode.BadRequest, exception.Message),
            
            UnauthorizedAccessException => 
                (HttpStatusCode.Unauthorized, "You are not authorized to perform this action"),
            
            KeyNotFoundException or FileNotFoundException => 
                (HttpStatusCode.NotFound, "The requested resource was not found"),
            
            InvalidOperationException => 
                (HttpStatusCode.Conflict, exception.Message),
            
            _ => (HttpStatusCode.InternalServerError, "An error occurred while processing your request")
        };

        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            message = message,
            statusCode = (int)statusCode,
            timestamp = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}

/// <summary>
/// Extension methods for registering the global exception handler middleware
/// </summary>
public static class GlobalExceptionHandlerExtensions
{
    /// <summary>
    /// Adds the global exception handler middleware to the application pipeline
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <returns>The application builder for chaining</returns>
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionHandler>();
    }
}
