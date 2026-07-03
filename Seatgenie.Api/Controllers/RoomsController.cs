using Microsoft.AspNetCore.Mvc;
using Seatgenie.Api.Models;
using Seatgenie.Api.Services;

namespace Seatgenie.Api.Controllers;

/// <summary>Meeting rooms and office rooms on a floor.</summary>
[Tags("Rooms")]
public class RoomsController : ApiControllerBase
{
    private readonly IRoomService _rooms;

    public RoomsController(IRoomService rooms) => _rooms = rooms;

    /// <summary>List meeting rooms on floor.</summary>
    [HttpGet("/floors/{id}/meeting-rooms")]
    [ProducesResponseType(typeof(IEnumerable<Room>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListMeetingRooms(string id, CancellationToken ct)
        => Ok(await _rooms.ListMeetingRoomsAsync(id, ct));

    /// <summary>Create meeting room.</summary>
    [HttpPost("/floors/{id}/meeting-rooms")]
    [ProducesResponseType(typeof(Room), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateMeetingRoom(string id, [FromBody] RoomInput input, CancellationToken ct)
        => StatusCode(StatusCodes.Status201Created, await _rooms.CreateMeetingRoomAsync(id, input, ct));

    /// <summary>Update meeting room.</summary>
    [HttpPatch("/meeting-rooms/{id}")]
    [ProducesResponseType(typeof(Room), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateMeetingRoom(string id, [FromBody] RoomInput input, CancellationToken ct)
        => OkOrNotFound(await _rooms.UpdateMeetingRoomAsync(id, input, ct));

    /// <summary>Delete meeting room.</summary>
    [HttpDelete("/meeting-rooms/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteMeetingRoom(string id, CancellationToken ct)
        => await _rooms.DeleteMeetingRoomAsync(id, ct) ? NoContent() : NotFoundError();

    /// <summary>List office rooms on floor.</summary>
    [HttpGet("/floors/{id}/office-rooms")]
    [ProducesResponseType(typeof(IEnumerable<Room>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListOfficeRooms(string id, CancellationToken ct)
        => Ok(await _rooms.ListOfficeRoomsAsync(id, ct));

    /// <summary>Create office room.</summary>
    [HttpPost("/floors/{id}/office-rooms")]
    [ProducesResponseType(typeof(Room), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateOfficeRoom(string id, [FromBody] RoomInput input, CancellationToken ct)
        => StatusCode(StatusCodes.Status201Created, await _rooms.CreateOfficeRoomAsync(id, input, ct));

    /// <summary>Update office room.</summary>
    [HttpPatch("/office-rooms/{id}")]
    [ProducesResponseType(typeof(Room), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateOfficeRoom(string id, [FromBody] RoomInput input, CancellationToken ct)
        => OkOrNotFound(await _rooms.UpdateOfficeRoomAsync(id, input, ct));

    /// <summary>Delete office room.</summary>
    [HttpDelete("/office-rooms/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteOfficeRoom(string id, CancellationToken ct)
        => await _rooms.DeleteOfficeRoomAsync(id, ct) ? NoContent() : NotFoundError();
}
