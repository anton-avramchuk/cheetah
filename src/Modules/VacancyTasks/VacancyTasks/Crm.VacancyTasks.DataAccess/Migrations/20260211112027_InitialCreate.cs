using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crm.VacancyTasks.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "vacancytasks");

            migrationBuilder.CreateTable(
                name: "VacancyTask",
                schema: "vacancytasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VacancyTask", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VacancyTask_Name",
                schema: "vacancytasks",
                table: "VacancyTask",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VacancyTask",
                schema: "vacancytasks");
        }
    }
}
