namespace Seatgenie.Api.Entities;

/// <summary>
/// A single user → desk-quality preference row (table: user_seat_preferences).
/// A user has a one-to-many set of these rows.
/// </summary>
public class UserSeatPreference
{
    /// <summary>seat_preference_id — serial primary key.</summary>
    public int Id { get; set; }

    /// <summary>user_id — text FK to <see cref="User"/>.Id.</summary>
    public string UserId { get; set; } = default!;

    /// <summary>quality_id — int FK to <see cref="DeskQuality"/>.Id.</summary>
    public int QualityId { get; set; }

    public User? User { get; set; }
    public DeskQuality? Quality { get; set; }
}
