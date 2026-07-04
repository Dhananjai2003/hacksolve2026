namespace Seatgenie.Api.Entities;

/// <summary>Lookup of service centers (table: service_center_mapping). A user's serviceid points here.</summary>
public class ServiceCenterMapping
{
    /// <summary>id — primary key.</summary>
    public int Id { get; set; }

    /// <summary>service_center_name.</summary>
    public string? ServiceCenterName { get; set; }
}
