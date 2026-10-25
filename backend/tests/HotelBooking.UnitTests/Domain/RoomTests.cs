using HotelBooking.Domain;

namespace HotelBooking.UnitTests.Domain;

public class RoomTests
{
    [Fact]
    public void Constructor_ValidInput_SetsProperties()
    {
        var room = new Room(" 101 ", RoomType.Suite, capacity: 4, pricePerNight: 250m, "Sea view");

        Assert.Equal("101", room.Number);
        Assert.Equal(RoomType.Suite, room.Type);
        Assert.Equal(4, room.Capacity);
        Assert.Equal(250m, room.PricePerNight);
    }

    [Theory]
    [InlineData("", 2, 100)]
    [InlineData("101", 0, 100)]
    [InlineData("101", 2, 0)]
    public void Constructor_InvalidInput_Throws(string number, int capacity, decimal price)
    {
        Assert.Throws<DomainException>(() => new Room(number, RoomType.Double, capacity, price));
    }
}
