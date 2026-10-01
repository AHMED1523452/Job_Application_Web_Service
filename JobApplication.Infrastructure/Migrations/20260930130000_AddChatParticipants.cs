using JobApplication.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobApplication.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260930130000_AddChatParticipants")]
public partial class AddChatParticipants : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ChatParticipants",
            columns: table => new
            {
                ChatId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                JoinedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                LastReadAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ChatParticipants", x => new { x.ChatId, x.UserId });
                table.ForeignKey(
                    name: "FK_ChatParticipants_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ChatParticipants_Chat_ChatId",
                    column: x => x.ChatId,
                    principalTable: "Chat",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ChatParticipants_UserId",
            table: "ChatParticipants",
            column: "UserId");

        // Backfill both participants for existing candidate/recruiter conversations.
        migrationBuilder.Sql("""
            INSERT INTO [ChatParticipants] ([ChatId], [UserId], [JoinedAtUtc], [LastReadAtUtc])
            SELECT c.[Id], cp.[UserId], c.[CreatedAt], NULL
            FROM [Chat] AS c
            INNER JOIN [candidates] AS cp ON cp.[Id] = c.[CandidateProfileId]
            UNION ALL
            SELECT c.[Id], hr.[UserId], c.[CreatedAt], NULL
            FROM [Chat] AS c
            INNER JOIN [HrProfiles] AS hr ON hr.[Id] = c.[HrProfileId]
            WHERE hr.[UserId] <> (
                SELECT cp.[UserId]
                FROM [candidates] AS cp
                WHERE cp.[Id] = c.[CandidateProfileId]);
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ChatParticipants");
    }
}
