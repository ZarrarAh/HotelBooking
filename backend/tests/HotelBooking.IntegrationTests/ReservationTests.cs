using System.Net;
using HotelBooking.Api.Contracts;
using HotelBooking.Domain;
using HotelBooking.IntegrationTests.Infrastructure;

namespace HotelBooking.IntegrationTests;

public class ReservationTests(HotelApiFactory factory) : ApiTestBase(factory)
{
    private static readonly DateOnly CheckIn = Today.AddDays(10);

    [Fact]
    public async Task Book_FreeRoom_Returns201WithTotalPrice()
    {
        var room = await CreateRoomAsync(price: 150m);
        var guest = await NewGuestAsync();

        var response = await BookAsync(guest, room.Id, CheckIn, CheckIn.AddDays(3), guests: 2);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var reservation = await ReadAsync<ReservationResponse>(response);
        Assert.Equal(ReservationStatus.Confirmed, reservation.Status);
        Assert.Equal(450m, reservation.TotalPrice);
    }

    [Fact]
    public async Task Book_OverlappingDates_Returns409()
    {
        var room = await CreateRoomAsync();
        await BookAsync(await NewGuestAsync(), room.Id, CheckIn, CheckIn.AddDays(3));

        var response = await BookAsync(await NewGuestAsync(), room.Id, CheckIn.AddDays(2), CheckIn.AddDays(4));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Book_BackToBack_IsAllowed()
    {
        var room = await CreateRoomAsync();
        await BookAsync(await NewGuestAsync(), room.Id, CheckIn, CheckIn.AddDays(3));

        // The first guest leaves on the morning the second one arrives.
        var response = await BookAsync(await NewGuestAsync(), room.Id, CheckIn.AddDays(3), CheckIn.AddDays(5));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Book_ConcurrentRequestsForSameRoom_OnlyOneSucceeds()
    {
        var room = await CreateRoomAsync();
        var guests = await Task.WhenAll(Enumerable.Range(0, 10).Select(_ => NewGuestAsync()));

        // Fire all requests at once so several pass the application-level check before any insert commits;
        // the PostgreSQL exclusion constraint must reject all but one.
        var responses = await Task.WhenAll(guests.Select(g => BookAsync(g, room.Id, CheckIn, CheckIn.AddDays(2))));

        Assert.Equal(1, responses.Count(r => r.StatusCode == HttpStatusCode.Created));
        Assert.Equal(9, responses.Count(r => r.StatusCode == HttpStatusCode.Conflict));
    }

    [Fact]
    public async Task Availability_ExcludesBookedRoom_UntilCancelled()
    {
        var room = await CreateRoomAsync();
        var guest = await NewGuestAsync();
        var booked = await ReadAsync<ReservationResponse>(
            await BookAsync(guest, room.Id, CheckIn, CheckIn.AddDays(2)));
        var url = $"/api/rooms/available?checkIn={CheckIn:yyyy-MM-dd}&checkOut={CheckIn.AddDays(1):yyyy-MM-dd}";

        var whileBooked = await ReadAsync<List<AvailableRoomResponse>>(await Anonymous().GetAsync(url));
        Assert.DoesNotContain(whileBooked, r => r.Id == room.Id);

        var cancel = await guest.PostAsync($"/api/reservations/{booked.Id}/cancel", null);
        Assert.Equal(HttpStatusCode.OK, cancel.StatusCode);

        var afterCancel = await ReadAsync<List<AvailableRoomResponse>>(await Anonymous().GetAsync(url));
        Assert.Contains(afterCancel, r => r.Id == room.Id);
    }

    [Fact]
    public async Task Availability_RespectsCapacity()
    {
        var room = await CreateRoomAsync(capacity: 2);
        var url = $"/api/rooms/available?checkIn={CheckIn:yyyy-MM-dd}&checkOut={CheckIn.AddDays(1):yyyy-MM-dd}&guests=3";

        var rooms = await ReadAsync<List<AvailableRoomResponse>>(await Anonymous().GetAsync(url));

        Assert.DoesNotContain(rooms, r => r.Id == room.Id);
    }

    [Fact]
    public async Task Book_InThePast_Returns400()
    {
        var room = await CreateRoomAsync();

        var response = await BookAsync(await NewGuestAsync(), room.Id, Today.AddDays(-2), Today);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetReservation_OfAnotherGuest_Returns404()
    {
        var room = await CreateRoomAsync();
        var owner = await NewGuestAsync();
        var reservation = await ReadAsync<ReservationResponse>(
            await BookAsync(owner, room.Id, CheckIn, CheckIn.AddDays(1)));

        var response = await (await NewGuestAsync()).GetAsync($"/api/reservations/{reservation.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Mine_ReturnsOnlyOwnReservations()
    {
        var room = await CreateRoomAsync();
        var me = await NewGuestAsync();
        await BookAsync(me, room.Id, CheckIn, CheckIn.AddDays(1));
        await BookAsync(await NewGuestAsync(), room.Id, CheckIn.AddDays(1), CheckIn.AddDays(2));

        var mine = await ReadAsync<List<ReservationResponse>>(await me.GetAsync("/api/reservations/mine"));

        Assert.Single(mine);
    }
}
