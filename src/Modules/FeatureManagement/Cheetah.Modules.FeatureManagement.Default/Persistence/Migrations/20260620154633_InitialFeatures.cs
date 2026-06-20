using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cheetah.Modules.FeatureManagement.Default.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "features");

            migrationBuilder.CreateTable(
                name: "FeatureFlags",
                schema: "features",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    OwnerService = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    ValueType = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeatureFlags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FeatureVariants",
                schema: "features",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FlagId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true),
                    Weight = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeatureVariants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeatureVariants_FeatureFlags_FlagId",
                        column: x => x.FlagId,
                        principalSchema: "features",
                        principalTable: "FeatureFlags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TargetingRules",
                schema: "features",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FlagId = table.Column<Guid>(type: "uuid", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    FilterName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ParametersJson = table.Column<string>(type: "jsonb", nullable: false),
                    ResultVariant = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Negate = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TargetingRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TargetingRules_FeatureFlags_FlagId",
                        column: x => x.FlagId,
                        principalSchema: "features",
                        principalTable: "FeatureFlags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantOverrides",
                schema: "features",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FlagId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    RulesJson = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantOverrides", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantOverrides_FeatureFlags_FlagId",
                        column: x => x.FlagId,
                        principalSchema: "features",
                        principalTable: "FeatureFlags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FeatureFlags_Key",
                schema: "features",
                table: "FeatureFlags",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FeatureFlags_OwnerService",
                schema: "features",
                table: "FeatureFlags",
                column: "OwnerService");

            migrationBuilder.CreateIndex(
                name: "IX_FeatureVariants_FlagId",
                schema: "features",
                table: "FeatureVariants",
                column: "FlagId");

            migrationBuilder.CreateIndex(
                name: "IX_TargetingRules_FlagId_Order",
                schema: "features",
                table: "TargetingRules",
                columns: new[] { "FlagId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_TenantOverrides_FlagId_TenantId",
                schema: "features",
                table: "TenantOverrides",
                columns: new[] { "FlagId", "TenantId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeatureVariants",
                schema: "features");

            migrationBuilder.DropTable(
                name: "TargetingRules",
                schema: "features");

            migrationBuilder.DropTable(
                name: "TenantOverrides",
                schema: "features");

            migrationBuilder.DropTable(
                name: "FeatureFlags",
                schema: "features");
        }
    }
}
