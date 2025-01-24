using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameCloud.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraintToUsernameAndApiKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                schema: "gc",
                table: "GameKeys",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Player_Username",
                schema: "gc",
                table: "Player",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameKeys_ApiKey",
                schema: "gc",
                table: "GameKeys",
                column: "ApiKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Player_Username",
                schema: "gc",
                table: "Player");

            migrationBuilder.DropIndex(
                name: "IX_GameKeys_ApiKey",
                schema: "gc",
                table: "GameKeys");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                schema: "gc",
                table: "GameKeys");
        }
    }
}
