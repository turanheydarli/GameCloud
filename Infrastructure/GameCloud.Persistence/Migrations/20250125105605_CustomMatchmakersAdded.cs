using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameCloud.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CustomMatchmakersAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MatchmakerFunctionId",
                schema: "gc",
                table: "MatchmakingQueues",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "UseCustomMatchmaker",
                schema: "gc",
                table: "MatchmakingQueues",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_MatchmakingQueues_MatchmakerFunctionId",
                schema: "gc",
                table: "MatchmakingQueues",
                column: "MatchmakerFunctionId");

            migrationBuilder.AddForeignKey(
                name: "FK_MatchmakingQueues_FunctionConfigs_MatchmakerFunctionId",
                schema: "gc",
                table: "MatchmakingQueues",
                column: "MatchmakerFunctionId",
                principalSchema: "gc",
                principalTable: "FunctionConfigs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MatchmakingQueues_FunctionConfigs_MatchmakerFunctionId",
                schema: "gc",
                table: "MatchmakingQueues");

            migrationBuilder.DropIndex(
                name: "IX_MatchmakingQueues_MatchmakerFunctionId",
                schema: "gc",
                table: "MatchmakingQueues");

            migrationBuilder.DropColumn(
                name: "MatchmakerFunctionId",
                schema: "gc",
                table: "MatchmakingQueues");

            migrationBuilder.DropColumn(
                name: "UseCustomMatchmaker",
                schema: "gc",
                table: "MatchmakingQueues");
        }
    }
}
