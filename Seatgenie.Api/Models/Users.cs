namespace Seatgenie.Api.Models;

/// <summary>An application user.</summary>
public class User
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public DateTime? EmailVerified { get; set; }
    public string? Image { get; set; }
    public string? OrganizationId { get; set; }
    public UserRole UserRole { get; set; }
    public string? CurrentOfficeId { get; set; }
}

/// <summary>Editable fields on the current user's profile.</summary>
public class UserUpdate
{
    public string? Name { get; set; }
    public string? Image { get; set; }
    public string? CurrentOfficeId { get; set; }
}

/// <summary>Per-user seating preferences (feeds the recommendation engine).</summary>
public class UserPreference
{
    public string? Id { get; set; }
    public string? UserId { get; set; }
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
}

/// <summary>Writable per-user seating preferences.</summary>
public class UserPreferenceInput
{
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
}
