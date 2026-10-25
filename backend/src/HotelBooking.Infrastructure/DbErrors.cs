using HotelBooking.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HotelBooking.Infrastructure;

/// <summary>Recognizes PostgreSQL constraint violations so callers don't depend on Npgsql directly.</summary>
public static class DbErrors
{
    /// <summary>True when the insert was rejected because the room is already booked for an overlapping range.</summary>
    public static bool IsReservationOverlap(DbUpdateException ex) =>
        ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.ExclusionViolation,
            ConstraintName: ReservationConfiguration.NoOverlapConstraint
        };

    /// <summary>True when a unique index (e.g. on users.email) rejected the write.</summary>
    public static bool IsUniqueViolation(DbUpdateException ex) =>
        ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
}
