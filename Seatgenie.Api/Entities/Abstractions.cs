namespace Seatgenie.Api.Entities;

/// <summary>An entity with a string primary key.</summary>
public interface IEntity
{
    string Id { get; set; }
}

/// <summary>An entity that tracks creation/update timestamps.</summary>
public interface IAuditable
{
    DateTimeOffset CreatedAt { get; set; }
    DateTimeOffset UpdatedAt { get; set; }
}
