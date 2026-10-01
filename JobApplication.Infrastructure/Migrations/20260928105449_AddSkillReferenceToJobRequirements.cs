using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSkillReferenceToJobRequirements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SkillId",
                table: "JobRequirements",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobRequirements_SkillId",
                table: "JobRequirements",
                column: "SkillId");

            migrationBuilder.AddForeignKey(
                name: "FK_JobRequirements_Skills_SkillId",
                table: "JobRequirements",
                column: "SkillId",
                principalTable: "Skills",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobRequirements_Skills_SkillId",
                table: "JobRequirements");

            migrationBuilder.DropIndex(
                name: "IX_JobRequirements_SkillId",
                table: "JobRequirements");

            migrationBuilder.DropColumn(
                name: "SkillId",
                table: "JobRequirements");
        }
    }
}
