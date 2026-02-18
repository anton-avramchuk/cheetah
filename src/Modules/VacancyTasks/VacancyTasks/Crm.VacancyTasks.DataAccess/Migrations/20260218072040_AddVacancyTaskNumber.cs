using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crm.VacancyTasks.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddVacancyTaskNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Number",
                schema: "vacancytasks",
                table: "VacancyTask",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_VacancyTask_VacancyId_Number",
                schema: "vacancytasks",
                table: "VacancyTask",
                columns: new[] { "VacancyId", "Number" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VacancyTask_VacancyId_Number",
                schema: "vacancytasks",
                table: "VacancyTask");

            migrationBuilder.DropColumn(
                name: "Number",
                schema: "vacancytasks",
                table: "VacancyTask");
        }
    }
}
