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
                name: "TaskPriority",
                schema: "vacancytasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskPriority", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaskState",
                schema: "vacancytasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskState", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VacancyTask",
                schema: "vacancytasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    VacancyId = table.Column<Guid>(type: "uuid", nullable: false),
                    StateId = table.Column<Guid>(type: "uuid", nullable: false),
                    PriorityId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssigneeId = table.Column<Guid>(type: "uuid", nullable: true),
                    DueDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VacancyTask", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VacancyTask_TaskPriority_PriorityId",
                        column: x => x.PriorityId,
                        principalSchema: "vacancytasks",
                        principalTable: "TaskPriority",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_VacancyTask_TaskState_StateId",
                        column: x => x.StateId,
                        principalSchema: "vacancytasks",
                        principalTable: "TaskState",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskPriority_Name",
                schema: "vacancytasks",
                table: "TaskPriority",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskState_Name",
                schema: "vacancytasks",
                table: "TaskState",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VacancyTask_AssigneeId",
                schema: "vacancytasks",
                table: "VacancyTask",
                column: "AssigneeId");

            migrationBuilder.CreateIndex(
                name: "IX_VacancyTask_PriorityId",
                schema: "vacancytasks",
                table: "VacancyTask",
                column: "PriorityId");

            migrationBuilder.CreateIndex(
                name: "IX_VacancyTask_StateId",
                schema: "vacancytasks",
                table: "VacancyTask",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "IX_VacancyTask_VacancyId",
                schema: "vacancytasks",
                table: "VacancyTask",
                column: "VacancyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VacancyTask",
                schema: "vacancytasks");

            migrationBuilder.DropTable(
                name: "TaskPriority",
                schema: "vacancytasks");

            migrationBuilder.DropTable(
                name: "TaskState",
                schema: "vacancytasks");
        }
    }
}
