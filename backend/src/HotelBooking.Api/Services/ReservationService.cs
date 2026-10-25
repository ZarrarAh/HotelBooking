using HotelBooking.Api.Contracts;
using HotelBooking.Api.Errors;
using HotelBooking.Domain;
using HotelBooking.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Api.Services;

public class ReservationService(AppDbContext db, TimeProvider time)
{
    private DateTime UtcNow => time.GetUtcNow().UtcDateTime;

    private IQueryable<Reservation> WithDetails =>
        db.Reservations.Include(r => r.Room).Include(r => r.Guest);

    public async Task<ReservationResponse> CreateAsync(int guestId, CreateReservationRequest request)
    {
        var room = await db.Rooms.FindAsync(request.RoomId)
            ?? throw new NotFoundException($"Room {request.RoomId} not found.");

        // Friendly early check. The database constraint is the real guarantee under concurrency.
        var isTaken = await db.Reservations
            .Overlapping(request.CheckIn, request.CheckOut)
            .AnyAsync(r => r.RoomId == room.Id);
        if (isTaken)
            throw new ConflictException($"Room {room.Number} is already booked for some of those nights.");

        var reservation = Reservation.Create(
            room, guestId, request.CheckIn, request.CheckOut, request.NumberOfGuests, UtcNow);
        db.Reservations.Add(reservation);
        await db.SaveChangesAsync();

        return await GetResponseAsync(reservation.Id);
    }

    public async Task<List<ReservationResponse>> GetForGuestAsync(int guestId)
    {
        var reservations = await WithDetails
            .AsNoTracking()
            .Where(r => r.GuestId == guestId)
            .OrderByDescending(r => r.CheckInDate)
            .ToListAsync();
        return reservations.Select(ReservationResponse.From).ToList();
    }

    /// <summary>Guests can only see their own reservations; staff can see all.</summary>
    public async Task<ReservationResponse> GetAsync(int id, int userId, bool isStaff)
    {
        var reservation = await FindVisibleAsync(id, userId, isStaff);
        return ReservationResponse.From(reservation);
    }

    public async Task<ReservationResponse> CancelAsync(int id, int userId, bool isStaff)
    {
        var reservation = await FindVisibleAsync(id, userId, isStaff);
        reservation.Cancel(UtcNow);
        await db.SaveChangesAsync();
        return ReservationResponse.From(reservation);
    }

    private async Task<Reservation> FindVisibleAsync(int id, int userId, bool isStaff)
    {
        var reservation = await WithDetails.FirstOrDefaultAsync(r => r.Id == id);
        // Another guest's booking is reported as "not found" so ids can't be probed.
        if (reservation is null || (!isStaff && reservation.GuestId != userId))
            throw new NotFoundException($"Reservation {id} not found.");
        return reservation;
    }

    private async Task<ReservationResponse> GetResponseAsync(int id) =>
        ReservationResponse.From(await WithDetails.AsNoTracking().FirstAsync(r => r.Id == id));
}
