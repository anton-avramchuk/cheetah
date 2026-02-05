using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crm.Recruitment.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddVacancyAssignmentSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "StateId",
                schema: "recruitment",
                table: "Vacancy",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "User",
                schema: "recruitment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VacancyRole",
                schema: "recruitment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsSingle = table.Column<bool>(type: "boolean", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VacancyRole", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VacancyState",
                schema: "recruitment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VacancyState", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VacancyAssignment",
                schema: "recruitment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VacancyId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VacancyAssignment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VacancyAssignment_User_UserId",
                        column: x => x.UserId,
                        principalSchema: "recruitment",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VacancyAssignment_VacancyRole_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "recruitment",
                        principalTable: "VacancyRole",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VacancyAssignment_Vacancy_VacancyId",
                        column: x => x.VacancyId,
                        principalSchema: "recruitment",
                        principalTable: "Vacancy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Vacancy_StateId",
                schema: "recruitment",
                table: "Vacancy",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                schema: "recruitment",
                table: "User",
                column: "Email",
                unique: true,
                filter: "\"Email\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_VacancyAssignment_RoleId",
                schema: "recruitment",
                table: "VacancyAssignment",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_VacancyAssignment_UserId",
                schema: "recruitment",
                table: "VacancyAssignment",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VacancyAssignment_VacancyId_UserId_RoleId",
                schema: "recruitment",
                table: "VacancyAssignment",
                columns: new[] { "VacancyId", "UserId", "RoleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VacancyRole_Code",
                schema: "recruitment",
                table: "VacancyRole",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VacancyState_Name",
                schema: "recruitment",
                table: "VacancyState",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Vacancy_VacancyState_StateId",
                schema: "recruitment",
                table: "Vacancy",
                column: "StateId",
                principalSchema: "recruitment",
                principalTable: "VacancyState",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vacancy_VacancyState_StateId",
                schema: "recruitment",
                table: "Vacancy");

            migrationBuilder.DropTable(
                name: "VacancyAssignment",
                schema: "recruitment");

            migrationBuilder.DropTable(
                name: "VacancyState",
                schema: "recruitment");

            migrationBuilder.DropTable(
                name: "User",
                schema: "recruitment");

            migrationBuilder.DropTable(
                name: "VacancyRole",
                schema: "recruitment");

            migrationBuilder.DropIndex(
                name: "IX_Vacancy_StateId",
                schema: "recruitment",
                table: "Vacancy");

            migrationBuilder.DropColumn(
                name: "StateId",
                schema: "recruitment",
                table: "Vacancy");
        }
    }
}
