using HotelBooking.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBooking.Infrastructure.Configurations;

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.TotalPrice).HasPrecision(10, 2);

        // Restrict: rooms and users with reservations can't be deleted out from under them.
        builder.HasOne(r => r.Room).WithMany().HasForeignKey(r => r.RoomId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(r => r.Guest).WithMany().HasForeignKey(r => r.GuestId).OnDelete(DeleteBehavior.Restrict);

        // Speeds up availability searches and arrivals/departures lookups.
        builder.HasIndex(r => new { r.RoomId, r.CheckInDate, r.CheckOutDate });
        builder.HasIndex(r => r.CheckInDate);
        builder.HasIndex(r => r.CheckOutDate);
    }
}
