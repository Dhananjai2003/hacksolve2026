using Seatgenie.Api.Repositories;

namespace Seatgenie.Api.Services;

public interface IRecommendationService
{
    /// <summary>
    /// Recommend preferred desk ids for the current user, in the user's current office:
    /// their most-reserved desk of the previous calendar month first (if free today),
    /// followed by other desks free today whose qualities match the user's seat preferences.
    /// Falls back to the first available seat when the user has no history or no preferences.
    /// </summary>
    Task<IReadOnlyList<string>> GetMyRecommendationsAsync(CancellationToken ct = default);
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

    public async Task<IReadOnlyList<string>> GetMyRecommendationsAsync(CancellationToken ct = default)
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

        // User's preferred desk qualities and their most-reserved desk last calendar month.
        var preferredQualityIds = (await _seatPreferences.GetByUserAsync(userId, ct))
            .Select(p => p.QualityId)
            .ToHashSet();

        var (monthStart, monthEnd) = PreviousCalendarMonth();
        var mostReservedDeskId = await _reservations.GetMostReservedDeskIdAsync(userId, monthStart, monthEnd, ct);

        // No history or no preferences → just the first available seat.
        if (preferredQualityIds.Count == 0 || mostReservedDeskId is null)
        {
            return [firstAvailableId];
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
        var result = new List<string>();
        if (freeDeskIds.Contains(mostReservedDeskId))
        {
            result.Add(mostReservedDeskId);
        }

        foreach (var id in preferenceMatches)
        {
            if (!result.Contains(id))
            {
                result.Add(id);
            }
        }

        // Nothing personalised was available → fall back to the first available seat.
        return result.Count > 0 ? result : [firstAvailableId];
    }

    /// <summary>[first day of last month 00:00 UTC, first day of this month 00:00 UTC).</summary>
    private static (DateTime Start, DateTime End) PreviousCalendarMonth()
    {
        var now = DateTime.UtcNow;
        var firstOfThisMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        return (firstOfThisMonth.AddMonths(-1), firstOfThisMonth);
    }
}
