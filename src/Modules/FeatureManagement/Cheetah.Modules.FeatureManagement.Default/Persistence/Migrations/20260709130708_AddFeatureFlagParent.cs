using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cheetah.Modules.FeatureManagement.Default.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFeatureFlagParent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ParentKey",
                schema: "features",
                table: "FeatureFlags",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FeatureFlags_ParentKey",
                schema: "features",
                table: "FeatureFlags",
                column: "ParentKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FeatureFlags_ParentKey",
                schema: "features",
                table: "FeatureFlags");

            migrationBuilder.DropColumn(
                name: "ParentKey",
                schema: "features",
                table: "FeatureFlags");
        }
    }
}
