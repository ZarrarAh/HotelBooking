using System.ComponentModel.DataAnnotations;
using HotelBooking.Domain;

namespace HotelBooking.Api.Contracts;

public record RegisterRequest(
    [Required, EmailAddress, MaxLength(256)] string Email,
    [Required, MinLength(8), MaxLength(100)] string Password,
    [Required, MaxLength(200)] string FullName);

public record LoginRequest(
    [Required] string Email,
    [Required] string Password);

public record UserResponse(int Id, string Email, string FullName, UserRole Role)
{
    public static UserResponse From(User user) => new(user.Id, user.Email, user.FullName, user.Role);
}

public record AuthResponse(string Token, DateTime ExpiresAtUtc, UserResponse User);
