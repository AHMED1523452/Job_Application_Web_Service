using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace JobApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddingSkillCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SkillCategory",
                table: "Skills",
                newName: "Description");

            migrationBuilder.AddColumn<long>(
                name: "SkillCategoryId",
                table: "Skills",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SkillCategories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillCategories", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "SkillCategories",
                columns: new[] { "Id", "Description", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1L, "Languages used to write software (e.g., C#, Java, Python)", true, "Programming Languages" },
                    { 2L, "Server-side development, APIs, and business logic", true, "Backend Development" },
                    { 3L, "Client-side UI/UX and web technologies", true, "Frontend Development" },
                    { 4L, "Relational and NoSQL databases and data modeling", true, "Database" },
                    { 5L, "Cloud platforms and services (e.g., Azure, AWS)", true, "Cloud" },
                    { 6L, "CI/CD, automation, and infrastructure as code", true, "DevOps" },
                    { 7L, "Unit, integration, and automated testing practices", true, "Testing" },
                    { 8L, "Application and infrastructure security practices", true, "Security" },
                    { 9L, "Machine learning, data science and AI-related skills", true, "AI / Machine Learning" },
                    { 10L, "Communication, teamwork and other interpersonal skills", true, "Soft Skills" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Skills_SkillCategoryId",
                table: "Skills",
                column: "SkillCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Skills_SkillCategories_SkillCategoryId",
                table: "Skills",
                column: "SkillCategoryId",
                principalTable: "SkillCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Skills_SkillCategories_SkillCategoryId",
                table: "Skills");

            migrationBuilder.DropTable(
                name: "SkillCategories");

            migrationBuilder.DropIndex(
                name: "IX_Skills_SkillCategoryId",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "SkillCategoryId",
                table: "Skills");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Skills",
                newName: "SkillCategory");
        }
    }
}
