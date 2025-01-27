using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameCloud.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MatchActionsAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MatchAction_Matches_MatchId",
                schema: "gc",
                table: "MatchAction");

            migrationBuilder.DropForeignKey(
                name: "FK_MatchAction_Player_PlayerId",
                schema: "gc",
                table: "MatchAction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MatchAction",
                schema: "gc",
                table: "MatchAction");

            migrationBuilder.RenameTable(
                name: "MatchAction",
                schema: "gc",
                newName: "MatchActions",
                newSchema: "gc");

            migrationBuilder.RenameIndex(
                name: "IX_MatchAction_PlayerId",
                schema: "gc",
                table: "MatchActions",
                newName: "IX_MatchActions_PlayerId");

            migrationBuilder.RenameIndex(
                name: "IX_MatchAction_MatchId",
                schema: "gc",
                table: "MatchActions",
                newName: "IX_MatchActions_MatchId");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "MatchTimeout",
                schema: "gc",
                table: "Matches",
                type: "interval",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "TurnTimeout",
                schema: "gc",
                table: "Matches",
                type: "interval",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_MatchActions",
                schema: "gc",
                table: "MatchActions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MatchActions_Matches_MatchId",
                schema: "gc",
                table: "MatchActions",
                column: "MatchId",
                principalSchema: "gc",
                principalTable: "Matches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MatchActions_Player_PlayerId",
                schema: "gc",
                table: "MatchActions",
                column: "PlayerId",
                principalSchema: "gc",
                principalTable: "Player",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MatchActions_Matches_MatchId",
                schema: "gc",
                table: "MatchActions");

            migrationBuilder.DropForeignKey(
                name: "FK_MatchActions_Player_PlayerId",
                schema: "gc",
                table: "MatchActions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MatchActions",
                schema: "gc",
                table: "MatchActions");

            migrationBuilder.DropColumn(
                name: "MatchTimeout",
                schema: "gc",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "TurnTimeout",
                schema: "gc",
                table: "Matches");

            migrationBuilder.RenameTable(
                name: "MatchActions",
                schema: "gc",
                newName: "MatchAction",
                newSchema: "gc");

            migrationBuilder.RenameIndex(
                name: "IX_MatchActions_PlayerId",
                schema: "gc",
                table: "MatchAction",
                newName: "IX_MatchAction_PlayerId");

            migrationBuilder.RenameIndex(
                name: "IX_MatchActions_MatchId",
                schema: "gc",
                table: "MatchAction",
                newName: "IX_MatchAction_MatchId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MatchAction",
                schema: "gc",
                table: "MatchAction",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MatchAction_Matches_MatchId",
                schema: "gc",
                table: "MatchAction",
                column: "MatchId",
                principalSchema: "gc",
                principalTable: "Matches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MatchAction_Player_PlayerId",
                schema: "gc",
                table: "MatchAction",
                column: "PlayerId",
                principalSchema: "gc",
                principalTable: "Player",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
