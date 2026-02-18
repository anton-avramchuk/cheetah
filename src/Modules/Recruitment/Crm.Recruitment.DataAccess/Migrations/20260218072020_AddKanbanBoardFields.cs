using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crm.Recruitment.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddKanbanBoardFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Color",
                schema: "recruitment",
                table: "VacancyState",
                type: "character varying(9)",
                maxLength: 9,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                schema: "recruitment",
                table: "VacancyState",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Order",
                schema: "recruitment",
                table: "Vacancy",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                schema: "recruitment",
                table: "Customer",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customer_Code",
                schema: "recruitment",
                table: "Customer",
                column: "Code",
                unique: true,
                filter: "\"Code\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Customer_Code",
                schema: "recruitment",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "Color",
                schema: "recruitment",
                table: "VacancyState");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                schema: "recruitment",
                table: "VacancyState");

            migrationBuilder.DropColumn(
                name: "Order",
                schema: "recruitment",
                table: "Vacancy");

            migrationBuilder.DropColumn(
                name: "Code",
                schema: "recruitment",
                table: "Customer");
        }
    }
}
