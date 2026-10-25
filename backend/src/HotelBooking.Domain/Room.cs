namespace HotelBooking.Domain;

public class Room
{
    public int Id { get; private set; }
    public string Number { get; private set; } = null!;
    public RoomType Type { get; private set; }
    public int Capacity { get; private set; }
    public decimal PricePerNight { get; private set; }
    public string? Description { get; private set; }

    // Required by EF Core.
    private Room() { }

    public Room(string number, RoomType type, int capacity, decimal pricePerNight, string? description = null)
    {
        Update(number, type, capacity, pricePerNight, description);
    }

    public void Update(string number, RoomType type, int capacity, decimal pricePerNight, string? description)
    {
        if (string.IsNullOrWhiteSpace(number))
            throw new DomainException("Room number is required.");
        if (capacity < 1)
            throw new DomainException("Room capacity must be at least 1.");
        if (pricePerNight <= 0)
            throw new DomainException("Price per night must be greater than zero.");

        Number = number.Trim();
        Type = type;
        Capacity = capacity;
        PricePerNight = pricePerNight;
        Description = description;
    }
}
