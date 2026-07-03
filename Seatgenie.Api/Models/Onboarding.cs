namespace Seatgenie.Api.Models;

/// <summary>First-run selections captured per user.</summary>
public class OnboardingSelection
{
    public string? Id { get; set; }
    public string? UserId { get; set; }
    public List<WorkplacifyPreference> WorkplacifyPreferences { get; set; } = new();
    public string? TemporaryInviteCode { get; set; }
    public bool Submitted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>Writable onboarding selection fields.</summary>
public class OnboardingSelectionInput
{
    public List<WorkplacifyPreference> WorkplacifyPreferences { get; set; } = new();
    public string? TemporaryInviteCode { get; set; }
}
