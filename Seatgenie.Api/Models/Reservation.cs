namespace Seatgenie.Api.Models;

/// <summary>Request body to create or update a desk reservation.</summary>
public class ReservationInput
{
    public required string DeskId { get; set; }
    public DateOnly? Date { get; set; }
    public bool WholeDay { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? Timezone { get; set; }
}

/// <summary>Result of a pre-flight conflict check.</summary>
public class ConflictResult
{
    public bool HasConflict { get; set; }
}
