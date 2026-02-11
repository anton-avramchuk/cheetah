using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crm.Candidates.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "candidates");

            migrationBuilder.CreateTable(
                name: "Candidate",
                schema: "candidates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    LastName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    City = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CurrentPosition = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CurrentCompany = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SalaryExpectation = table.Column<decimal>(type: "numeric", nullable: true),
                    About = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Candidate", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CandidateSource",
                schema: "candidates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateSource", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CandidateStage",
                schema: "candidates",
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
                    table.PrimaryKey("PK_CandidateStage", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CandidateComment",
                schema: "candidates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateComment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CandidateComment_Candidate_CandidateId",
                        column: x => x.CandidateId,
                        principalSchema: "candidates",
                        principalTable: "Candidate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CandidateExternalProfile",
                schema: "candidates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    ExternalId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateExternalProfile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CandidateExternalProfile_CandidateSource_SourceId",
                        column: x => x.SourceId,
                        principalSchema: "candidates",
                        principalTable: "CandidateSource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CandidateExternalProfile_Candidate_CandidateId",
                        column: x => x.CandidateId,
                        principalSchema: "candidates",
                        principalTable: "Candidate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CandidateApplication",
                schema: "candidates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateId = table.Column<Guid>(type: "uuid", nullable: false),
                    VacancyId = table.Column<Guid>(type: "uuid", nullable: false),
                    StageId = table.Column<Guid>(type: "uuid", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateApplication", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CandidateApplication_CandidateStage_StageId",
                        column: x => x.StageId,
                        principalSchema: "candidates",
                        principalTable: "CandidateStage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CandidateApplication_Candidate_CandidateId",
                        column: x => x.CandidateId,
                        principalSchema: "candidates",
                        principalTable: "Candidate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CandidateApplication_CandidateId_VacancyId",
                schema: "candidates",
                table: "CandidateApplication",
                columns: new[] { "CandidateId", "VacancyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CandidateApplication_StageId",
                schema: "candidates",
                table: "CandidateApplication",
                column: "StageId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateApplication_VacancyId",
                schema: "candidates",
                table: "CandidateApplication",
                column: "VacancyId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateComment_CandidateId",
                schema: "candidates",
                table: "CandidateComment",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateExternalProfile_CandidateId",
                schema: "candidates",
                table: "CandidateExternalProfile",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateExternalProfile_SourceId_ExternalId",
                schema: "candidates",
                table: "CandidateExternalProfile",
                columns: new[] { "SourceId", "ExternalId" },
                unique: true,
                filter: "\"ExternalId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateSource_Name",
                schema: "candidates",
                table: "CandidateSource",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CandidateStage_Name",
                schema: "candidates",
                table: "CandidateStage",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CandidateApplication",
                schema: "candidates");

            migrationBuilder.DropTable(
                name: "CandidateComment",
                schema: "candidates");

            migrationBuilder.DropTable(
                name: "CandidateExternalProfile",
                schema: "candidates");

            migrationBuilder.DropTable(
                name: "CandidateStage",
                schema: "candidates");

            migrationBuilder.DropTable(
                name: "CandidateSource",
                schema: "candidates");

            migrationBuilder.DropTable(
                name: "Candidate",
                schema: "candidates");
        }
    }
}
