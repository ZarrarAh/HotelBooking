using HotelBooking.Api.Auth;
using HotelBooking.Api.Contracts;
using HotelBooking.Api.Errors;
using HotelBooking.Domain;
using HotelBooking.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Api.Services;

public class AuthService(AppDbContext db, IPasswordHasher<User> hasher, TokenService tokens)
{
    /// <summary>Creates a Guest account. Staff accounts are only created by seeding.</summary>
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(u => u.Email == email))
            throw new ConflictException("An account with this email already exists.");

        var user = new User(email, request.FullName, UserRole.Guest);
        user.SetPasswordHash(hasher.HashPassword(user, request.Password));
        db.Users.Add(user);
        await db.SaveChangesAsync();

        return CreateResponse(user);
    }

    /// <returns>null when the email or password is wrong (deliberately not saying which).</returns>
    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null)
            return null;

        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            return null;

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.SetPasswordHash(hasher.HashPassword(user, request.Password));
            await db.SaveChangesAsync();
        }

        return CreateResponse(user);
    }

    public async Task<UserResponse?> GetUserAsync(int userId)
    {
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        return user is null ? null : UserResponse.From(user);
    }

    private AuthResponse CreateResponse(User user)
    {
        var (token, expires) = tokens.CreateToken(user);
        return new AuthResponse(token, expires, UserResponse.From(user));
    }
}
