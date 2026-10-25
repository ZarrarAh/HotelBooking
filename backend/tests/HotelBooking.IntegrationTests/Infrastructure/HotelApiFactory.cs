using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace HotelBooking.IntegrationTests.Infrastructure;

/// <summary>
/// Boots the real API in memory against a throwaway PostgreSQL container, so tests exercise the
/// actual migrations and constraints (including the double-booking exclusion constraint).
/// </summary>
public class HotelApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string StaffEmail = "staff@test.local";
    public const string StaffPassword = "StaffPassword1!";

    private readonly PostgreSqlContainer _db = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Default", _db.GetConnectionString());
        builder.UseSetting("Database:InitializeOnStartup", "true");
        builder.UseSetting("Jwt:SigningKey", "integration-test-signing-key-0123456789abcdef");
        builder.UseSetting("Seed:StaffEmail", StaffEmail);
        builder.UseSetting("Seed:StaffPassword", StaffPassword);
    }

    public Task InitializeAsync() => _db.StartAsync();

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await _db.DisposeAsync();
    }
}

/// <summary>All test classes share one API instance and database container.</summary>
[CollectionDefinition(Name)]
public class ApiCollection : ICollectionFixture<HotelApiFactory>
{
    public const string Name = "api";
}
