using System.Text.Json;
using Microsoft.ApplicationInsights;
using Seatgenie.Api.Models;
using Seatgenie.Api.Services;

namespace Seatgenie.Api.Infrastructure;

/// <summary>
/// Translates service-layer domain exceptions into the API's <see cref="Error"/> payload
/// with the appropriate HTTP status code.
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Attach the caller id so App Insights traces/exceptions carry a UserId dimension.
        var userId = context.Request.Headers["X-User-Id"].FirstOrDefault();
        using var scope = string.IsNullOrWhiteSpace(userId)
            ? null
            : _logger.BeginScope(new Dictionary<string, object> { ["UserId"] = userId });

        try
        {
            await _next(context);
        }
        catch (NotAuthenticatedException ex)
        {
            await WriteError(context, StatusCodes.Status401Unauthorized, "UNAUTHORIZED", ex.Message);
        }
        catch (BookingConflictException ex)
        {
            await WriteError(context, StatusCodes.Status409Conflict, "CONFLICT", ex.Message);
        }
        catch (Exception ex)
        {
            // Report to App Insights when configured (no-op otherwise).
            context.RequestServices.GetService<TelemetryClient>()?.TrackException(ex);
            _logger.LogError(ex, "Unhandled exception");
            await WriteError(context, StatusCodes.Status500InternalServerError, "INTERNAL_ERROR", "An unexpected error occurred.");
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static async Task WriteError(HttpContext context, int statusCode, string code, string message)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        var payload = JsonSerializer.Serialize(new Error { Code = code, Message = message }, JsonOptions);
        await context.Response.WriteAsync(payload);
    }
}
