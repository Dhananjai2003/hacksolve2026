using Seatgenie.Api.Models;

namespace Seatgenie.Api.Entities;

/// <summary>Application user (table: user).</summary>
public class User : IEntity
{
    public string Id { get; set; } = default!;
    public string? Name { get; set; }
    public string? Email { get; set; }
    public DateTime? EmailVerified { get; set; }
    public string? Image { get; set; }
    public string? OrganizationId { get; set; }
    public UserRole UserRole { get; set; } = UserRole.MEMBER;
    public string? CurrentOfficeId { get; set; }

    public Organization? Organization { get; set; }
    public Office? CurrentOffice { get; set; }
    public ICollection<Account> Accounts { get; set; } = new List<Account>();
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
    public OnboardingSelection? OnboardingSelection { get; set; }
    public UserPreference? UserPreference { get; set; }
    public ICollection<DeskSchedule> DeskSchedules { get; set; } = new List<DeskSchedule>();
}

/// <summary>OAuth / provider account linked to a user (table: account, NextAuth).</summary>
public class Account : IEntity
{
    public string Id { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public string? Type { get; set; }
    public string? Provider { get; set; }
    public string? ProviderAccountId { get; set; }
    public string? RefreshToken { get; set; }
    public string? AccessToken { get; set; }
    public int? ExpiresAt { get; set; }
    public string? TokenType { get; set; }
    public string? Scope { get; set; }
    public string? IdToken { get; set; }
    public string? SessionState { get; set; }
    public int? ExtExpiresIn { get; set; }

    public User? User { get; set; }
}

/// <summary>Authenticated session (table: session, NextAuth).</summary>
public class Session : IEntity
{
    public string Id { get; set; } = default!;
    public string SessionToken { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public DateTime Expires { get; set; }

    public User? User { get; set; }
}

/// <summary>Email verification token (table: verification_token, NextAuth). Keyed by (identifier, token).</summary>
public class VerificationToken
{
    public string Identifier { get; set; } = default!;
    public string Token { get; set; } = default!;
    public DateTime Expires { get; set; }
}
