namespace Seatgenie.Api.Models;

/// <summary>A tenant organization.</summary>
public class Organization
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? InviteCode { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>Writable organization fields.</summary>
public class OrganizationInput
{
    public required string Name { get; set; }
    public string? Description { get; set; }
}

/// <summary>Request body for joining an organization by invite code.</summary>
public class JoinOrganizationInput
{
    public required string InviteCode { get; set; }
}

/// <summary>Response carrying a (re)generated invite code.</summary>
public class InviteCodeResponse
{
    public string? InviteCode { get; set; }
}
