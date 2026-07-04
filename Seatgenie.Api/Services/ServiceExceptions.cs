namespace Seatgenie.Api.Services;

/// <summary>Raised when a "me"-scoped operation runs without an identified caller (→ 401).</summary>
public class NotAuthenticatedException : Exception
{
    public NotAuthenticatedException()
        : base("No authenticated user. Supply the X-User-Id header until auth is wired up.") { }
}

/// <summary>Raised when a request conflicts with current state (→ 409).</summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}

/// <summary>Raised when a desk is already booked for the requested date (→ 409).</summary>
public class BookingConflictException : ConflictException
{
    public BookingConflictException(string message) : base(message) { }
}

/// <summary>Raised when a request fails a business/input rule (→ 400).</summary>
public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}
