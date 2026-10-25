using System.Linq.Expressions;
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
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (DbErrors.IsReservationOverlap(ex))
        {
            // Another request booked the same room between our check and our insert.
            throw new ConflictException($"Room {room.Number} was just booked by someone else for those nights.");
        }

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

    public DateOnly Today => DateOnly.FromDateTime(UtcNow);

    /// <summary>Guests due to arrive on the date, including those already checked in.</summary>
    public Task<List<ReservationResponse>> GetArrivalsAsync(DateOnly date) =>
        ListAsync(r => r.CheckInDate == date
            && (r.Status == ReservationStatus.Confirmed || r.Status == ReservationStatus.CheckedIn));

    /// <summary>Guests due to leave on the date, including those already checked out.</summary>
    public Task<List<ReservationResponse>> GetDeparturesAsync(DateOnly date) =>
        ListAsync(r => r.CheckOutDate == date
            && (r.Status == ReservationStatus.CheckedIn || r.Status == ReservationStatus.CheckedOut));

    public async Task<ReservationResponse> CheckInAsync(int id)
    {
        var reservation = await WithDetails.FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new NotFoundException($"Reservation {id} not found.");
        reservation.CheckIn(UtcNow);
        await db.SaveChangesAsync();
        return ReservationResponse.From(reservation);
    }

    public async Task<ReservationResponse> CheckOutAsync(int id)
    {
        var reservation = await WithDetails.FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new NotFoundException($"Reservation {id} not found.");
        reservation.CheckOut(UtcNow);
        await db.SaveChangesAsync();
        return ReservationResponse.From(reservation);
    }

    private async Task<List<ReservationResponse>> ListAsync(Expression<Func<Reservation, bool>> filter)
    {
        var reservations = await WithDetails
            .AsNoTracking()
            .Where(filter)
            .OrderBy(r => r.Room.Number)
            .ToListAsync();
        return reservations.Select(ReservationResponse.From).ToList();
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
