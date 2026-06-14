using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bugtracker_back.Migrations
{
    /// <inheritdoc />
    public partial class CascadeFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Bugs",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Bugs_OwnerId",
                table: "Bugs",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bugs_AspNetUsers_OwnerId",
                table: "Bugs",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bugs_AspNetUsers_OwnerId",
                table: "Bugs");

            migrationBuilder.DropIndex(
                name: "IX_Bugs_OwnerId",
                table: "Bugs");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Bugs");
        }
    }
}
