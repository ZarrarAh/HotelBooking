using HotelBooking.Api.Auth;
using HotelBooking.Api.Contracts;
using HotelBooking.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AuthService auth) : ControllerBase
{
    /// <summary>Registers a new guest account and returns a JWT.</summary>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request) =>
        await auth.RegisterAsync(request);

    /// <summary>Exchanges email and password for a JWT.</summary>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var response = await auth.LoginAsync(request);
        return response is null
            ? Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Invalid email or password.")
            : response;
    }

    /// <summary>Returns the currently signed-in user.</summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserResponse>> Me()
    {
        var user = await auth.GetUserAsync(User.GetUserId());
        return user is null ? Unauthorized() : user;
    }
}
