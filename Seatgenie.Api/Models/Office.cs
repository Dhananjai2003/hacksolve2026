namespace Seatgenie.Api.Models;

/// <summary>An office within an organization.</summary>
public class Office
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public double? Longitude { get; set; }
    public double? Latitude { get; set; }

    /// <example>Europe/Brussels</example>
    public string? Timezone { get; set; }
    public string? OrganizationId { get; set; }
    public string? OfficeSettingId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

/// <summary>Office including its floors and desks.</summary>
public class OfficeDetail : Office
{
    public List<FloorDetail> Floors { get; set; } = new();
}

/// <summary>Writable office fields.</summary>
public class OfficeInput
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public double? Longitude { get; set; }
    public double? Latitude { get; set; }
    public string? Timezone { get; set; }
}

/// <summary>Weekdays on which booking is permitted for an office.</summary>
public class WeekdaysAllowed
{
    public string? Id { get; set; }
    public bool AllowMonday { get; set; }
    public bool AllowTuesday { get; set; }
    public bool AllowWednesday { get; set; }
    public bool AllowThursday { get; set; }
    public bool AllowFriday { get; set; }
    public bool AllowSaturday { get; set; }
    public bool AllowSunday { get; set; }
}

/// <summary>Booking rules for an office.</summary>
public class OfficeSetting
{
    public string? Id { get; set; }
    public bool AllowSchedulingInThePast { get; set; }
    public int? DurationSchedulingFuture { get; set; }
    public WeekdaysAllowed? WeekdaysAllowed { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

/// <summary>Writable office booking rules.</summary>
public class OfficeSettingInput
{
    public bool AllowSchedulingInThePast { get; set; }
    public int? DurationSchedulingFuture { get; set; }
}
