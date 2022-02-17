using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DotnetToLambda.Core.Infrastructure.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    BookingId = table.Column<string>(nullable: false),
                    OutboundFlightId = table.Column<string>(nullable: true),
                    CustomerId = table.Column<string>(nullable: true),
                    ChargeId = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    ConfirmedOn = table.Column<DateTime>(nullable: true),
                    CancellationReason = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingId", x => x.BookingId);
                });

            migrationBuilder.CreateIndex(
                name: "UX_Booking_CustomerId",
                table: "Bookings",
                column: "CustomerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bookings");
        }
    }
}
