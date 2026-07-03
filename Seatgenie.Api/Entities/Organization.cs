namespace Seatgenie.Api.Entities;

/// <summary>Tenant organization (table: organization).</summary>
public class Organization : IEntity, IAuditable
{
    public string Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string InviteCode { get; set; } = default!;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Office> Offices { get; set; } = new List<Office>();
}
