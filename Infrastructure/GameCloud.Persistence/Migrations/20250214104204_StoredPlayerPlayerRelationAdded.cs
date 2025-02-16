using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameCloud.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class StoredPlayerPlayerRelationAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PlayerId",
                schema: "gc",
                table: "StoredPlayers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

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

            migrationBuilder.AddForeignKey(
                name: "FK_StoredPlayers_Player_PlayerId",
                schema: "gc",
                table: "StoredPlayers",
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
                name: "FK_StoredPlayers_Player_Id",
                schema: "gc",
                table: "StoredPlayers");

            migrationBuilder.DropForeignKey(
                name: "FK_StoredPlayers_Player_PlayerId",
                schema: "gc",
                table: "StoredPlayers");

            migrationBuilder.DropIndex(
                name: "IX_StoredPlayers_PlayerId",
                schema: "gc",
                table: "StoredPlayers");

            migrationBuilder.DropColumn(
                name: "PlayerId",
                schema: "gc",
                table: "StoredPlayers");
        }
    }
}
