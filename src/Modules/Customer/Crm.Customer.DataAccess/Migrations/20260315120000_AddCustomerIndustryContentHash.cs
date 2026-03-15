using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crm.Customer.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerIndustryContentHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentHash",
                schema: "customer",
                table: "CustomerIndustry",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentHash",
                schema: "customer",
                table: "CustomerIndustry");
        }
    }
}
