using HotelBooking.Api.Contracts;
using HotelBooking.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Api.Services;

public class RoomService(AppDbContext db)
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
}
