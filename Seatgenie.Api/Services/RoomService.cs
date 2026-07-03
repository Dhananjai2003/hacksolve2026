using Seatgenie.Api.Models;
using Seatgenie.Api.Repositories;
using Ent = Seatgenie.Api.Entities;

namespace Seatgenie.Api.Services;

public interface IRoomService
{
    Task<IReadOnlyList<Room>> ListMeetingRoomsAsync(string floorId, CancellationToken ct = default);
    Task<Room> CreateMeetingRoomAsync(string floorId, RoomInput input, CancellationToken ct = default);
    Task<Room?> UpdateMeetingRoomAsync(string id, RoomInput input, CancellationToken ct = default);
    Task<bool> DeleteMeetingRoomAsync(string id, CancellationToken ct = default);

    Task<IReadOnlyList<Room>> ListOfficeRoomsAsync(string floorId, CancellationToken ct = default);
    Task<Room> CreateOfficeRoomAsync(string floorId, RoomInput input, CancellationToken ct = default);
    Task<Room?> UpdateOfficeRoomAsync(string id, RoomInput input, CancellationToken ct = default);
    Task<bool> DeleteOfficeRoomAsync(string id, CancellationToken ct = default);
}

public class RoomService : IRoomService
{
    private readonly IMeetingRoomRepository _meetingRooms;
    private readonly IOfficeRoomRepository _officeRooms;

    public RoomService(IMeetingRoomRepository meetingRooms, IOfficeRoomRepository officeRooms)
    {
        _meetingRooms = meetingRooms;
        _officeRooms = officeRooms;
    }

    // ---------------------------------------------------------------- Meeting rooms
    public async Task<IReadOnlyList<Room>> ListMeetingRoomsAsync(string floorId, CancellationToken ct = default)
    {
        var rooms = await _meetingRooms.ListByFloorAsync(floorId, ct);
        return rooms.Select(r => r.ToDto()).ToList();
    }

    public async Task<Room> CreateMeetingRoomAsync(string floorId, RoomInput input, CancellationToken ct = default)
    {
        var entity = new Ent.MeetingRoom { Name = input.Name, Description = input.Description, FloorId = floorId };
        var created = await _meetingRooms.AddAsync(entity, ct);
        return created.ToDto();
    }

    public async Task<Room?> UpdateMeetingRoomAsync(string id, RoomInput input, CancellationToken ct = default)
    {
        if (await _meetingRooms.GetByIdAsync(id, ct) is not { } room)
        {
            return null;
        }

        room.Name = input.Name;
        room.Description = input.Description;
        await _meetingRooms.UpdateAsync(room, ct);
        return room.ToDto();
    }

    public Task<bool> DeleteMeetingRoomAsync(string id, CancellationToken ct = default)
        => _meetingRooms.DeleteAsync(id, ct);

    // ---------------------------------------------------------------- Office rooms
    public async Task<IReadOnlyList<Room>> ListOfficeRoomsAsync(string floorId, CancellationToken ct = default)
    {
        var rooms = await _officeRooms.ListByFloorAsync(floorId, ct);
        return rooms.Select(r => r.ToDto()).ToList();
    }

    public async Task<Room> CreateOfficeRoomAsync(string floorId, RoomInput input, CancellationToken ct = default)
    {
        var entity = new Ent.OfficeRoom { Name = input.Name, Description = input.Description, FloorId = floorId };
        var created = await _officeRooms.AddAsync(entity, ct);
        return created.ToDto();
    }

    public async Task<Room?> UpdateOfficeRoomAsync(string id, RoomInput input, CancellationToken ct = default)
    {
        if (await _officeRooms.GetByIdAsync(id, ct) is not { } room)
        {
            return null;
        }

        room.Name = input.Name;
        room.Description = input.Description;
        await _officeRooms.UpdateAsync(room, ct);
        return room.ToDto();
    }

    public Task<bool> DeleteOfficeRoomAsync(string id, CancellationToken ct = default)
        => _officeRooms.DeleteAsync(id, ct);
}
