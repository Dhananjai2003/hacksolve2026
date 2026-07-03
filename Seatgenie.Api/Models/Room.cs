namespace Seatgenie.Api.Models;

/// <summary>A meeting room or office room on a floor.</summary>
public class Room
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? FloorId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>Writable room fields.</summary>
public class RoomInput
{
    public required string Name { get; set; }
    public string? Description { get; set; }
}
