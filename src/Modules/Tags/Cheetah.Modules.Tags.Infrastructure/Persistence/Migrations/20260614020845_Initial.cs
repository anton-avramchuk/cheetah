using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cheetah.Modules.Tags.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "tags");

            migrationBuilder.CreateTable(
                name: "TaggableEntityTypes",
                schema: "tags",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    OwnerService = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    MaxTagsPerEntity = table.Column<int>(type: "integer", nullable: true),
                    AllowAdHocTags = table.Column<bool>(type: "boolean", nullable: false),
                    AllowedGroups = table.Column<List<string>>(type: "text[]", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaggableEntityTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                schema: "tags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Slug = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Color = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Group = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "tags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SyncHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TagAssignments",
                schema: "tags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TagId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TagAssignments_Users_AssignedBy",
                        column: x => x.AssignedBy,
                        principalSchema: "tags",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TagAssignments_AssignedBy",
                schema: "tags",
                table: "TagAssignments",
                column: "AssignedBy");

            migrationBuilder.CreateIndex(
                name: "IX_TagAssignments_EntityType_EntityId_TagId",
                schema: "tags",
                table: "TagAssignments",
                columns: new[] { "EntityType", "EntityId", "TagId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TagAssignments_EntityType_TagId",
                schema: "tags",
                table: "TagAssignments",
                columns: new[] { "EntityType", "TagId" });

            migrationBuilder.CreateIndex(
                name: "IX_TaggableEntityTypes_OwnerService",
                schema: "tags",
                table: "TaggableEntityTypes",
                column: "OwnerService");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Group",
                schema: "tags",
                table: "Tags",
                column: "Group");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Name",
                schema: "tags",
                table: "Tags",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TagAssignments",
                schema: "tags");

            migrationBuilder.DropTable(
                name: "TaggableEntityTypes",
                schema: "tags");

            migrationBuilder.DropTable(
                name: "Tags",
                schema: "tags");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "tags");
        }
    }
}
