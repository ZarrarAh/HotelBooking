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
