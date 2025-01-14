using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameCloud.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PlayerActivitesAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FunctionConfigs_Games_GameId1",
                schema: "gc",
                table: "FunctionConfigs");

            migrationBuilder.DropForeignKey(
                name: "FK_GameKeys_Games_GameId1",
                schema: "gc",
                table: "GameKeys");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
        }
    }
}
