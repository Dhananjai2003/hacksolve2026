using System.Data.Common;
using System.Text.Json;
using Microsoft.ApplicationInsights;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Seatgenie.Api.Models;
using Seatgenie.Api.Services;

namespace Seatgenie.Api.Infrastructure;

/// <summary>
/// Translates service-layer domain exceptions and database errors into the API's
/// <see cref="Error"/> payload with an appropriate status code, so predictable failures
/// (validation, missing references, duplicates, DB outages) never surface as raw 500s.
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
        catch (ValidationException ex)
        {
            await WriteError(context, StatusCodes.Status400BadRequest, "VALIDATION_ERROR", ex.Message);
        }
        catch (ConflictException ex)
        {
            await WriteError(context, StatusCodes.Status409Conflict, "CONFLICT", ex.Message);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // Client disconnected / cancelled — no response needed.
        }
        catch (Exception ex)
        {
            // EF wraps DB failures (e.g. in InvalidOperationException "transient failure"),
            // so inspect the whole inner-exception chain rather than just the top type.
            if (FindInChain<PostgresException>(ex) is { } pg)
            {
                var (status, code, message) = MapPostgres(pg);
                _logger.LogWarning(ex, "Database constraint rejected ({SqlState})", pg.SqlState);
                await WriteError(context, status, code, message);
                return;
            }

            if (FindInChain<DbException>(ex) is not null)
            {
                // Connectivity / transport failures (host unreachable, timeout, auth).
                _logger.LogError(ex, "Database unavailable");
                await WriteError(context, StatusCodes.Status503ServiceUnavailable, "DATABASE_UNAVAILABLE",
                    "The database is currently unavailable. Please try again later.");
                return;
            }

            context.RequestServices.GetService<TelemetryClient>()?.TrackException(ex);
            _logger.LogError(ex, "Unhandled exception");
            await WriteError(context, StatusCodes.Status500InternalServerError, "INTERNAL_ERROR", "An unexpected error occurred.");
        }
    }

    private static T? FindInChain<T>(Exception? exception) where T : class
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is T match)
            {
                return match;
            }
        }

        return null;
    }

    /// <summary>Maps a PostgreSQL SQLSTATE to a client-facing status + message.</summary>
    private static (int Status, string Code, string Message) MapPostgres(PostgresException pg) => pg.SqlState switch
    {
        // https://www.postgresql.org/docs/current/errcodes-appendix.html
        PostgresErrorCodes.ForeignKeyViolation =>
            (StatusCodes.Status400BadRequest, "INVALID_REFERENCE", "The request references a resource that does not exist."),
        PostgresErrorCodes.UniqueViolation =>
            (StatusCodes.Status409Conflict, "DUPLICATE", "A resource with the same unique value already exists."),
        PostgresErrorCodes.NotNullViolation =>
            (StatusCodes.Status400BadRequest, "MISSING_FIELD", "A required field was missing."),
        PostgresErrorCodes.StringDataRightTruncation =>
            (StatusCodes.Status400BadRequest, "VALUE_TOO_LONG", "A field value exceeded the allowed length."),
        PostgresErrorCodes.CheckViolation =>
            (StatusCodes.Status400BadRequest, "INVALID_VALUE", "A field value violated a constraint."),
        _ => (StatusCodes.Status400BadRequest, "INVALID_REQUEST", "The request could not be processed."),
    };

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
