using Microsoft.AspNetCore.Mvc;
using Seatgenie.Api.Models;
using Seatgenie.Api.Services;

namespace Seatgenie.Api.Controllers;

/// <summary>Desk scheduling / booking engine.</summary>
[Tags("Reservations")]
public class ReservationsController : ApiControllerBase
{
    private readonly IReservationService _reservations;

    public ReservationsController(IReservationService reservations) => _reservations = reservations;

    /// <summary>Get a desk's schedule for a date.</summary>
    [HttpGet("/desks/{id}/schedule")]
    [ProducesResponseType(typeof(IEnumerable<DeskSchedule>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeskSchedule(string id, [FromQuery] DateOnly? date, CancellationToken ct)
        => Ok(await _reservations.GetDeskScheduleAsync(id, date, ct));

    /// <summary>Free vs. booked desks for a day.</summary>
    [HttpGet("/floors/{id}/availability")]
    [ProducesResponseType(typeof(IEnumerable<DeskAvailability>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFloorAvailability(string id, [FromQuery] DateOnly date, CancellationToken ct)
        => Ok(await _reservations.GetFloorAvailabilityAsync(id, date, ct));

    /// <summary>My upcoming reservations.</summary>
    [HttpGet("/me/reservations")]
    [ProducesResponseType(typeof(IEnumerable<DeskSchedule>), StatusCodes.Status200OK)]
    public async Task<IActionResult> MyReservations(CancellationToken ct)
        => Ok(await _reservations.GetMyReservationsAsync(ct));

    /// <summary>Book a desk.</summary>
    [HttpPost("/reservations")]
    [ProducesResponseType(typeof(DeskSchedule), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> BookDesk([FromBody] ReservationInput input, CancellationToken ct)
        => StatusCode(StatusCodes.Status201Created, await _reservations.BookAsync(input, ct));

    /// <summary>Update a reservation.</summary>
    [HttpPatch("/reservations/{id}")]
    [ProducesResponseType(typeof(DeskSchedule), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateReservation(string id, [FromBody] ReservationInput input, CancellationToken ct)
        => OkOrNotFound(await _reservations.UpdateAsync(id, input, ct));

    /// <summary>Cancel a reservation.</summary>
    [HttpDelete("/reservations/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> CancelReservation(string id, CancellationToken ct)
        => await _reservations.CancelAsync(id, ct) ? NoContent() : NotFoundError();

    /// <summary>Pre-flight conflict check.</summary>
    [HttpGet("/reservations/conflicts")]
    [ProducesResponseType(typeof(Models.ConflictResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckConflicts([FromQuery] string deskId, [FromQuery] DateOnly date, CancellationToken ct)
        => Ok(await _reservations.CheckConflictAsync(deskId, date, ct));
}
