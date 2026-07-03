namespace Seatgenie.Api.Models;

/// <summary>Role of a user within an organization.</summary>
public enum UserRole
{
    ADMIN,
    MEMBER,
}

/// <summary>Feature preferences selected during onboarding.</summary>
public enum WorkplacifyPreference
{
    DESK_BOOKING,
    WORKPLACE_ANALYTICS,
    JOIN_ORGANIZATION,
}

/// <summary>Days of the week.</summary>
public enum Weekday
{
    MONDAY,
    TUESDAY,
    WEDNESDAY,
    THURSDAY,
    FRIDAY,
    SATURDAY,
    SUNDAY,
}

/// <summary>The fixed set of predefined chatbot prompts.</summary>
public enum ChatIntentId
{
    /// <summary>Book my favourite seat (most-booked in history).</summary>
    BOOK_FAVORITE_SEAT,

    /// <summary>Find a seat near my team.</summary>
    FIND_SEAT_NEAR_TEAM,

    /// <summary>Book the same seat I booked last Monday.</summary>
    BOOK_SAME_AS_LAST_WEEKDAY,

    /// <summary>Book the best recommended seat for me.</summary>
    BOOK_RECOMMENDED_SEAT,

    /// <summary>Book my preferred desk (from preferences).</summary>
    BOOK_PREFERRED_DESK,

    /// <summary>What desks are free today?</summary>
    FIND_FREE_SEAT_TODAY,

    /// <summary>Find any free seat on my preferred floor.</summary>
    FIND_SEAT_ON_PREFERRED_FLOOR,

    /// <summary>Find a seat near a specific colleague.</summary>
    FIND_SEAT_NEAR_COLLEAGUE,

    /// <summary>Book my usual seat for my in-office days this week.</summary>
    BOOK_FOR_MY_OFFICE_DAYS,

    /// <summary>Show my upcoming reservations.</summary>
    SHOW_MY_BOOKINGS,

    /// <summary>Cancel my next booking.</summary>
    CANCEL_NEXT_BOOKING,

    /// <summary>Book the same seat as my last visit.</summary>
    REBOOK_LAST_SEAT,

    /// <summary>Book seat 112 (by seat number), fallback to any free seat.</summary>
    BOOK_SPECIFIC_SEAT,

    /// <summary>Cancel my last booking and book seat 112 or a new available seat.</summary>
    CANCEL_AND_REBOOK,

    /// <summary>Cancel my current booking and move me to a better/available seat.</summary>
    SWAP_SEAT,
}

/// <summary>The kind of action a chatbot proposal represents.</summary>
public enum ChatActionType
{
    BOOK_DESK,
    CANCEL_RESERVATION,
    CANCEL_AND_BOOK,
    NONE,
}

/// <summary>Source of a resolved favourite desk.</summary>
public enum FavoriteDeskSource
{
    HISTORY,
    PREFERENCE,
}
