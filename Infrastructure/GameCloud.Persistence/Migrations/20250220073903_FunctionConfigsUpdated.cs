using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameCloud.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FunctionConfigsUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EndFunctionId",
                schema: "gc",
                table: "MatchmakingQueues",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InitializeFunctionId",
                schema: "gc",
                table: "MatchmakingQueues",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LeaveFunctionId",
                schema: "gc",
                table: "MatchmakingQueues",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TransitionFunctionId",
                schema: "gc",
                table: "MatchmakingQueues",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MatchmakingQueues_EndFunctionId",
                schema: "gc",
                table: "MatchmakingQueues",
                column: "EndFunctionId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchmakingQueues_InitializeFunctionId",
                schema: "gc",
                table: "MatchmakingQueues",
                column: "InitializeFunctionId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchmakingQueues_LeaveFunctionId",
                schema: "gc",
                table: "MatchmakingQueues",
                column: "LeaveFunctionId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchmakingQueues_TransitionFunctionId",
                schema: "gc",
                table: "MatchmakingQueues",
                column: "TransitionFunctionId");

            migrationBuilder.AddForeignKey(
                name: "FK_MatchmakingQueues_FunctionConfigs_EndFunctionId",
                schema: "gc",
                table: "MatchmakingQueues",
                column: "EndFunctionId",
                principalSchema: "gc",
                principalTable: "FunctionConfigs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MatchmakingQueues_FunctionConfigs_InitializeFunctionId",
                schema: "gc",
                table: "MatchmakingQueues",
                column: "InitializeFunctionId",
                principalSchema: "gc",
                principalTable: "FunctionConfigs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MatchmakingQueues_FunctionConfigs_LeaveFunctionId",
                schema: "gc",
                table: "MatchmakingQueues",
                column: "LeaveFunctionId",
                principalSchema: "gc",
                principalTable: "FunctionConfigs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MatchmakingQueues_FunctionConfigs_TransitionFunctionId",
                schema: "gc",
                table: "MatchmakingQueues",
                column: "TransitionFunctionId",
                principalSchema: "gc",
                principalTable: "FunctionConfigs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MatchmakingQueues_FunctionConfigs_EndFunctionId",
                schema: "gc",
                table: "MatchmakingQueues");

            migrationBuilder.DropForeignKey(
                name: "FK_MatchmakingQueues_FunctionConfigs_InitializeFunctionId",
                schema: "gc",
                table: "MatchmakingQueues");

            migrationBuilder.DropForeignKey(
                name: "FK_MatchmakingQueues_FunctionConfigs_LeaveFunctionId",
                schema: "gc",
                table: "MatchmakingQueues");

            migrationBuilder.DropForeignKey(
                name: "FK_MatchmakingQueues_FunctionConfigs_TransitionFunctionId",
                schema: "gc",
                table: "MatchmakingQueues");

            migrationBuilder.DropIndex(
                name: "IX_MatchmakingQueues_EndFunctionId",
                schema: "gc",
                table: "MatchmakingQueues");

            migrationBuilder.DropIndex(
                name: "IX_MatchmakingQueues_InitializeFunctionId",
                schema: "gc",
                table: "MatchmakingQueues");

            migrationBuilder.DropIndex(
                name: "IX_MatchmakingQueues_LeaveFunctionId",
                schema: "gc",
                table: "MatchmakingQueues");

            migrationBuilder.DropIndex(
                name: "IX_MatchmakingQueues_TransitionFunctionId",
                schema: "gc",
                table: "MatchmakingQueues");

            migrationBuilder.DropColumn(
                name: "EndFunctionId",
                schema: "gc",
                table: "MatchmakingQueues");

            migrationBuilder.DropColumn(
                name: "InitializeFunctionId",
                schema: "gc",
                table: "MatchmakingQueues");

            migrationBuilder.DropColumn(
                name: "LeaveFunctionId",
                schema: "gc",
                table: "MatchmakingQueues");

            migrationBuilder.DropColumn(
                name: "TransitionFunctionId",
                schema: "gc",
                table: "MatchmakingQueues");
        }
    }
}
