using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crm.Recruitment.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerPositionStackItemWorkFormat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                schema: "recruitment",
                table: "Vacancy",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PositionId",
                schema: "recruitment",
                table: "Vacancy",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StackItemId",
                schema: "recruitment",
                table: "Vacancy",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WorkFormatId",
                schema: "recruitment",
                table: "Vacancy",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CustomerDirection",
                schema: "recruitment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerDirection", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Position",
                schema: "recruitment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Position", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StackItem",
                schema: "recruitment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StackItem", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkFormat",
                schema: "recruitment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkFormat", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Customer",
                schema: "recruitment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    DirectionId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Customer_CustomerDirection_DirectionId",
                        column: x => x.DirectionId,
                        principalSchema: "recruitment",
                        principalTable: "CustomerDirection",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Vacancy_CustomerId",
                schema: "recruitment",
                table: "Vacancy",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Vacancy_PositionId",
                schema: "recruitment",
                table: "Vacancy",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_Vacancy_StackItemId",
                schema: "recruitment",
                table: "Vacancy",
                column: "StackItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Vacancy_WorkFormatId",
                schema: "recruitment",
                table: "Vacancy",
                column: "WorkFormatId");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_DirectionId",
                schema: "recruitment",
                table: "Customer",
                column: "DirectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_Name",
                schema: "recruitment",
                table: "Customer",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDirection_Name",
                schema: "recruitment",
                table: "CustomerDirection",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Position_Name",
                schema: "recruitment",
                table: "Position",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StackItem_Name",
                schema: "recruitment",
                table: "StackItem",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkFormat_Name",
                schema: "recruitment",
                table: "WorkFormat",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Vacancy_Customer_CustomerId",
                schema: "recruitment",
                table: "Vacancy",
                column: "CustomerId",
                principalSchema: "recruitment",
                principalTable: "Customer",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Vacancy_Position_PositionId",
                schema: "recruitment",
                table: "Vacancy",
                column: "PositionId",
                principalSchema: "recruitment",
                principalTable: "Position",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Vacancy_StackItem_StackItemId",
                schema: "recruitment",
                table: "Vacancy",
                column: "StackItemId",
                principalSchema: "recruitment",
                principalTable: "StackItem",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Vacancy_WorkFormat_WorkFormatId",
                schema: "recruitment",
                table: "Vacancy",
                column: "WorkFormatId",
                principalSchema: "recruitment",
                principalTable: "WorkFormat",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vacancy_Customer_CustomerId",
                schema: "recruitment",
                table: "Vacancy");

            migrationBuilder.DropForeignKey(
                name: "FK_Vacancy_Position_PositionId",
                schema: "recruitment",
                table: "Vacancy");

            migrationBuilder.DropForeignKey(
                name: "FK_Vacancy_StackItem_StackItemId",
                schema: "recruitment",
                table: "Vacancy");

            migrationBuilder.DropForeignKey(
                name: "FK_Vacancy_WorkFormat_WorkFormatId",
                schema: "recruitment",
                table: "Vacancy");

            migrationBuilder.DropTable(
                name: "Customer",
                schema: "recruitment");

            migrationBuilder.DropTable(
                name: "Position",
                schema: "recruitment");

            migrationBuilder.DropTable(
                name: "StackItem",
                schema: "recruitment");

            migrationBuilder.DropTable(
                name: "WorkFormat",
                schema: "recruitment");

            migrationBuilder.DropTable(
                name: "CustomerDirection",
                schema: "recruitment");

            migrationBuilder.DropIndex(
                name: "IX_Vacancy_CustomerId",
                schema: "recruitment",
                table: "Vacancy");

            migrationBuilder.DropIndex(
                name: "IX_Vacancy_PositionId",
                schema: "recruitment",
                table: "Vacancy");

            migrationBuilder.DropIndex(
                name: "IX_Vacancy_StackItemId",
                schema: "recruitment",
                table: "Vacancy");

            migrationBuilder.DropIndex(
                name: "IX_Vacancy_WorkFormatId",
                schema: "recruitment",
                table: "Vacancy");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                schema: "recruitment",
                table: "Vacancy");

            migrationBuilder.DropColumn(
                name: "PositionId",
                schema: "recruitment",
                table: "Vacancy");

            migrationBuilder.DropColumn(
                name: "StackItemId",
                schema: "recruitment",
                table: "Vacancy");

            migrationBuilder.DropColumn(
                name: "WorkFormatId",
                schema: "recruitment",
                table: "Vacancy");
        }
    }
}
