using HotelBooking.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReservationOverlapConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "ck_reservations_check_out_after_check_in",
                table: "reservations",
                sql: "check_out_date > check_in_date");

            // btree_gist lets a GiST index combine plain equality (room_id =) with range overlap (&&).
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS btree_gist;");

            // No two active reservations of the same room may have overlapping [check_in, check_out) ranges.
            // '[)' makes the check-out day exclusive, so back-to-back stays are allowed.
            migrationBuilder.Sql($"""
                ALTER TABLE reservations
                ADD CONSTRAINT {ReservationConfiguration.NoOverlapConstraint}
                EXCLUDE USING gist (
                    room_id WITH =,
                    daterange(check_in_date, check_out_date, '[)') WITH &&
                )
                WHERE (status <> 'Cancelled');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                $"ALTER TABLE reservations DROP CONSTRAINT IF EXISTS {ReservationConfiguration.NoOverlapConstraint};");

            migrationBuilder.DropCheckConstraint(
                name: "ck_reservations_check_out_after_check_in",
                table: "reservations");
        }
    }
}
