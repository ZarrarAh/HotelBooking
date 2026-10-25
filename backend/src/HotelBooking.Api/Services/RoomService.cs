using HotelBooking.Api.Contracts;
using HotelBooking.Domain;
using HotelBooking.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Api.Services;

public class RoomService(AppDbContext db, TimeProvider time)
{
    public async Task<List<RoomResponse>> GetAllAsync() =>
        await db.Rooms
            .AsNoTracking()
            .OrderBy(r => r.Number)
            .Select(r => RoomResponse.From(r))
            .ToListAsync();

    public async Task<RoomResponse?> GetByIdAsync(int id)
    {
        var room = await db.Rooms.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
        return room is null ? null : RoomResponse.From(room);
    }

    /// <summary>Rooms big enough for the party that have no active reservation overlapping the dates.</summary>
    public async Task<List<AvailableRoomResponse>> SearchAvailableAsync(DateOnly checkIn, DateOnly checkOut, int guests)
    {
        var today = DateOnly.FromDateTime(time.GetUtcNow().UtcDateTime);
        if (checkIn < today)
            throw new DomainException("Check-in date cannot be in the past.");

        var nights = checkOut.DayNumber - checkIn.DayNumber;
        var bookedRoomIds = db.Reservations.Overlapping(checkIn, checkOut).Select(r => r.RoomId);

        // Translates to a single SQL query with a NOT IN (subquery).
        return await db.Rooms
            .AsNoTracking()
            .Where(room => room.Capacity >= guests && !bookedRoomIds.Contains(room.Id))
            .OrderBy(room => room.PricePerNight).ThenBy(room => room.Number)
            .Select(room => new AvailableRoomResponse(
                room.Id, room.Number, room.Type, room.Capacity, room.PricePerNight, room.Description,
                nights, room.PricePerNight * nights))
            .ToListAsync();
    }
}
