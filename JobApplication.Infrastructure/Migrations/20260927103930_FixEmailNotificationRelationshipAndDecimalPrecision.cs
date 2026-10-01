using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobApplication.Infrastructure.Migrations
{
    public partial class FixEmailNotificationRelationshipAndDecimalPrecision : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmailNotification_Notification_NotificationId1",
                table: "EmailNotification");

            migrationBuilder.DropIndex(
                name: "IX_EmailNotification_NotificationId1",
                table: "EmailNotification");

            // Preserve the old incompatible Guid value for manual review. The
            // bigint shadow FK contains the actual Notification relationship.
            migrationBuilder.RenameColumn(
                name: "NotificationId",
                table: "EmailNotification",
                newName: "LegacyNotificationId");

            migrationBuilder.RenameColumn(
                name: "NotificationId1",
                table: "EmailNotification",
                newName: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailNotification_NotificationId",
                table: "EmailNotification",
                column: "NotificationId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailNotification_Notification_NotificationId",
                table: "EmailNotification",
                column: "NotificationId",
                principalTable: "Notification",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmailNotification_Notification_NotificationId",
                table: "EmailNotification");

            migrationBuilder.DropIndex(
                name: "IX_EmailNotification_NotificationId",
                table: "EmailNotification");

            migrationBuilder.RenameColumn(
                name: "NotificationId",
                table: "EmailNotification",
                newName: "NotificationId1");

            migrationBuilder.RenameColumn(
                name: "LegacyNotificationId",
                table: "EmailNotification",
                newName: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailNotification_NotificationId1",
                table: "EmailNotification",
                column: "NotificationId1");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailNotification_Notification_NotificationId1",
                table: "EmailNotification",
                column: "NotificationId1",
                principalTable: "Notification",
                principalColumn: "Id");
        }
    }
}
