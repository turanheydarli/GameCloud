using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameCloud.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class QueueTypesAddedToMatchmaker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MatchmakingQueueId",
                schema: "gc",
                table: "MatchTickets",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QueueType",
                schema: "gc",
                table: "MatchmakingQueues",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "MatchmakingQueueId",
                schema: "gc",
                table: "MatchActions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MatchTickets_MatchmakingQueueId",
                schema: "gc",
                table: "MatchTickets",
                column: "MatchmakingQueueId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchActions_MatchmakingQueueId",
                schema: "gc",
                table: "MatchActions",
                column: "MatchmakingQueueId");

            migrationBuilder.AddForeignKey(
                name: "FK_MatchActions_MatchmakingQueues_MatchmakingQueueId",
                schema: "gc",
                table: "MatchActions",
                column: "MatchmakingQueueId",
                principalSchema: "gc",
                principalTable: "MatchmakingQueues",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MatchTickets_MatchmakingQueues_MatchmakingQueueId",
                schema: "gc",
                table: "MatchTickets",
                column: "MatchmakingQueueId",
                principalSchema: "gc",
                principalTable: "MatchmakingQueues",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MatchActions_MatchmakingQueues_MatchmakingQueueId",
                schema: "gc",
                table: "MatchActions");

            migrationBuilder.DropForeignKey(
                name: "FK_MatchTickets_MatchmakingQueues_MatchmakingQueueId",
                schema: "gc",
                table: "MatchTickets");

            migrationBuilder.DropIndex(
                name: "IX_MatchTickets_MatchmakingQueueId",
                schema: "gc",
                table: "MatchTickets");

            migrationBuilder.DropIndex(
                name: "IX_MatchActions_MatchmakingQueueId",
                schema: "gc",
                table: "MatchActions");

            migrationBuilder.DropColumn(
                name: "MatchmakingQueueId",
                schema: "gc",
                table: "MatchTickets");

            migrationBuilder.DropColumn(
                name: "QueueType",
                schema: "gc",
                table: "MatchmakingQueues");

            migrationBuilder.DropColumn(
                name: "MatchmakingQueueId",
                schema: "gc",
                table: "MatchActions");
        }
    }
}
