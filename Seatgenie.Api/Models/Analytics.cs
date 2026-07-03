namespace Seatgenie.Api.Models;

/// <summary>A single point in a desk-utilization series.</summary>
public class UtilizationPoint
{
    public DateOnly Date { get; set; }

    /// <summary>Occupancy rate for the day (0.0 - 1.0).</summary>
    public double OccupancyRate { get; set; }
}

/// <summary>In-office people analytics for an office on a given date.</summary>
public class PeopleAnalytics
{
    public DateOnly Date { get; set; }

    /// <summary>Distinct people booked in the office on the date.</summary>
    public int TotalInOffice { get; set; }

    /// <summary>Headcount keyed by floor id.</summary>
    public Dictionary<string, int> ByFloor { get; set; } = new();
}
