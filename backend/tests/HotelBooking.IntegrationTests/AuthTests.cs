using System.Net;
using System.Net.Http.Json;
using HotelBooking.Api.Contracts;
using HotelBooking.Domain;
using HotelBooking.IntegrationTests.Infrastructure;

namespace HotelBooking.IntegrationTests;

public class AuthTests(HotelApiFactory factory) : ApiTestBase(factory)
{
    [Fact]
    public async Task Register_ReturnsTokenForGuest()
    {
        var auth = await RegisterGuestAsync();

        Assert.False(string.IsNullOrEmpty(auth.Token));
        Assert.Equal(UserRole.Guest, auth.User.Role);

        var me = await ReadAsync<UserResponse>(await Authenticated(auth.Token).GetAsync("/api/auth/me"));
        Assert.Equal(auth.User.Email, me.Email);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        var email = UniqueEmail();
        await RegisterGuestAsync(email);

        var response = await Anonymous().PostAsJsonAsync("/api/auth/register",
            new RegisterRequest(email.ToUpperInvariant(), "Password1!", "Someone Else"), Json);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var auth = await RegisterGuestAsync();

        var response = await Anonymous().PostAsJsonAsync("/api/auth/login",
            new LoginRequest(auth.User.Email, "not-the-password"), Json);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task StaffEndpoint_WithoutToken_Returns401()
    {
        var response = await Anonymous().GetAsync("/api/staff/arrivals");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task StaffEndpoint_AsGuest_Returns403()
    {
        var guest = await NewGuestAsync();

        var response = await guest.GetAsync("/api/staff/arrivals");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
