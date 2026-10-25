namespace HotelBooking.Domain;

/// <summary>
/// A booking of one room for the nights in [CheckInDate, CheckOutDate).
/// The check-out day itself is free, so another guest can arrive that day.
/// Lifecycle: Confirmed → CheckedIn → CheckedOut, or Confirmed → Cancelled.
/// </summary>
public class Reservation
{
    public int Id { get; private set; }
    public int RoomId { get; private set; }
    public Room Room { get; private set; } = null!;
    public int GuestId { get; private set; }
    public User Guest { get; private set; } = null!;
    public DateOnly CheckInDate { get; private set; }
    public DateOnly CheckOutDate { get; private set; }
    public int NumberOfGuests { get; private set; }
    public decimal TotalPrice { get; private set; }
    public ReservationStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? CheckedInAtUtc { get; private set; }
    public DateTime? CheckedOutAtUtc { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }

    public int Nights => CheckOutDate.DayNumber - CheckInDate.DayNumber;

    // Required by EF Core.
    private Reservation() { }

    public static Reservation Create(
        Room room, int guestId, DateOnly checkIn, DateOnly checkOut, int numberOfGuests, DateTime utcNow)
    {
        var today = DateOnly.FromDateTime(utcNow);

        if (checkIn < today)
            throw new DomainException("Check-in date cannot be in the past.");
        if (checkOut <= checkIn)
            throw new DomainException("Check-out date must be after the check-in date.");
        if (numberOfGuests < 1)
            throw new DomainException("At least one guest is required.");
        if (numberOfGuests > room.Capacity)
            throw new DomainException($"Room {room.Number} sleeps at most {room.Capacity}.");

        var reservation = new Reservation
        {
            RoomId = room.Id,
            Room = room,
            GuestId = guestId,
            CheckInDate = checkIn,
            CheckOutDate = checkOut,
            NumberOfGuests = numberOfGuests,
            Status = ReservationStatus.Confirmed,
            CreatedAtUtc = utcNow
        };
        // Price is locked in at booking time, so later room price changes don't affect it.
        reservation.TotalPrice = reservation.Nights * room.PricePerNight;
        return reservation;
    }

    public void CheckIn(DateTime utcNow)
    {
        var today = DateOnly.FromDateTime(utcNow);

        if (Status != ReservationStatus.Confirmed)
            throw new DomainException($"Cannot check in a reservation that is {Status}.");
        if (today < CheckInDate)
            throw new DomainException("Guest cannot check in before the check-in date.");
        if (today >= CheckOutDate)
            throw new DomainException("The stay has already ended.");

        Status = ReservationStatus.CheckedIn;
        CheckedInAtUtc = utcNow;
    }

    public void CheckOut(DateTime utcNow)
    {
        if (Status != ReservationStatus.CheckedIn)
            throw new DomainException($"Cannot check out a reservation that is {Status}.");

        Status = ReservationStatus.CheckedOut;
        CheckedOutAtUtc = utcNow;
    }

    public void Cancel(DateTime utcNow)
    {
        if (Status != ReservationStatus.Confirmed)
            throw new DomainException($"Cannot cancel a reservation that is {Status}.");

        Status = ReservationStatus.Cancelled;
        CancelledAtUtc = utcNow;
    }
}
