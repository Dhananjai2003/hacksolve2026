namespace Seatgenie.Api.Services;

/// <summary>
/// Supplies the id of the caller for "me"-scoped operations.
/// </summary>
/// <remarks>
/// Authentication is not wired up yet, so the id is read from the <c>X-User-Id</c>
/// request header. Replace this with the authenticated principal (NextAuth session /
/// Microsoft Entra) once auth is in place — the service layer needs no changes.
/// </remarks>
public interface ICurrentUserContext
{
    string? UserId { get; }

    /// <summary>Returns the current user id or throws <see cref="NotAuthenticatedException"/>.</summary>
    string RequireUserId();
}

public class CurrentUserContext : ICurrentUserContext
{
    private const string UserIdHeader = "X-User-Id";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserContext(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    public string? UserId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.Request.Headers[UserIdHeader].FirstOrDefault();
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }
    }

    public string RequireUserId() => UserId ?? throw new NotAuthenticatedException();
}
