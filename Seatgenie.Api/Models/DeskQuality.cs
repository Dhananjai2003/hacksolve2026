namespace Seatgenie.Api.Models;

/// <summary>A desk quality (a tag such as "Standing desk", "Window", "Dual monitor").</summary>
public class DeskQuality
{
    public int QualityId { get; set; }
    public string? QualityName { get; set; }
}

/// <summary>Writable desk-quality fields.</summary>
public class DeskQualityInput
{
    /// <summary>Display name of the quality.</summary>
    public required string QualityName { get; set; }
}
