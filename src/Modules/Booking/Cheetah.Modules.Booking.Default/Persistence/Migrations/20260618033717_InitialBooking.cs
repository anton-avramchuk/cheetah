using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cheetah.Modules.Booking.Default.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "booking");

            migrationBuilder.CreateTable(
                name: "AvailabilitySchedules",
                schema: "booking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HostUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TimeZoneId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvailabilitySchedules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                schema: "booking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    HostUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    InviteeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    InviteeEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    InviteePhone = table.Column<string>(type: "text", nullable: true),
                    InviteeTimeZone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StartUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CalendarEventId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedLeadId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedActivityId = table.Column<Guid>(type: "uuid", nullable: true),
                    ManageToken = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CancelReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Attributes = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BookingTypes",
                schema: "booking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HostUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    LocationKind = table.Column<int>(type: "integer", nullable: false),
                    LocationDetails = table.Column<string>(type: "text", nullable: true),
                    BufferBeforeMinutes = table.Column<int>(type: "integer", nullable: false),
                    BufferAfterMinutes = table.Column<int>(type: "integer", nullable: false),
                    MinNoticeMinutes = table.Column<int>(type: "integer", nullable: false),
                    MaxAdvanceDays = table.Column<int>(type: "integer", nullable: false),
                    SlotStepMinutes = table.Column<int>(type: "integer", nullable: true),
                    Color = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Attributes = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RemovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AvailabilityDateOverrides",
                schema: "booking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    IsUnavailable = table.Column<bool>(type: "boolean", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvailabilityDateOverrides", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AvailabilityDateOverrides_AvailabilitySchedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalSchema: "booking",
                        principalTable: "AvailabilitySchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WeeklyAvailabilityRules",
                schema: "booking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduleId = table.Column<Guid>(type: "uuid", nullable: false),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeeklyAvailabilityRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeeklyAvailabilityRules_AvailabilitySchedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalSchema: "booking",
                        principalTable: "AvailabilitySchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookingAnswers",
                schema: "booking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    Question = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingAnswers_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalSchema: "booking",
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AvailabilityDateOverrides_ScheduleId",
                schema: "booking",
                table: "AvailabilityDateOverrides",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_AvailabilitySchedules_HostUserId",
                schema: "booking",
                table: "AvailabilitySchedules",
                column: "HostUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookingAnswers_BookingId",
                schema: "booking",
                table: "BookingAnswers",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_HostUserId_StartUtc",
                schema: "booking",
                table: "Bookings",
                columns: new[] { "HostUserId", "StartUtc" },
                unique: true,
                filter: "\"Status\" IN (0, 1)");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_HostUserId_Status_StartUtc",
                schema: "booking",
                table: "Bookings",
                columns: new[] { "HostUserId", "Status", "StartUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_ManageToken",
                schema: "booking",
                table: "Bookings",
                column: "ManageToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookingTypes_HostUserId",
                schema: "booking",
                table: "BookingTypes",
                column: "HostUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingTypes_Slug",
                schema: "booking",
                table: "BookingTypes",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyAvailabilityRules_ScheduleId",
                schema: "booking",
                table: "WeeklyAvailabilityRules",
                column: "ScheduleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AvailabilityDateOverrides",
                schema: "booking");

            migrationBuilder.DropTable(
                name: "BookingAnswers",
                schema: "booking");

            migrationBuilder.DropTable(
                name: "BookingTypes",
                schema: "booking");

            migrationBuilder.DropTable(
                name: "WeeklyAvailabilityRules",
                schema: "booking");

            migrationBuilder.DropTable(
                name: "Bookings",
                schema: "booking");

            migrationBuilder.DropTable(
                name: "AvailabilitySchedules",
                schema: "booking");
        }
    }
}
