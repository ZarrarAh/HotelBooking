using System.Net;
using HotelBooking.Api.Contracts;
using HotelBooking.Domain;
using HotelBooking.IntegrationTests.Infrastructure;

namespace HotelBooking.IntegrationTests;

public class StaffTests(HotelApiFactory factory) : ApiTestBase(factory)
{
    [Fact]
    public async Task FullStay_ArrivalCheckInDepartureCheckOut()
    {
        var room = await CreateRoomAsync();
        var guest = await NewGuestAsync();
        var staff = await StaffAsync();
        var reservation = await ReadAsync<ReservationResponse>(
            await BookAsync(guest, room.Id, Today, Today.AddDays(2)));

        var arrivals = await ReadAsync<List<ReservationResponse>>(await staff.GetAsync("/api/staff/arrivals"));
        Assert.Contains(arrivals, r => r.Id == reservation.Id);

        var checkedIn = await ReadAsync<ReservationResponse>(
            await staff.PostAsync($"/api/staff/reservations/{reservation.Id}/check-in", null));
        Assert.Equal(ReservationStatus.CheckedIn, checkedIn.Status);

        var departures = await ReadAsync<List<ReservationResponse>>(
            await staff.GetAsync($"/api/staff/departures?date={Today.AddDays(2):yyyy-MM-dd}"));
        Assert.Contains(departures, r => r.Id == reservation.Id);

        var checkedOut = await ReadAsync<ReservationResponse>(
            await staff.PostAsync($"/api/staff/reservations/{reservation.Id}/check-out", null));
        Assert.Equal(ReservationStatus.CheckedOut, checkedOut.Status);
    }

    [Fact]
    public async Task CheckIn_BeforeArrivalDate_Returns400()
    {
        var room = await CreateRoomAsync();
        var reservation = await ReadAsync<ReservationResponse>(
            await BookAsync(await NewGuestAsync(), room.Id, Today.AddDays(5), Today.AddDays(6)));

        var response = await (await StaffAsync()).PostAsync($"/api/staff/reservations/{reservation.Id}/check-in", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CheckOut_WithoutCheckIn_Returns400()
    {
        var room = await CreateRoomAsync();
        var reservation = await ReadAsync<ReservationResponse>(
            await BookAsync(await NewGuestAsync(), room.Id, Today, Today.AddDays(1)));

        var response = await (await StaffAsync()).PostAsync($"/api/staff/reservations/{reservation.Id}/check-out", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CheckIn_UnknownReservation_Returns404()
    {
        var response = await (await StaffAsync()).PostAsync("/api/staff/reservations/999999/check-in", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
