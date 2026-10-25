using System.ComponentModel.DataAnnotations;
using HotelBooking.Domain;

namespace HotelBooking.Api.Contracts;

public record CreateReservationRequest(
    int RoomId,
    DateOnly CheckIn,
    DateOnly CheckOut,
    [Range(1, 10)] int NumberOfGuests) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CheckOut <= CheckIn)
            yield return new ValidationResult("Check-out must be after check-in.", [nameof(CheckOut)]);
        else if (CheckOut.DayNumber - CheckIn.DayNumber > AvailabilityQuery.MaxNights)
            yield return new ValidationResult(
                $"A stay can be at most {AvailabilityQuery.MaxNights} nights.", [nameof(CheckOut)]);
    }
}

public record ReservationResponse(
    int Id,
    int RoomId,
    string RoomNumber,
    RoomType RoomType,
    int GuestId,
    string GuestName,
    string GuestEmail,
    DateOnly CheckIn,
    DateOnly CheckOut,
    int Nights,
    int NumberOfGuests,
    decimal TotalPrice,
    ReservationStatus Status,
    DateTime CreatedAtUtc,
    DateTime? CheckedInAtUtc,
    DateTime? CheckedOutAtUtc,
    DateTime? CancelledAtUtc)
{
    /// <remarks>Expects Room and Guest to be loaded.</remarks>
    public static ReservationResponse From(Reservation r) => new(
        r.Id, r.RoomId, r.Room.Number, r.Room.Type, r.GuestId, r.Guest.FullName, r.Guest.Email,
        r.CheckInDate, r.CheckOutDate, r.Nights, r.NumberOfGuests, r.TotalPrice, r.Status,
        r.CreatedAtUtc, r.CheckedInAtUtc, r.CheckedOutAtUtc, r.CancelledAtUtc);
}
