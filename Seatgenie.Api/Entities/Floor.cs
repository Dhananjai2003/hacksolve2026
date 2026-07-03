namespace Seatgenie.Api.Entities;

/// <summary>Floor within an office (table: floor).</summary>
public class Floor : IEntity, IAuditable
{
    public string Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string? FloorPlan { get; set; }
    public string? OfficeId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public Office? Office { get; set; }
    public ICollection<Desk> Desks { get; set; } = new List<Desk>();
    public ICollection<MeetingRoom> MeetingRooms { get; set; } = new List<MeetingRoom>();
    public ICollection<OfficeRoom> OfficeRooms { get; set; } = new List<OfficeRoom>();
}

/// <summary>Meeting room on a floor (table: meeting_room).</summary>
public class MeetingRoom : IEntity, IAuditable
{
    public string Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string FloorId { get; set; } = default!;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public Floor? Floor { get; set; }
}

/// <summary>Office room on a floor (table: office_room).</summary>
public class OfficeRoom : IEntity, IAuditable
{
    public string Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string FloorId { get; set; } = default!;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public Floor? Floor { get; set; }
}
