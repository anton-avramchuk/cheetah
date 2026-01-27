using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cheetah.Admin.Modules.Clients.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class MakeClientNameUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Client_Name",
                schema: "clients",
                table: "Client");

            migrationBuilder.CreateIndex(
                name: "IX_Client_Name",
                schema: "clients",
                table: "Client",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Client_Name",
                schema: "clients",
                table: "Client");

            migrationBuilder.CreateIndex(
                name: "IX_Client_Name",
                schema: "clients",
                table: "Client",
                column: "Name");
        }
    }
}
