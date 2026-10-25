using System.ComponentModel.DataAnnotations;
using HotelBooking.Domain;

namespace HotelBooking.Api.Contracts;

public record RoomResponse(
    int Id,
    string Number,
    RoomType Type,
    int Capacity,
    decimal PricePerNight,
    string? Description)
{
    public static RoomResponse From(Room room) =>
        new(room.Id, room.Number, room.Type, room.Capacity, room.PricePerNight, room.Description);
}

public record RoomRequest(
    [Required, MaxLength(10)] string Number,
    RoomType Type,
    [Range(1, 10)] int Capacity,
    [Range(1, 100_000)] decimal PricePerNight,
    [MaxLength(500)] string? Description);

public record AvailableRoomResponse(
    int Id,
    string Number,
    RoomType Type,
    int Capacity,
    decimal PricePerNight,
    string? Description,
    int Nights,
    decimal TotalPrice);

public class AvailabilityQuery : IValidatableObject
{
    public const int MaxNights = 30;

    /// <example>2026-12-01</example>
    [Required]
    public DateOnly? CheckIn { get; set; }

    /// <example>2026-12-04</example>
    [Required]
    public DateOnly? CheckOut { get; set; }

    [Range(1, 10)]
    public int Guests { get; set; } = 1;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CheckIn is null || CheckOut is null)
            yield break;
        if (CheckOut <= CheckIn)
            yield return new ValidationResult("Check-out must be after check-in.", [nameof(CheckOut)]);
        else if (CheckOut.Value.DayNumber - CheckIn.Value.DayNumber > MaxNights)
            yield return new ValidationResult($"A stay can be at most {MaxNights} nights.", [nameof(CheckOut)]);
    }
}
