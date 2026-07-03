namespace Seatgenie.Api.Models;

/// <summary>A predefined quick-reply the UI renders as a button/chip.</summary>
public class ChatIntent
{
    public ChatIntentId IntentId { get; set; }

    /// <summary>Human-facing label shown on the chip.</summary>
    /// <example>Book my favourite seat</example>
    public string? Text { get; set; }

    /// <summary>Whether the intent requires extra params (date, weekday, colleagueId).</summary>
    public bool NeedsParams { get; set; }
}

/// <summary>Slots an intent may need to resolve into a concrete booking.</summary>
public class ChatMessageParams
{
    public DateOnly? Date { get; set; }
    public Weekday? Weekday { get; set; }
    public string? ColleagueId { get; set; }

    /// <summary>Human seat label / public desk id (e.g. "112").</summary>
    /// <example>112</example>
    public string? SeatNumber { get; set; }

    /// <summary>If the requested seat is taken, book the best available seat instead of failing.</summary>
    public bool FallbackToAvailable { get; set; } = true;

    /// <summary>Reservation to cancel for CANCEL_AND_REBOOK / SWAP_SEAT (defaults to last/next booking).</summary>
    public string? ReservationId { get; set; }
}

/// <summary>Send either a predefined intentId or free text.</summary>
public class ChatMessageRequest
{
    public ChatIntentId? IntentId { get; set; }

    /// <summary>Free-text message (used when no intentId is supplied).</summary>
    /// <example>Book the same seat I booked last Monday</example>
    public string? Text { get; set; }

    public ChatMessageParams? Params { get; set; }

    /// <summary>Optional client conversation id for multi-turn context.</summary>
    public string? SessionId { get; set; }
}

/// <summary>An actionable proposal the user can confirm via /chatbot/execute.</summary>
public class ChatAction
{
    public ChatActionType Type { get; set; }

    /// <summary>Target desk to book (preferred seat, e.g. seat "112").</summary>
    public string? DeskId { get; set; }

    /// <summary>Reservation to cancel (for CANCEL_RESERVATION / CANCEL_AND_BOOK).</summary>
    public string? CancelReservationId { get; set; }

    /// <summary>Deprecated alias of cancelReservationId; kept for single CANCEL_RESERVATION.</summary>
    public string? ReservationId { get; set; }

    /// <summary>If the preferred deskId is unavailable, book fallbackDeskId instead.</summary>
    public bool FallbackToAvailable { get; set; } = true;

    /// <summary>Pre-resolved best available seat used when the preferred desk is taken.</summary>
    public string? FallbackDeskId { get; set; }

    public DateOnly? Date { get; set; }
    public bool WholeDay { get; set; } = true;

    /// <example>Cancel Mon booking on Desk A-3 and book seat 112 (or Desk B-7 if taken)</example>
    public string? Label { get; set; }
}

/// <summary>Assistant reply with resolved candidates / action.</summary>
public class ChatMessageResponse
{
    public ChatIntentId? IntentId { get; set; }

    /// <summary>Natural-language assistant reply to render in the chat.</summary>
    /// <example>Your favourite seat is Desk A-12 (booked 23 times). It's free on Mon 6 Jul — book it?</example>
    public string? Reply { get; set; }

    /// <summary>Resolved desk options (ranked); empty for non-booking intents.</summary>
    public List<DeskSuggestion> Candidates { get; set; } = new();

    public ChatAction? Action { get; set; }

    /// <summary>Populated for SHOW_MY_BOOKINGS.</summary>
    public List<DeskSchedule> Reservations { get; set; } = new();
}

/// <summary>Echo back the ChatAction returned by /chatbot/message to run it.</summary>
public class ChatExecuteRequest
{
    public required ChatAction Action { get; set; }
    public string? SessionId { get; set; }
}

/// <summary>Result of executing a chatbot action.</summary>
public class ChatExecuteResponse
{
    public bool Success { get; set; }

    /// <example>Done! Desk A-12 is booked for Monday 6 Jul.</example>
    public string? Reply { get; set; }

    public DeskSchedule? Reservation { get; set; }
}

/// <summary>Resolved desk + availability (+ fallback when taken) for a seat lookup.</summary>
public class DeskLookupResult
{
    public Desk? Desk { get; set; }
    public bool IsFree { get; set; }
    public DeskSuggestion? Fallback { get; set; }
}
