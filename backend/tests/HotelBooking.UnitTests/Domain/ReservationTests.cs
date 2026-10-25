using HotelBooking.Domain;

namespace HotelBooking.UnitTests.Domain;

public class ReservationTests
{
    private static readonly DateTime Now = new(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateOnly Today = DateOnly.FromDateTime(Now);

    private static Room DoubleRoom() => new("101", RoomType.Double, capacity: 2, pricePerNight: 120m);

    private static Reservation ConfirmedReservation(int startsInDays = 0, int nights = 3) =>
        Reservation.Create(DoubleRoom(), guestId: 1, Today.AddDays(startsInDays),
            Today.AddDays(startsInDays + nights), numberOfGuests: 2, Now);

    [Fact]
    public void Create_ValidInput_IsConfirmedWithTotalPrice()
    {
        var reservation = ConfirmedReservation(startsInDays: 5, nights: 3);

        Assert.Equal(ReservationStatus.Confirmed, reservation.Status);
        Assert.Equal(3, reservation.Nights);
        Assert.Equal(360m, reservation.TotalPrice);
    }

    [Fact]
    public void Create_CheckInInThePast_Throws()
    {
        Assert.Throws<DomainException>(() =>
            Reservation.Create(DoubleRoom(), 1, Today.AddDays(-1), Today.AddDays(2), 1, Now));
    }

    [Theory]
    [InlineData(0)]  // same day
    [InlineData(-1)] // check-out before check-in
    public void Create_CheckOutNotAfterCheckIn_Throws(int nights)
    {
        Assert.Throws<DomainException>(() =>
            Reservation.Create(DoubleRoom(), 1, Today, Today.AddDays(nights), 1, Now));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)] // room capacity is 2
    public void Create_InvalidNumberOfGuests_Throws(int guests)
    {
        Assert.Throws<DomainException>(() =>
            Reservation.Create(DoubleRoom(), 1, Today, Today.AddDays(1), guests, Now));
    }

    [Fact]
    public void CheckIn_OnArrivalDay_SetsCheckedIn()
    {
        var reservation = ConfirmedReservation();

        reservation.CheckIn(Now);

        Assert.Equal(ReservationStatus.CheckedIn, reservation.Status);
        Assert.Equal(Now, reservation.CheckedInAtUtc);
    }

    [Fact]
    public void CheckIn_BeforeArrivalDay_Throws()
    {
        var reservation = ConfirmedReservation(startsInDays: 2);

        Assert.Throws<DomainException>(() => reservation.CheckIn(Now));
    }

    [Fact]
    public void CheckIn_OnCheckOutDay_Throws()
    {
        var reservation = ConfirmedReservation(nights: 2);

        Assert.Throws<DomainException>(() => reservation.CheckIn(Now.AddDays(2)));
    }

    [Fact]
    public void CheckIn_Cancelled_Throws()
    {
        var reservation = ConfirmedReservation();
        reservation.Cancel(Now);

        Assert.Throws<DomainException>(() => reservation.CheckIn(Now));
    }

    [Fact]
    public void CheckOut_AfterCheckIn_SetsCheckedOut()
    {
        var reservation = ConfirmedReservation();
        reservation.CheckIn(Now);

        reservation.CheckOut(Now.AddDays(3));

        Assert.Equal(ReservationStatus.CheckedOut, reservation.Status);
        Assert.NotNull(reservation.CheckedOutAtUtc);
    }

    [Fact]
    public void CheckOut_WithoutCheckIn_Throws()
    {
        var reservation = ConfirmedReservation();

        Assert.Throws<DomainException>(() => reservation.CheckOut(Now));
    }

    [Fact]
    public void Cancel_Confirmed_SetsCancelled()
    {
        var reservation = ConfirmedReservation(startsInDays: 5);

        reservation.Cancel(Now);

        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
    }

    [Fact]
    public void Cancel_AfterCheckIn_Throws()
    {
        var reservation = ConfirmedReservation();
        reservation.CheckIn(Now);

        Assert.Throws<DomainException>(() => reservation.Cancel(Now));
    }
}
