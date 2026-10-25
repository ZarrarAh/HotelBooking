using HotelBooking.Domain;

namespace HotelBooking.Api.Services;

public static class ReservationQueries
{
    /// <summary>
    /// Reservations that occupy the room for at least one night of [checkIn, checkOut).
    /// Two half-open ranges overlap when each one starts before the other ends.
    /// Cancelled reservations don't block anything.
    /// </summary>
    public static IQueryable<Reservation> Overlapping(
        this IQueryable<Reservation> reservations, DateOnly checkIn, DateOnly checkOut) =>
        reservations.Where(r =>
            r.Status != ReservationStatus.Cancelled
            && r.CheckInDate < checkOut
            && checkIn < r.CheckOutDate);
}
