namespace Seatgenie.Api.Models;

/// <summary>Standard API error payload.</summary>
public class Error
{
    /// <example>FORBIDDEN</example>
    public string? Code { get; set; }

    public string? Message { get; set; }
}
