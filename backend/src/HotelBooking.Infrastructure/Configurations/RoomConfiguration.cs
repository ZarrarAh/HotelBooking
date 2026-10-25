using HotelBooking.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBooking.Infrastructure.Configurations;

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.Property(r => r.Number).HasMaxLength(10);
        builder.HasIndex(r => r.Number).IsUnique();
        builder.Property(r => r.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.PricePerNight).HasPrecision(10, 2);
        builder.Property(r => r.Description).HasMaxLength(500);
    }
}
