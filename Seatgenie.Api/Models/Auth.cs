namespace Seatgenie.Api.Models;

/// <summary>Current session and user.</summary>
public class SessionResponse
{
    public User? User { get; set; }
    public DateTimeOffset Expires { get; set; }
}

/// <summary>Request body to begin sign-in.</summary>
public class SignInRequest
{
    /// <example>azure-ad</example>
    public string? Provider { get; set; }
}
