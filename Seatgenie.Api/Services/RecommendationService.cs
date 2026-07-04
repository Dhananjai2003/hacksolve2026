using Seatgenie.Api.Models;
using Seatgenie.Api.Repositories;
using Ent = Seatgenie.Api.Entities;

namespace Seatgenie.Api.Services;

public interface IRecommendationService
{
    /// <summary>
    /// Recommend desks for the current user, in the user's current office: their most-reserved
    /// desk of the previous calendar month first (if free today), followed by other desks free
    /// today whose qualities match the user's seat preferences. Falls back to the first available
    /// seat when the user has no history or no preferences. Every returned desk is free today.
    /// </summary>
    Task<IReadOnlyList<DeskAvailability>> GetMyRecommendationsAsync(CancellationToken ct = default);

    /// <summary>
    /// Seats free today nearest to my teammates (users in my service center) who are booked today.
    /// Proximity is computed per floor from desk x/y; ranked by distance to the closest teammate,
    /// then by distance to the team's centroid. Returns up to <paramref name="limit"/> suggestions.
    /// </summary>
    Task<IReadOnlyList<DeskSuggestion>> GetSeatsNearMyTeamAsync(int limit, CancellationToken ct = default);
}

public class RecommendationService : IRecommendationService
{
    private readonly IUserRepository _users;
    private readonly IOfficeRepository _offices;
    private readonly IReservationRepository _reservations;
    private readonly IUserSeatPreferenceRepository _seatPreferences;
    private readonly ICurrentUserContext _currentUser;

    public RecommendationService(
        IUserRepository users,
        IOfficeRepository offices,
        IReservationRepository reservations,
        IUserSeatPreferenceRepository seatPreferences,
        ICurrentUserContext currentUser)
    {
        _users = users;
        _offices = offices;
        _reservations = reservations;
        _seatPreferences = seatPreferences;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<DeskAvailability>> GetMyRecommendationsAsync(CancellationToken ct = default)
    {
        var userId = _currentUser.RequireUserId();

        // Scope: the user's current office (all floors/desks, with quality mappings loaded).
        if (await _users.GetByIdAsync(userId, ct) is not { CurrentOfficeId: { } officeId })
        {
            return [];
        }

        if (await _offices.GetDetailAsync(officeId, ct) is not { } office)
        {
            return [];
        }

        var candidateDesks = office.Floors.SelectMany(f => f.Desks).ToList();
        if (candidateDesks.Count == 0)
        {
            return [];
        }

        var candidateById = candidateDesks.ToDictionary(d => d.Id);

        // Which of those desks are free today.
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var freeDeskIds = new HashSet<string>();
        foreach (var floor in office.Floors)
        {
            foreach (var slot in await _reservations.GetFloorAvailabilityAsync(floor.Id, today, ct))
            {
                if (slot.IsFree)
                {
                    freeDeskIds.Add(slot.Desk.Id);
                }
            }
        }

        var availableDesks = candidateDesks.Where(d => freeDeskIds.Contains(d.Id)).ToList();
        if (availableDesks.Count == 0)
        {
            return [];
        }

        var firstAvailableId = availableDesks.OrderBy(d => d.PublicDeskId).First().Id;

        // Every recommended desk is free today, so IsFree is always true here.
        IReadOnlyList<DeskAvailability> ToSuggestions(IEnumerable<string> deskIds) =>
            deskIds.Select(id => new DeskAvailability { Desk = candidateById[id].ToDto(), IsFree = true }).ToList();

        // User's preferred desk qualities and their most-reserved desk last calendar month.
        var preferredQualityIds = (await _seatPreferences.GetByUserAsync(userId, ct))
            .Select(p => p.QualityId)
            .ToHashSet();

        var (monthStart, monthEnd) = PreviousCalendarMonth();
        var mostReservedDeskId = await _reservations.GetMostReservedDeskIdAsync(userId, monthStart, monthEnd, ct);

        // No history or no preferences → just the first available seat.
        if (preferredQualityIds.Count == 0 || mostReservedDeskId is null)
        {
            return ToSuggestions([firstAvailableId]);
        }

        // Available desks that share at least one preferred quality, best match first.
        var preferenceMatches = availableDesks
            .Select(d => new
            {
                d.Id,
                d.PublicDeskId,
                MatchCount = d.QualityMappings.Count(m => preferredQualityIds.Contains(m.QualityId)),
            })
            .Where(x => x.MatchCount > 0)
            .OrderByDescending(x => x.MatchCount)
            .ThenBy(x => x.PublicDeskId)
            .Select(x => x.Id)
            .ToList();

        // Most-reserved desk first (only if it's in this office and free today), then matches.
        var orderedIds = new List<string>();
        if (freeDeskIds.Contains(mostReservedDeskId))
        {
            orderedIds.Add(mostReservedDeskId);
        }

        foreach (var id in preferenceMatches)
        {
            if (!orderedIds.Contains(id))
            {
                orderedIds.Add(id);
            }
        }

        // Nothing personalised was available → fall back to the first available seat.
        return ToSuggestions(orderedIds.Count > 0 ? orderedIds : [firstAvailableId]);
    }

    public async Task<IReadOnlyList<DeskSuggestion>> GetSeatsNearMyTeamAsync(int limit, CancellationToken ct = default)
    {
        if (limit <= 0)
        {
            limit = 5;
        }

        var userId = _currentUser.RequireUserId();

        // Team = users sharing my service center. No service center → no team.
        if (await _users.GetByIdAsync(userId, ct) is not { ServiceId: { } serviceId })
        {
            return [];
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var teammateDesks = await _reservations.GetTeamBookedDesksAsync(serviceId, userId, today, ct);
        if (teammateDesks.Count == 0)
        {
            return [];
        }

        // Rank free desks by proximity to teammates, per floor (x/y are floor-plan coords).
        var candidates = new List<(Ent.Desk Desk, double MinDistance, double CentroidDistance)>();
        foreach (var floorGroup in teammateDesks.GroupBy(t => t.FloorId))
        {
            var teammates = floorGroup.ToList();
            var centroidX = teammates.Average(t => t.X);
            var centroidY = teammates.Average(t => t.Y);

            foreach (var slot in await _reservations.GetFloorAvailabilityAsync(floorGroup.Key, today, ct))
            {
                if (!slot.IsFree)
                {
                    continue;
                }

                var desk = slot.Desk;
                var minDistance = teammates.Min(t => Distance(desk.X, desk.Y, t.X, t.Y));
                var centroidDistance = Distance(desk.X, desk.Y, centroidX, centroidY);
                candidates.Add((desk, minDistance, centroidDistance));
            }
        }

        return candidates
            .OrderBy(c => c.MinDistance)          // nearest single teammate
            .ThenBy(c => c.CentroidDistance)      // fall back to nearest to the team centroid
            .ThenBy(c => c.Desk.PublicDeskId)
            .Take(limit)
            .Select(c => new DeskSuggestion
            {
                Desk = c.Desk.ToDto(),
                IsFree = true,
                DistanceMeters = Math.Round(c.MinDistance, 2),
                Score = 1.0 / (1.0 + c.MinDistance),
                Reason = $"Near your team — {Math.Round(c.MinDistance, 1)} from your nearest teammate",
            })
            .ToList();
    }

    private static double Distance(double x1, double y1, double x2, double y2)
    {
        var dx = x1 - x2;
        var dy = y1 - y2;
        return Math.Sqrt((dx * dx) + (dy * dy));
    }

    /// <summary>[first day of last month 00:00 UTC, first day of this month 00:00 UTC).</summary>
    private static (DateTime Start, DateTime End) PreviousCalendarMonth()
    {
        var now = DateTime.UtcNow;
        var firstOfThisMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        return (firstOfThisMonth.AddMonths(-1), firstOfThisMonth);
    }
}
