namespace Seatgenie.Api.Models;

/// <summary>A floor within an office.</summary>
public class Floor
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }

    /// <summary>Floor-plan image (URL or encoded data).</summary>
    public string? FloorPlan { get; set; }
    public string? OfficeId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>Floor including its desks.</summary>
public class FloorDetail : Floor
{
    public List<Desk> Desks { get; set; } = new();
}

/// <summary>Writable floor fields.</summary>
public class FloorInput
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? FloorPlan { get; set; }
}
