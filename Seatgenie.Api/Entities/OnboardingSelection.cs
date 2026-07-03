namespace Seatgenie.Api.Entities;

/// <summary>First-run selections per user (table: onboarding_selection).</summary>
public class OnboardingSelection : IEntity, IAuditable
{
    public string Id { get; set; } = default!;
    public string UserId { get; set; } = default!;

    /// <summary>
    /// Stored as a PostgreSQL text[] of <see cref="Models.WorkplacifyPreference"/> names.
    /// </summary>
    public List<string> WorkplacifyPreferences { get; set; } = new();
    public string? TemporaryInviteCode { get; set; }
    public bool Submitted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User? User { get; set; }
}
