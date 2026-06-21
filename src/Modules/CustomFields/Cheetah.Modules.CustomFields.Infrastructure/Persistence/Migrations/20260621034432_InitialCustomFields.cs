using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cheetah.Modules.CustomFields.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCustomFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "custom_fields");

            migrationBuilder.CreateTable(
                name: "CustomFieldDefinitions",
                schema: "custom_fields",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DataType = table.Column<int>(type: "integer", nullable: false),
                    Required = table.Column<bool>(type: "boolean", nullable: false),
                    Options = table.Column<string>(type: "jsonb", nullable: true),
                    ValidationRulesJson = table.Column<string>(type: "jsonb", nullable: true),
                    VisibilityRule = table.Column<string>(type: "jsonb", nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomFieldDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomFieldEntityTypes",
                schema: "custom_fields",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    OwnerService = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IdType = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomFieldEntityTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomFieldValueSets",
                schema: "custom_fields",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ValuesJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomFieldValueSets", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomFieldDefinitions_TenantId_EntityType",
                schema: "custom_fields",
                table: "CustomFieldDefinitions",
                columns: new[] { "TenantId", "EntityType" });

            migrationBuilder.CreateIndex(
                name: "IX_CustomFieldDefinitions_TenantId_EntityType_Key",
                schema: "custom_fields",
                table: "CustomFieldDefinitions",
                columns: new[] { "TenantId", "EntityType", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomFieldEntityTypes_Key",
                schema: "custom_fields",
                table: "CustomFieldEntityTypes",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomFieldEntityTypes_OwnerService",
                schema: "custom_fields",
                table: "CustomFieldEntityTypes",
                column: "OwnerService");

            migrationBuilder.CreateIndex(
                name: "IX_CustomFieldValueSets_TenantId_EntityType_EntityId",
                schema: "custom_fields",
                table: "CustomFieldValueSets",
                columns: new[] { "TenantId", "EntityType", "EntityId" },
                unique: true);

            // GIN-индекс по jsonb-значениям — задел под фильтрацию по значениям (jsonb @>), план §1.2-1.
            migrationBuilder.Sql(
                "CREATE INDEX IF NOT EXISTS \"IX_CustomFieldValueSets_Values_Gin\" " +
                "ON custom_fields.\"CustomFieldValueSets\" USING gin (\"ValuesJson\" jsonb_path_ops);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomFieldDefinitions",
                schema: "custom_fields");

            migrationBuilder.DropTable(
                name: "CustomFieldEntityTypes",
                schema: "custom_fields");

            migrationBuilder.DropTable(
                name: "CustomFieldValueSets",
                schema: "custom_fields");
        }
    }
}
