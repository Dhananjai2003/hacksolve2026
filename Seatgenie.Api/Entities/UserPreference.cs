namespace Seatgenie.Api.Entities;

/// <summary>Per-user seating preferences (table: user_preference).</summary>
public class UserPreference : IEntity, IAuditable
{
    public string Id { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public string? PreferredOfficeId { get; set; }
    public string? PreferredFloorId { get; set; }
    public string? FavoriteDeskId { get; set; }
    public bool PreferMonday { get; set; }
    public bool PreferTuesday { get; set; }
    public bool PreferWednesday { get; set; }
    public bool PreferThursday { get; set; }
    public bool PreferFriday { get; set; }
    public bool PreferSaturday { get; set; }
    public bool PreferSunday { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User? User { get; set; }
    public Office? PreferredOffice { get; set; }
    public Floor? PreferredFloor { get; set; }
    public Desk? FavoriteDesk { get; set; }
}
