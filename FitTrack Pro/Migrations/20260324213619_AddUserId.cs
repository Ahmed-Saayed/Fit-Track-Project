using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitTrack_Pro.Migrations
{
    /// <inheritdoc />
    public partial class AddUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserAccountId",
                table: "Trainers",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Trainers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserAccountId",
                table: "Members",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Members",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trainers_UserAccountId",
                table: "Trainers",
                column: "UserAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Members_UserAccountId",
                table: "Members",
                column: "UserAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Members_AspNetUsers_UserAccountId",
                table: "Members",
                column: "UserAccountId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Trainers_AspNetUsers_UserAccountId",
                table: "Trainers",
                column: "UserAccountId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Members_AspNetUsers_UserAccountId",
                table: "Members");

            migrationBuilder.DropForeignKey(
                name: "FK_Trainers_AspNetUsers_UserAccountId",
                table: "Trainers");

            migrationBuilder.DropIndex(
                name: "IX_Trainers_UserAccountId",
                table: "Trainers");

            migrationBuilder.DropIndex(
                name: "IX_Members_UserAccountId",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "UserAccountId",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "UserAccountId",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Members");
        }
    }
}
