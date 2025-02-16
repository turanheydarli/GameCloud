using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameCloud.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class IsStoredPlayerPropertyAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StoredPlayers_Player_Id",
                schema: "gc",
                table: "StoredPlayers");

            migrationBuilder.DropIndex(
                name: "IX_StoredPlayers_PlayerId",
                schema: "gc",
                table: "StoredPlayers");

            migrationBuilder.AddColumn<bool>(
                name: "IsStoredPlayer",
                schema: "gc",
                table: "MatchTickets",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_StoredPlayers_PlayerId",
                schema: "gc",
                table: "StoredPlayers",
                column: "PlayerId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StoredPlayers_PlayerId",
                schema: "gc",
                table: "StoredPlayers");

            migrationBuilder.DropColumn(
                name: "IsStoredPlayer",
                schema: "gc",
                table: "MatchTickets");

            migrationBuilder.CreateIndex(
                name: "IX_StoredPlayers_PlayerId",
                schema: "gc",
                table: "StoredPlayers",
                column: "PlayerId");

            migrationBuilder.AddForeignKey(
                name: "FK_StoredPlayers_Player_Id",
                schema: "gc",
                table: "StoredPlayers",
                column: "Id",
                principalSchema: "gc",
                principalTable: "Player",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
