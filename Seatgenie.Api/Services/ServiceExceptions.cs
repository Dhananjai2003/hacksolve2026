namespace Seatgenie.Api.Services;

/// <summary>Raised when a "me"-scoped operation runs without an identified caller (→ 401).</summary>
public class NotAuthenticatedException : Exception
{
    public NotAuthenticatedException()
        : base("No authenticated user. Supply the X-User-Id header until auth is wired up.") { }
}

/// <summary>Raised when a desk is already booked for the requested date (→ 409).</summary>
public class BookingConflictException : Exception
{
    public BookingConflictException(string message) : base(message) { }
}
