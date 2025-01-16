using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameCloud.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ActionLogsNavigationPropertyAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ActionLogs_FunctionId",
                schema: "gc",
                table: "ActionLogs",
                column: "FunctionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ActionLogs_FunctionConfigs_FunctionId",
                schema: "gc",
                table: "ActionLogs",
                column: "FunctionId",
                principalSchema: "gc",
                principalTable: "FunctionConfigs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActionLogs_FunctionConfigs_FunctionId",
                schema: "gc",
                table: "ActionLogs");

            migrationBuilder.DropIndex(
                name: "IX_ActionLogs_FunctionId",
                schema: "gc",
                table: "ActionLogs");
        }
    }
}
