namespace Seatgenie.Api.Models;

/// <summary>Desk master data and floor-plan coordinates.</summary>
public class Desk
{
    public string? Id { get; set; }
    public string? PublicDeskId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? FloorId { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>Writable desk fields.</summary>
public class DeskInput
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
}

/// <summary>A single desk position update for the floor-plan editor.</summary>
public class DeskPosition
{
    public string? Id { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
}

/// <summary>A desk booking / schedule entry.</summary>
public class DeskSchedule
{
    public string? Id { get; set; }
    public string? DeskId { get; set; }
    public string? UserId { get; set; }
    public DateTime? Date { get; set; }
    public string? Timezone { get; set; }
    public bool WholeDay { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>A desk schedule entry including the desk it references.</summary>
public class DeskScheduleWithDesk : DeskSchedule
{
    public Desk? Desk { get; set; }
}

/// <summary>Availability of a single desk for a requested date.</summary>
public class DeskAvailability
{
    public Desk? Desk { get; set; }
    public bool IsFree { get; set; }
}

/// <summary>A persisted desk suggestion from the recommendation engine.</summary>
public class DeskRecommendation
{
    public string? Id { get; set; }
    public string? UserId { get; set; }
    public string? DeskId { get; set; }
    public DateTime? Date { get; set; }
    public double Score { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>A candidate desk with why-it-was-suggested context.</summary>
public class DeskSuggestion
{
    public Desk? Desk { get; set; }

    /// <summary>Ranking score (higher is better).</summary>
    public double Score { get; set; }

    /// <summary>Approx distance to team/colleague (proximity intents).</summary>
    public double? DistanceMeters { get; set; }

    /// <summary>Availability for the requested date.</summary>
    public bool IsFree { get; set; }

    /// <example>2 desks from your teammate Ada; free all day</example>
    public string? Reason { get; set; }
}

/// <summary>The user's favourite (most-booked) desk plus supporting stats.</summary>
public class FavoriteDesk
{
    public Desk? Desk { get; set; }

    /// <summary>Times this desk was booked in the look-back window.</summary>
    public int BookingCount { get; set; }
    public DateOnly? LastBookedDate { get; set; }

    /// <summary>Whether it came from booking history or saved preference.</summary>
    public FavoriteDeskSource Source { get; set; }
}
