namespace Seatgenie.Api.Entities;

/// <summary>Office within an organization (table: office).</summary>
public class Office : IEntity, IAuditable
{
    public string Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public double? Longitude { get; set; }
    public double? Latitude { get; set; }
    public string Timezone { get; set; } = "Etc/GMT";
    public string? OrganizationId { get; set; }
    public string? OfficeSettingId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Organization? Organization { get; set; }
    public OfficeSetting? OfficeSetting { get; set; }
    public ICollection<Floor> Floors { get; set; } = new List<Floor>();
}

/// <summary>Booking rules for an office (table: office_setting).</summary>
public class OfficeSetting : IEntity, IAuditable
{
    public string Id { get; set; } = default!;
    public bool AllowSchedulingInThePast { get; set; }
    public int? DurationSchedulingFuture { get; set; }
    public string? OfficeSettingWeekdaysAllowedId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public OfficeSettingWeekdaysAllowed? WeekdaysAllowed { get; set; }
    public ICollection<Office> Offices { get; set; } = new List<Office>();
}

/// <summary>Allowed booking weekdays for an office setting (table: office_setting_weekdays_allowed).</summary>
public class OfficeSettingWeekdaysAllowed : IEntity
{
    public string Id { get; set; } = default!;
    public bool AllowMonday { get; set; }
    public bool AllowTuesday { get; set; }
    public bool AllowWednesday { get; set; }
    public bool AllowThursday { get; set; }
    public bool AllowFriday { get; set; }
    public bool AllowSaturday { get; set; }
    public bool AllowSunday { get; set; }
}
