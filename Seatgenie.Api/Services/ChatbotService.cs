using Seatgenie.Api.Models;
using Seatgenie.Api.Repositories;
using Weekday = Seatgenie.Api.Models.Weekday;

namespace Seatgenie.Api.Services;

public interface IChatbotService
{
    IReadOnlyList<ChatIntent> ListIntents();
    Task<ChatMessageResponse> SendMessageAsync(ChatMessageRequest request, CancellationToken ct = default);
    Task<ChatExecuteResponse> ExecuteAsync(ChatExecuteRequest request, CancellationToken ct = default);

    Task<FavoriteDesk?> GetFavoriteDeskAsync(int sinceDays, CancellationToken ct = default);
    Task<DeskScheduleWithDesk?> GetLastBookingAsync(Weekday? weekday, CancellationToken ct = default);
    Task<IReadOnlyList<DeskSuggestion>> GetTeamDesksAsync(DateOnly date, string? colleagueId, int limit, CancellationToken ct = default);
    Task<DeskLookupResult?> LookupDeskAsync(string seatNumber, DateOnly date, bool fallbackToAvailable, CancellationToken ct = default);
    Task<IReadOnlyList<DeskScheduleWithDesk>> GetBookingHistoryAsync(int sinceDays, CancellationToken ct = default);
}

public class ChatbotService : IChatbotService
{
    private readonly IReservationRepository _reservations;
    private readonly IReservationService _reservationService;
    private readonly IDeskRepository _desks;
    private readonly IPreferenceRepository _preferences;
    private readonly ICurrentUserContext _currentUser;

    public ChatbotService(
        IReservationRepository reservations,
        IReservationService reservationService,
        IDeskRepository desks,
        IPreferenceRepository preferences,
        ICurrentUserContext currentUser)
    {
        _reservations = reservations;
        _reservationService = reservationService;
        _desks = desks;
        _preferences = preferences;
        _currentUser = currentUser;
    }

    // ---------------------------------------------------------------- Intents catalogue
    public IReadOnlyList<ChatIntent> ListIntents() =>
    [
        Intent(ChatIntentId.BOOK_FAVORITE_SEAT, "Book my favourite seat", false),
        Intent(ChatIntentId.FIND_SEAT_NEAR_TEAM, "Find a seat near my team", false),
        Intent(ChatIntentId.BOOK_SAME_AS_LAST_WEEKDAY, "Book the same seat I booked last Monday", true),
        Intent(ChatIntentId.FIND_FREE_SEAT_TODAY, "What desks are free today?", false),
        Intent(ChatIntentId.SHOW_MY_BOOKINGS, "Show my upcoming reservations", false),
        Intent(ChatIntentId.CANCEL_NEXT_BOOKING, "Cancel my next booking", false),
        Intent(ChatIntentId.REBOOK_LAST_SEAT, "Book the same seat as my last visit", false),
        Intent(ChatIntentId.BOOK_SPECIFIC_SEAT, "Book seat 112 (or a new available seat)", true),
        Intent(ChatIntentId.CANCEL_AND_REBOOK, "Cancel my last booking and book seat 112 or a new available seat", true),
    ];

    // ---------------------------------------------------------------- Message resolver
    public async Task<ChatMessageResponse> SendMessageAsync(ChatMessageRequest request, CancellationToken ct = default)
    {
        var date = request.Params?.Date ?? Today();
        var intent = request.IntentId ?? ChatIntentId.FIND_FREE_SEAT_TODAY;

        return intent switch
        {
            ChatIntentId.BOOK_FAVORITE_SEAT => await BookFavoriteAsync(date, ct),
            ChatIntentId.BOOK_SAME_AS_LAST_WEEKDAY => await BookSameAsLastWeekdayAsync(request.Params?.Weekday, date, ct),
            ChatIntentId.REBOOK_LAST_SEAT => await BookSameAsLastWeekdayAsync(null, date, ct),
            ChatIntentId.BOOK_SPECIFIC_SEAT => await BookSpecificSeatAsync(request.Params, date, ct),
            ChatIntentId.CANCEL_AND_REBOOK => await CancelAndRebookAsync(request.Params, date, ct),
            ChatIntentId.SHOW_MY_BOOKINGS => await ShowMyBookingsAsync(ct),
            ChatIntentId.CANCEL_NEXT_BOOKING => await CancelNextBookingAsync(ct),
            _ => await FindFreeSeatsAsync(date, ct),
        };
    }

    private async Task<ChatMessageResponse> BookFavoriteAsync(DateOnly date, CancellationToken ct)
    {
        var favorite = await GetFavoriteDeskAsync(180, ct);
        if (favorite?.Desk is null)
        {
            return Reply(ChatIntentId.BOOK_FAVORITE_SEAT, "I couldn't find a favourite seat from your history yet.");
        }

        return new ChatMessageResponse
        {
            IntentId = ChatIntentId.BOOK_FAVORITE_SEAT,
            Reply = $"Your favourite seat is {SeatLabel(favorite.Desk)} (booked {favorite.BookingCount} times). Book it for {date:ddd d MMM}?",
            Candidates = [Suggestion(favorite.Desk, "Your most-booked seat")],
            Action = BookAction(favorite.Desk.Id!, date, $"Book {SeatLabel(favorite.Desk)} on {date:ddd d MMM}"),
        };
    }

    private async Task<ChatMessageResponse> BookSameAsLastWeekdayAsync(Weekday? weekday, DateOnly date, CancellationToken ct)
    {
        var last = await GetLastBookingAsync(weekday, ct);
        if (last?.Desk is null)
        {
            return Reply(ChatIntentId.BOOK_SAME_AS_LAST_WEEKDAY, "I couldn't find a matching past booking.");
        }

        return new ChatMessageResponse
        {
            IntentId = ChatIntentId.BOOK_SAME_AS_LAST_WEEKDAY,
            Reply = $"Last time you sat at {SeatLabel(last.Desk)}. Book it again for {date:ddd d MMM}?",
            Candidates = [Suggestion(last.Desk, "Same seat as your last visit")],
            Action = BookAction(last.Desk.Id!, date, $"Book {SeatLabel(last.Desk)} on {date:ddd d MMM}"),
        };
    }

    private async Task<ChatMessageResponse> BookSpecificSeatAsync(ChatMessageParams? p, DateOnly date, CancellationToken ct)
    {
        if (p?.SeatNumber is not { } seatNumber)
        {
            return Reply(ChatIntentId.BOOK_SPECIFIC_SEAT, "Which seat number would you like to book?");
        }

        var lookup = await LookupDeskAsync(seatNumber, date, p.FallbackToAvailable, ct);
        if (lookup?.Desk is null)
        {
            return Reply(ChatIntentId.BOOK_SPECIFIC_SEAT, $"I couldn't find seat {seatNumber}.");
        }

        if (lookup.IsFree)
        {
            return new ChatMessageResponse
            {
                IntentId = ChatIntentId.BOOK_SPECIFIC_SEAT,
                Reply = $"Seat {seatNumber} is free on {date:ddd d MMM}. Book it?",
                Candidates = [Suggestion(lookup.Desk, "Requested seat, free")],
                Action = BookAction(lookup.Desk.Id!, date, $"Book seat {seatNumber} on {date:ddd d MMM}"),
            };
        }

        var fallbackDesk = lookup.Fallback?.Desk;
        var action = BookAction(lookup.Desk.Id!, date, $"Book seat {seatNumber} (or {SeatLabel(fallbackDesk)} if taken)");
        action.FallbackDeskId = fallbackDesk?.Id;
        action.FallbackToAvailable = p.FallbackToAvailable;

        var candidates = new List<DeskSuggestion> { Suggestion(lookup.Desk, "Requested seat (currently taken)") };
        if (fallbackDesk is not null)
        {
            candidates.Add(Suggestion(fallbackDesk, "Best available alternative"));
        }

        return new ChatMessageResponse
        {
            IntentId = ChatIntentId.BOOK_SPECIFIC_SEAT,
            Reply = fallbackDesk is null
                ? $"Seat {seatNumber} is taken on {date:ddd d MMM} and no alternative is free."
                : $"Seat {seatNumber} is taken on {date:ddd d MMM}. I can book {SeatLabel(fallbackDesk)} instead.",
            Candidates = candidates,
            Action = action,
        };
    }

    private async Task<ChatMessageResponse> CancelAndRebookAsync(ChatMessageParams? p, DateOnly date, CancellationToken ct)
    {
        var next = (await _reservations.GetUpcomingForUserAsync(_currentUser.RequireUserId(), ct)).FirstOrDefault();
        var cancelId = p?.ReservationId ?? next?.Id;

        var seatNumber = p?.SeatNumber;
        var lookup = seatNumber is null ? null : await LookupDeskAsync(seatNumber, date, p!.FallbackToAvailable, ct);
        var targetDesk = lookup?.IsFree == true ? lookup.Desk : lookup?.Fallback?.Desk ?? lookup?.Desk;

        if (cancelId is null || targetDesk is null)
        {
            return Reply(ChatIntentId.CANCEL_AND_REBOOK, "I need both a booking to cancel and a seat to book.");
        }

        return new ChatMessageResponse
        {
            IntentId = ChatIntentId.CANCEL_AND_REBOOK,
            Reply = $"Cancel your current booking and book {SeatLabel(targetDesk)} for {date:ddd d MMM}?",
            Candidates = [Suggestion(targetDesk, "New seat")],
            Action = new ChatAction
            {
                Type = ChatActionType.CANCEL_AND_BOOK,
                CancelReservationId = cancelId,
                DeskId = targetDesk.Id,
                FallbackDeskId = lookup?.Fallback?.Desk?.Id,
                FallbackToAvailable = p?.FallbackToAvailable ?? true,
                Date = date,
                WholeDay = true,
                Label = $"Cancel current booking and book {SeatLabel(targetDesk)}",
            },
        };
    }

    private async Task<ChatMessageResponse> ShowMyBookingsAsync(CancellationToken ct)
    {
        var reservations = await _reservationService.GetMyReservationsAsync(ct);
        return new ChatMessageResponse
        {
            IntentId = ChatIntentId.SHOW_MY_BOOKINGS,
            Reply = reservations.Count == 0
                ? "You have no upcoming reservations."
                : $"You have {reservations.Count} upcoming reservation(s).",
            Reservations = reservations.ToList(),
        };
    }

    private async Task<ChatMessageResponse> CancelNextBookingAsync(CancellationToken ct)
    {
        var next = (await _reservations.GetUpcomingForUserAsync(_currentUser.RequireUserId(), ct)).FirstOrDefault();
        if (next is null)
        {
            return Reply(ChatIntentId.CANCEL_NEXT_BOOKING, "You have no upcoming bookings to cancel.");
        }

        return new ChatMessageResponse
        {
            IntentId = ChatIntentId.CANCEL_NEXT_BOOKING,
            Reply = $"Cancel your next booking on {next.Date:ddd d MMM}?",
            Action = new ChatAction
            {
                Type = ChatActionType.CANCEL_RESERVATION,
                CancelReservationId = next.Id,
                ReservationId = next.Id,
                Label = $"Cancel booking on {next.Date:ddd d MMM}",
            },
        };
    }

    private async Task<ChatMessageResponse> FindFreeSeatsAsync(DateOnly date, CancellationToken ct)
    {
        var suggestions = await GetTeamDesksAsync(date, null, 5, ct);
        return new ChatMessageResponse
        {
            IntentId = ChatIntentId.FIND_FREE_SEAT_TODAY,
            Reply = suggestions.Count == 0
                ? $"I couldn't find free seats for {date:ddd d MMM}."
                : $"Here are {suggestions.Count} free seat(s) for {date:ddd d MMM}.",
            Candidates = suggestions.ToList(),
        };
    }

    // ---------------------------------------------------------------- Execute
    public async Task<ChatExecuteResponse> ExecuteAsync(ChatExecuteRequest request, CancellationToken ct = default)
    {
        var action = request.Action;
        return action.Type switch
        {
            ChatActionType.BOOK_DESK => await ExecuteBookAsync(action, ct),
            ChatActionType.CANCEL_RESERVATION => await ExecuteCancelAsync(action, ct),
            ChatActionType.CANCEL_AND_BOOK => await ExecuteCancelAndBookAsync(action, ct),
            _ => new ChatExecuteResponse { Success = false, Reply = "Nothing to do." },
        };
    }

    private async Task<ChatExecuteResponse> ExecuteBookAsync(ChatAction action, CancellationToken ct)
    {
        var reservation = await TryBookAsync(action.DeskId, action.FallbackToAvailable ? action.FallbackDeskId : null, action.Date, action.WholeDay, ct);
        return reservation is null
            ? new ChatExecuteResponse { Success = false, Reply = "That seat is no longer available." }
            : new ChatExecuteResponse { Success = true, Reply = "Done! Your seat is booked.", Reservation = reservation };
    }

    private async Task<ChatExecuteResponse> ExecuteCancelAsync(ChatAction action, CancellationToken ct)
    {
        var id = action.CancelReservationId ?? action.ReservationId;
        var cancelled = id is not null && await _reservationService.CancelAsync(id, ct);
        return new ChatExecuteResponse
        {
            Success = cancelled,
            Reply = cancelled ? "Your booking has been cancelled." : "I couldn't find that booking.",
        };
    }

    private async Task<ChatExecuteResponse> ExecuteCancelAndBookAsync(ChatAction action, CancellationToken ct)
    {
        var id = action.CancelReservationId ?? action.ReservationId;
        if (id is not null)
        {
            await _reservationService.CancelAsync(id, ct);
        }

        var reservation = await TryBookAsync(action.DeskId, action.FallbackToAvailable ? action.FallbackDeskId : null, action.Date, action.WholeDay, ct);
        return reservation is null
            ? new ChatExecuteResponse { Success = false, Reply = "Cancelled, but the new seat was no longer available." }
            : new ChatExecuteResponse { Success = true, Reply = "Done! Cancelled and rebooked.", Reservation = reservation };
    }

    /// <summary>Book <paramref name="deskId"/>, falling back to <paramref name="fallbackDeskId"/> on conflict.</summary>
    private async Task<DeskSchedule?> TryBookAsync(string? deskId, string? fallbackDeskId, DateOnly? date, bool wholeDay, CancellationToken ct)
    {
        foreach (var candidate in new[] { deskId, fallbackDeskId })
        {
            if (candidate is null)
            {
                continue;
            }

            try
            {
                return await _reservationService.BookAsync(
                    new ReservationInput { DeskId = candidate, Date = date, WholeDay = wholeDay }, ct);
            }
            catch (BookingConflictException)
            {
                // Try the next candidate.
            }
        }

        return null;
    }

    // ---------------------------------------------------------------- Supporting queries
    public async Task<FavoriteDesk?> GetFavoriteDeskAsync(int sinceDays, CancellationToken ct = default)
    {
        var userId = _currentUser.RequireUserId();
        if (await _reservations.GetFavoriteDeskAsync(userId, sinceDays, ct) is { } fav)
        {
            return new FavoriteDesk
            {
                Desk = fav.Desk.ToDto(),
                BookingCount = fav.BookingCount,
                LastBookedDate = ToDateOnly(fav.LastBookedDate),
                Source = FavoriteDeskSource.HISTORY,
            };
        }

        // Fall back to the saved preference when there's no booking history.
        var preference = await _preferences.GetByUserAsync(userId, ct);
        if (preference?.FavoriteDeskId is { } favoriteDeskId &&
            await _desks.GetByIdAsync(favoriteDeskId, ct) is { } desk)
        {
            return new FavoriteDesk
            {
                Desk = desk.ToDto(),
                BookingCount = 0,
                Source = FavoriteDeskSource.PREFERENCE,
            };
        }

        return null;
    }

    public async Task<DeskScheduleWithDesk?> GetLastBookingAsync(Weekday? weekday, CancellationToken ct = default)
    {
        var last = await _reservations.GetLastBookingAsync(_currentUser.RequireUserId(), weekday, ct);
        return last?.ToWithDeskDto();
    }

    /// <remarks>
    /// The schema has no explicit "team" model, so this returns free desks on the user's
    /// preferred floor for the date as a proximity proxy. Extend once team membership exists.
    /// </remarks>
    public async Task<IReadOnlyList<DeskSuggestion>> GetTeamDesksAsync(DateOnly date, string? colleagueId, int limit, CancellationToken ct = default)
    {
        var preference = await _preferences.GetByUserAsync(_currentUser.RequireUserId(), ct);
        if (preference?.PreferredFloorId is not { } floorId)
        {
            return [];
        }

        var availability = await _reservations.GetFloorAvailabilityAsync(floorId, date, ct);
        return availability
            .Where(a => a.IsFree)
            .Take(limit)
            .Select(a => Suggestion(a.Desk.ToDto(), "Free on your preferred floor"))
            .ToList();
    }

    public async Task<DeskLookupResult?> LookupDeskAsync(string seatNumber, DateOnly date, bool fallbackToAvailable, CancellationToken ct = default)
    {
        if (await _desks.GetByPublicDeskIdAsync(seatNumber, ct) is not { } desk)
        {
            return null;
        }

        var isFree = !await _reservations.HasConflictAsync(desk.Id, date, ct);
        var result = new DeskLookupResult { Desk = desk.ToDto(), IsFree = isFree };

        if (!isFree && fallbackToAvailable)
        {
            var availability = await _reservations.GetFloorAvailabilityAsync(desk.FloorId, date, ct);
            if (availability.FirstOrDefault(a => a.IsFree && a.Desk.Id != desk.Id) is { } free)
            {
                result.Fallback = Suggestion(free.Desk.ToDto(), "Best available seat");
            }
        }

        return result;
    }

    public async Task<IReadOnlyList<DeskScheduleWithDesk>> GetBookingHistoryAsync(int sinceDays, CancellationToken ct = default)
    {
        var history = await _reservations.GetHistoryForUserAsync(_currentUser.RequireUserId(), sinceDays, ct);
        return history.Select(h => h.ToWithDeskDto()).ToList();
    }

    // ---------------------------------------------------------------- Helpers
    private static ChatIntent Intent(ChatIntentId id, string text, bool needsParams)
        => new() { IntentId = id, Text = text, NeedsParams = needsParams };

    private static ChatMessageResponse Reply(ChatIntentId id, string reply)
        => new() { IntentId = id, Reply = reply };

    private static DeskSuggestion Suggestion(Desk desk, string reason)
        => new() { Desk = desk, Score = 1, IsFree = true, Reason = reason };

    private static ChatAction BookAction(string deskId, DateOnly date, string label)
        => new() { Type = ChatActionType.BOOK_DESK, DeskId = deskId, Date = date, WholeDay = true, Label = label };

    private static string SeatLabel(Desk? desk)
        => desk?.Name ?? desk?.PublicDeskId ?? "the seat";

    private static DateOnly Today() => DateOnly.FromDateTime(DateTime.UtcNow);

    private static DateOnly? ToDateOnly(DateTimeOffset? value)
        => value is { } v ? DateOnly.FromDateTime(v.UtcDateTime) : null;
}
