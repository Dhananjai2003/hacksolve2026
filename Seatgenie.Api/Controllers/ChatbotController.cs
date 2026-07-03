using Microsoft.AspNetCore.Mvc;
using Seatgenie.Api.Models;
using Seatgenie.Api.Services;

namespace Seatgenie.Api.Controllers;

/// <summary>
/// Conversational booking assistant plus the supporting queries the intents
/// resolve against.
/// </summary>
[Tags("Chatbot")]
public class ChatbotController : ApiControllerBase
{
    private readonly IChatbotService _chatbot;

    public ChatbotController(IChatbotService chatbot) => _chatbot = chatbot;

    /// <summary>List predefined chatbot quick-reply texts.</summary>
    [HttpGet("/chatbot/intents")]
    [ProducesResponseType(typeof(IEnumerable<ChatIntent>), StatusCodes.Status200OK)]
    public IActionResult ListIntents()
        => Ok(_chatbot.ListIntents());

    /// <summary>Send a chat message / trigger an intent.</summary>
    [HttpPost("/chatbot/message")]
    [ProducesResponseType(typeof(ChatMessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SendMessage([FromBody] ChatMessageRequest request, CancellationToken ct)
        => Ok(await _chatbot.SendMessageAsync(request, ct));

    /// <summary>Confirm and execute a chatbot booking action.</summary>
    [HttpPost("/chatbot/execute")]
    [ProducesResponseType(typeof(ChatExecuteResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Execute([FromBody] ChatExecuteRequest request, CancellationToken ct)
        => StatusCode(StatusCodes.Status201Created, await _chatbot.ExecuteAsync(request, ct));

    /// <summary>My favourite desk (most-booked in history).</summary>
    [HttpGet("/me/favorite-desk")]
    [ProducesResponseType(typeof(FavoriteDesk), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FavoriteDesk([FromQuery] int sinceDays = 180, CancellationToken ct = default)
        => OkOrNotFound(await _chatbot.GetFavoriteDeskAsync(sinceDays, ct), "No favourite desk found.");

    /// <summary>My most recent booking (optionally by weekday).</summary>
    [HttpGet("/me/last-booking")]
    [ProducesResponseType(typeof(DeskScheduleWithDesk), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LastBooking([FromQuery] Weekday? weekday, CancellationToken ct)
        => OkOrNotFound(await _chatbot.GetLastBookingAsync(weekday, ct), "No matching past booking.");

    /// <summary>Seats near my team for a date.</summary>
    [HttpGet("/me/team/desks")]
    [ProducesResponseType(typeof(IEnumerable<DeskSuggestion>), StatusCodes.Status200OK)]
    public async Task<IActionResult> TeamDesks([FromQuery] DateOnly date, [FromQuery] string? colleagueId, [FromQuery] int limit = 5, CancellationToken ct = default)
        => Ok(await _chatbot.GetTeamDesksAsync(date, colleagueId, limit, ct));

    /// <summary>Resolve a seat number and check availability.</summary>
    [HttpGet("/desks/lookup")]
    [ProducesResponseType(typeof(DeskLookupResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LookupDesk([FromQuery] string seatNumber, [FromQuery] DateOnly date, [FromQuery] bool fallbackToAvailable = true, CancellationToken ct = default)
        => OkOrNotFound(await _chatbot.LookupDeskAsync(seatNumber, date, fallbackToAvailable, ct), $"Seat {seatNumber} not found.");

    /// <summary>My booking history.</summary>
    [HttpGet("/me/booking-history")]
    [ProducesResponseType(typeof(IEnumerable<DeskScheduleWithDesk>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BookingHistory([FromQuery] int sinceDays = 90, CancellationToken ct = default)
        => Ok(await _chatbot.GetBookingHistoryAsync(sinceDays, ct));
}
