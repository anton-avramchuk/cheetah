using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cheetah.Modules.Workflow.Default.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "workflow");

            migrationBuilder.CreateTable(
                name: "AutomationRules",
                schema: "workflow",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    OwnerService = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ConditionExpression = table.Column<string>(type: "jsonb", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomationRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AutomationRuns",
                schema: "workflow",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PayloadSnapshot = table.Column<string>(type: "jsonb", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomationRuns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RuleActions",
                schema: "workflow",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    ActionType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Parameters = table.Column<string>(type: "jsonb", nullable: false),
                    FailureMode = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RuleActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RuleActions_AutomationRules_RuleId",
                        column: x => x.RuleId,
                        principalSchema: "workflow",
                        principalTable: "AutomationRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TriggerBindings",
                schema: "workflow",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    TriggerType = table.Column<int>(type: "integer", nullable: false),
                    TriggerKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Parameters = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TriggerBindings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TriggerBindings_AutomationRules_RuleId",
                        column: x => x.RuleId,
                        principalSchema: "workflow",
                        principalTable: "AutomationRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AutomationRunSteps",
                schema: "workflow",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RunId = table.Column<Guid>(type: "uuid", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    ActionType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Error = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomationRunSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AutomationRunSteps_AutomationRuns_RunId",
                        column: x => x.RunId,
                        principalSchema: "workflow",
                        principalTable: "AutomationRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AutomationRules_IsActive",
                schema: "workflow",
                table: "AutomationRules",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationRules_OwnerService",
                schema: "workflow",
                table: "AutomationRules",
                column: "OwnerService");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationRuns_RuleId_CreatedAt",
                schema: "workflow",
                table: "AutomationRuns",
                columns: new[] { "RuleId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AutomationRuns_RuleId_EventId",
                schema: "workflow",
                table: "AutomationRuns",
                columns: new[] { "RuleId", "EventId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AutomationRuns_Status",
                schema: "workflow",
                table: "AutomationRuns",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationRunSteps_RunId",
                schema: "workflow",
                table: "AutomationRunSteps",
                column: "RunId");

            migrationBuilder.CreateIndex(
                name: "IX_RuleActions_RuleId_Order",
                schema: "workflow",
                table: "RuleActions",
                columns: new[] { "RuleId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_TriggerBindings_RuleId",
                schema: "workflow",
                table: "TriggerBindings",
                column: "RuleId");

            migrationBuilder.CreateIndex(
                name: "IX_TriggerBindings_TriggerType_TriggerKey",
                schema: "workflow",
                table: "TriggerBindings",
                columns: new[] { "TriggerType", "TriggerKey" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AutomationRunSteps",
                schema: "workflow");

            migrationBuilder.DropTable(
                name: "RuleActions",
                schema: "workflow");

            migrationBuilder.DropTable(
                name: "TriggerBindings",
                schema: "workflow");

            migrationBuilder.DropTable(
                name: "AutomationRuns",
                schema: "workflow");

            migrationBuilder.DropTable(
                name: "AutomationRules",
                schema: "workflow");
        }
    }
}
