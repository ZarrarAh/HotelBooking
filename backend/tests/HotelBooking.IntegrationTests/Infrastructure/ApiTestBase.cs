using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using HotelBooking.Api.Contracts;
using HotelBooking.Domain;

namespace HotelBooking.IntegrationTests.Infrastructure;

/// <summary>
/// Helpers shared by the integration tests. Tests share one database, so each test creates its own
/// guests and rooms (with unique emails / room numbers) instead of relying on seeded data.
/// </summary>
[Collection(ApiCollection.Name)]
public abstract class ApiTestBase(HotelApiFactory factory)
{
    protected static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    protected static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    protected HttpClient Anonymous() => factory.CreateClient();

    protected HttpClient Authenticated(string token)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    protected async Task<HttpClient> StaffAsync() =>
        Authenticated(await LoginAsync(HotelApiFactory.StaffEmail, HotelApiFactory.StaffPassword));

    protected async Task<HttpClient> NewGuestAsync() => Authenticated((await RegisterGuestAsync()).Token);

    protected async Task<AuthResponse> RegisterGuestAsync(string? email = null)
    {
        var response = await Anonymous().PostAsJsonAsync("/api/auth/register",
            new RegisterRequest(email ?? UniqueEmail(), "Password1!", "Test Guest"), Json);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthResponse>(Json))!;
    }

    protected async Task<string> LoginAsync(string email, string password)
    {
        var response = await Anonymous().PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password), Json);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthResponse>(Json))!.Token;
    }

    /// <summary>Creates a fresh room so the test's bookings can't collide with other tests.</summary>
    protected async Task<RoomResponse> CreateRoomAsync(int capacity = 2, decimal price = 100m)
    {
        var staff = await StaffAsync();
        var number = Random.Shared.Next(1000, 9_999_999).ToString();
        var response = await staff.PostAsJsonAsync("/api/rooms",
            new RoomRequest(number, RoomType.Double, capacity, price, null), Json);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RoomResponse>(Json))!;
    }

    protected static Task<HttpResponseMessage> BookAsync(
        HttpClient guest, int roomId, DateOnly checkIn, DateOnly checkOut, int guests = 1) =>
        guest.PostAsJsonAsync("/api/reservations",
            new CreateReservationRequest(roomId, checkIn, checkOut, guests), Json);

    protected static async Task<T> ReadAsync<T>(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<T>(Json))!;

    protected static string UniqueEmail() => $"guest-{Guid.NewGuid():N}@test.local";
}
