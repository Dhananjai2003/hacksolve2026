namespace Seatgenie.Api.Models;

/// <summary>A single user seat-preference row (user mapped to a desk quality).</summary>
public class UserSeatPreference
{
    public int SeatPreferenceId { get; set; }
    public int QualityId { get; set; }
    public string? QualityName { get; set; }
}

/// <summary>Request body carrying the desk-quality ids to map to the current user.</summary>
public class UserSeatPreferenceInput
{
    /// <summary>Desk-quality ids to map to the user (one-to-many).</summary>
    public List<int> QualityIds { get; set; } = new();
}
