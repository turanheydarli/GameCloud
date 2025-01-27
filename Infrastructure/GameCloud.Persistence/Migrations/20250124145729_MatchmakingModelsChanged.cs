using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameCloud.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MatchmakingModelsChanged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CustomProperties",
                schema: "gc",
                table: "MatchTickets",
                newName: "Properties");

            migrationBuilder.RenameColumn(
                name: "MatchData",
                schema: "gc",
                table: "Matches",
                newName: "TurnHistory");

            migrationBuilder.AddColumn<JsonDocument>(
                name: "MatchCriteria",
                schema: "gc",
                table: "MatchTickets",
                type: "jsonb",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "gc",
                table: "MatchmakingQueues",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                schema: "gc",
                table: "MatchmakingQueues",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "MatchTimeout",
                schema: "gc",
                table: "MatchmakingQueues",
                type: "interval",
                nullable: true);

            migrationBuilder.AddColumn<JsonDocument>(
                name: "Rules",
                schema: "gc",
                table: "MatchmakingQueues",
                type: "jsonb",
                nullable: false);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "TurnTimeout",
                schema: "gc",
                table: "MatchmakingQueues",
                type: "interval",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                schema: "gc",
                table: "Matches",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CurrentPlayerId",
                schema: "gc",
                table: "Matches",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastActionAt",
                schema: "gc",
                table: "Matches",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<JsonDocument>(
                name: "MatchState",
                schema: "gc",
                table: "Matches",
                type: "jsonb",
                nullable: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextActionDeadline",
                schema: "gc",
                table: "Matches",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<JsonDocument>(
                name: "PlayerStates",
                schema: "gc",
                table: "Matches",
                type: "jsonb",
                nullable: false);

            migrationBuilder.CreateTable(
                name: "MatchAction",
                schema: "gc",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActionType = table.Column<string>(type: "text", nullable: false),
                    ActionData = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchAction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchAction_Matches_MatchId",
                        column: x => x.MatchId,
                        principalSchema: "gc",
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MatchAction_Player_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "gc",
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MatchAction_MatchId",
                schema: "gc",
                table: "MatchAction",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchAction_PlayerId",
                schema: "gc",
                table: "MatchAction",
                column: "PlayerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MatchAction",
                schema: "gc");

            migrationBuilder.DropColumn(
                name: "MatchCriteria",
                schema: "gc",
                table: "MatchTickets");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "gc",
                table: "MatchmakingQueues");

            migrationBuilder.DropColumn(
                name: "IsEnabled",
                schema: "gc",
                table: "MatchmakingQueues");

            migrationBuilder.DropColumn(
                name: "MatchTimeout",
                schema: "gc",
                table: "MatchmakingQueues");

            migrationBuilder.DropColumn(
                name: "Rules",
                schema: "gc",
                table: "MatchmakingQueues");

            migrationBuilder.DropColumn(
                name: "TurnTimeout",
                schema: "gc",
                table: "MatchmakingQueues");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                schema: "gc",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "CurrentPlayerId",
                schema: "gc",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "LastActionAt",
                schema: "gc",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "MatchState",
                schema: "gc",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "NextActionDeadline",
                schema: "gc",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "PlayerStates",
                schema: "gc",
                table: "Matches");

            migrationBuilder.RenameColumn(
                name: "Properties",
                schema: "gc",
                table: "MatchTickets",
                newName: "CustomProperties");

            migrationBuilder.RenameColumn(
                name: "TurnHistory",
                schema: "gc",
                table: "Matches",
                newName: "MatchData");
        }
    }
}
