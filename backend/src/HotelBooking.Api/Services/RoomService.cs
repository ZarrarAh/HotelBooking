using HotelBooking.Api.Contracts;
using HotelBooking.Api.Errors;
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

    public async Task<RoomResponse> CreateAsync(RoomRequest request)
    {
        await EnsureNumberIsFreeAsync(request.Number);
        var room = new Room(request.Number, request.Type, request.Capacity, request.PricePerNight, request.Description);
        db.Rooms.Add(room);
        await db.SaveChangesAsync();
        return RoomResponse.From(room);
    }

    public async Task<RoomResponse> UpdateAsync(int id, RoomRequest request)
    {
        var room = await db.Rooms.FindAsync(id) ?? throw new NotFoundException($"Room {id} not found.");
        await EnsureNumberIsFreeAsync(request.Number, exceptRoomId: id);
        // Existing reservations keep the price they were booked at.
        room.Update(request.Number, request.Type, request.Capacity, request.PricePerNight, request.Description);
        await db.SaveChangesAsync();
        return RoomResponse.From(room);
    }

    public async Task DeleteAsync(int id)
    {
        var room = await db.Rooms.FindAsync(id) ?? throw new NotFoundException($"Room {id} not found.");
        if (await db.Reservations.AnyAsync(r => r.RoomId == id))
            throw new ConflictException("Rooms with reservations cannot be deleted.");
        db.Rooms.Remove(room);
        await db.SaveChangesAsync();
    }

    private async Task EnsureNumberIsFreeAsync(string number, int? exceptRoomId = null)
    {
        var trimmed = number.Trim();
        if (await db.Rooms.AnyAsync(r => r.Number == trimmed && r.Id != exceptRoomId))
            throw new ConflictException($"Room number {trimmed} is already in use.");
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
