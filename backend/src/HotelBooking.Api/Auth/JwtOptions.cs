using System.ComponentModel.DataAnnotations;

namespace HotelBooking.Api.Auth;

/// <summary>Bound from the "Jwt" configuration section.</summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; set; } = "";

    [Required]
    public string Audience { get; set; } = "";

    /// <summary>HMAC-SHA256 secret. Must be at least 32 characters; supply it via environment variable outside development.</summary>
    [Required, MinLength(32)]
    public string SigningKey { get; set; } = "";

    [Range(1, 24 * 60)]
    public int ExpiresMinutes { get; set; } = 120;
}
