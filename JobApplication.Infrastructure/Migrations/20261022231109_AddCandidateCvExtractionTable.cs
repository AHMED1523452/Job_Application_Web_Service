using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCandidateCvExtractionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CandidateCvExtractions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CandidateCvId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ExtractedText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateCvExtractions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CandidateCvExtractions_CandidateCVs_CandidateCvId",
                        column: x => x.CandidateCvId,
                        principalTable: "CandidateCVs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CandidateCvExtractions_CandidateCvId",
                table: "CandidateCvExtractions",
                column: "CandidateCvId",
                unique: true);

            //. Preserve already-extracted text by moving it into the new table before the old
            //. column is dropped. CVs that never produced text are recorded as Failed so the
            //. reason stays explicit instead of a silent null.
            migrationBuilder.Sql(@"
                INSERT INTO [CandidateCvExtractions] ([Id], [CandidateCvId], [Status], [ExtractedText], [ErrorMessage], [CreatedAtUtc], [CompletedAtUtc])
                SELECT NEWID(), cv.[Id],
                       CASE WHEN cv.[ExtractedText] IS NULL THEN 3 ELSE 2 END,
                       cv.[ExtractedText],
                       CASE WHEN cv.[ExtractedText] IS NULL THEN 'Extraction was never completed before the extraction table was introduced.' ELSE NULL END,
                       cv.[UploadedAtUtc],
                       cv.[ProcessedAtUtc]
                FROM [CandidateCVs] cv;
            ");

            migrationBuilder.DropColumn(
                name: "ExtractedText",
                table: "CandidateCVs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CandidateCvExtractions");

            migrationBuilder.AddColumn<string>(
                name: "ExtractedText",
                table: "CandidateCVs",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
