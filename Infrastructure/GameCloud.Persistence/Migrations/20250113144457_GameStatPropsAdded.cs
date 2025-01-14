using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameCloud.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class GameStatPropsAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GameId1",
                schema: "gc",
                table: "GameKeys",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GameId1",
                schema: "gc",
                table: "FunctionConfigs",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GameActivities",
                schema: "gc",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GameId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Details = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameActivities_Games_GameId",
                        column: x => x.GameId,
                        principalSchema: "gc",
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Player_GameId",
                schema: "gc",
                table: "Player",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_GameKeys_GameId1",
                schema: "gc",
                table: "GameKeys",
                column: "GameId1");

            migrationBuilder.CreateIndex(
                name: "IX_FunctionConfigs_GameId1",
                schema: "gc",
                table: "FunctionConfigs",
                column: "GameId1");

            migrationBuilder.CreateIndex(
                name: "IX_GameActivities_GameId",
                schema: "gc",
                table: "GameActivities",
                column: "GameId");

            migrationBuilder.AddForeignKey(
                name: "FK_FunctionConfigs_Games_GameId1",
                schema: "gc",
                table: "FunctionConfigs",
                column: "GameId1",
                principalSchema: "gc",
                principalTable: "Games",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GameKeys_Games_GameId1",
                schema: "gc",
                table: "GameKeys",
                column: "GameId1",
                principalSchema: "gc",
                principalTable: "Games",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Player_Games_GameId",
                schema: "gc",
                table: "Player",
                column: "GameId",
                principalSchema: "gc",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FunctionConfigs_Games_GameId1",
                schema: "gc",
                table: "FunctionConfigs");

            migrationBuilder.DropForeignKey(
                name: "FK_GameKeys_Games_GameId1",
                schema: "gc",
                table: "GameKeys");

            migrationBuilder.DropForeignKey(
                name: "FK_Player_Games_GameId",
                schema: "gc",
                table: "Player");

            migrationBuilder.DropTable(
                name: "GameActivities",
                schema: "gc");

            migrationBuilder.DropIndex(
                name: "IX_Player_GameId",
                schema: "gc",
                table: "Player");

            migrationBuilder.DropIndex(
                name: "IX_GameKeys_GameId1",
                schema: "gc",
                table: "GameKeys");

            migrationBuilder.DropIndex(
                name: "IX_FunctionConfigs_GameId1",
                schema: "gc",
                table: "FunctionConfigs");

            migrationBuilder.DropColumn(
                name: "GameId1",
                schema: "gc",
                table: "GameKeys");

            migrationBuilder.DropColumn(
                name: "GameId1",
                schema: "gc",
                table: "FunctionConfigs");
        }
    }
}
