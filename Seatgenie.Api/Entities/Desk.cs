namespace Seatgenie.Api.Entities;

/// <summary>Desk master data and floor-plan coordinates (table: desk).</summary>
public class Desk : IEntity, IAuditable
{
    public string Id { get; set; } = default!;
    public string PublicDeskId { get; set; } = default!;
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string FloorId { get; set; } = default!;
    public double X { get; set; }
    public double Y { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Floor? Floor { get; set; }
    public ICollection<DeskSchedule> DeskSchedules { get; set; } = new List<DeskSchedule>();
    public ICollection<DeskRecommendation> DeskRecommendations { get; set; } = new List<DeskRecommendation>();
}

/// <summary>Desk booking / schedule entry (table: desk_schedule).</summary>
public class DeskSchedule : IEntity, IAuditable
{
    public string Id { get; set; } = default!;
    public string DeskId { get; set; } = default!;
    public string? UserId { get; set; }
    public DateTime? Date { get; set; }
    public string Timezone { get; set; } = "Etc/GMT";
    public bool WholeDay { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Desk? Desk { get; set; }
    public User? User { get; set; }
}

/// <summary>Persisted desk recommendation from the engine (table: desk_recommendation).</summary>
public class DeskRecommendation : IEntity, IAuditable
{
    public string Id { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public string DeskId { get; set; } = default!;
    public DateTime? Date { get; set; }
    public double Score { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User? User { get; set; }
    public Desk? Desk { get; set; }
}
