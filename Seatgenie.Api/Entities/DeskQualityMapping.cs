namespace Seatgenie.Api.Entities;

/// <summary>
/// Join row giving a desk one of its qualities (table: desk_quality_mapping).
/// A desk has a one-to-many set of these rows.
/// </summary>
public class DeskQualityMapping
{
    /// <summary>mapping_id — serial primary key.</summary>
    public int Id { get; set; }

    /// <summary>desk_id — text FK to <see cref="Desk"/>.Id.</summary>
    public string DeskId { get; set; } = default!;

    /// <summary>quality_id — int FK to <see cref="DeskQuality"/>.Id.</summary>
    public int QualityId { get; set; }

    public Desk? Desk { get; set; }
    public DeskQuality? Quality { get; set; }
}
