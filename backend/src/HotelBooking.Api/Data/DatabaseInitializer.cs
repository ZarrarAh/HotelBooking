using HotelBooking.Domain;
using HotelBooking.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Api.Data;

/// <summary>
/// Applies pending migrations and inserts demo data (rooms + a staff account) into an empty database.
/// </summary>
public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

        await db.Database.MigrateAsync();

        if (!await db.Rooms.AnyAsync())
        {
            db.Rooms.AddRange(
                new Room("101", RoomType.Single, 1, 79m, "Cozy single room with a city view."),
                new Room("102", RoomType.Single, 1, 79m, "Quiet single room facing the courtyard."),
                new Room("201", RoomType.Double, 2, 119m, "Double room with a queen bed."),
                new Room("202", RoomType.Double, 2, 129m, "Double room with a balcony."),
                new Room("203", RoomType.Double, 3, 139m, "Double room with an extra sofa bed."),
                new Room("301", RoomType.Suite, 4, 249m, "Suite with a living area and sea view."));
        }

        var staffEmail = config["Seed:StaffEmail"];
        var staffPassword = config["Seed:StaffPassword"];
        if (!string.IsNullOrWhiteSpace(staffEmail) && !string.IsNullOrWhiteSpace(staffPassword)
            && !await db.Users.AnyAsync(u => u.Role == UserRole.Staff))
        {
            var staff = new User(staffEmail, "Front Desk", UserRole.Staff);
            staff.SetPasswordHash(hasher.HashPassword(staff, staffPassword));
            db.Users.Add(staff);
        }

        await db.SaveChangesAsync();
    }
}
