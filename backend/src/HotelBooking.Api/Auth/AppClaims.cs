using System.Security.Claims;

namespace HotelBooking.Api.Auth;

/// <summary>
/// Short JWT claim names. Inbound claim mapping is turned off in Program.cs, so these
/// arrive in User.Claims exactly as written here.
/// </summary>
public static class AppClaims
{
    public const string UserId = "sub";
    public const string Email = "email";
    public const string Name = "name";
    public const string Role = "role";

    public static int GetUserId(this ClaimsPrincipal user) =>
        int.Parse(user.FindFirstValue(UserId) ?? throw new InvalidOperationException("Missing user id claim."));
}
