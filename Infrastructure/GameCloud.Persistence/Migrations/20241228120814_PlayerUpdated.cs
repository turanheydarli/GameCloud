using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameCloud.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PlayerUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Player_Session_SessionId",
                schema: "gc",
                table: "Player");

            migrationBuilder.DropIndex(
                name: "IX_Player_SessionId",
                schema: "gc",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "SessionId",
                schema: "gc",
                table: "Player");

            migrationBuilder.AlterColumn<Guid>(
                name: "GameId",
                schema: "gc",
                table: "Player",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "GameId",
                schema: "gc",
                table: "Player",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "SessionId",
                schema: "gc",
                table: "Player",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Player_SessionId",
                schema: "gc",
                table: "Player",
                column: "SessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Player_Session_SessionId",
                schema: "gc",
                table: "Player",
                column: "SessionId",
                principalSchema: "gc",
                principalTable: "Session",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
