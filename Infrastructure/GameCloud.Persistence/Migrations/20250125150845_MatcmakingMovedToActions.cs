using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameCloud.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MatcmakingMovedToActions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ErrorCode",
                schema: "gc",
                table: "ActionLogs");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "gc",
                table: "ActionLogs");

            migrationBuilder.AddColumn<bool>(
                name: "IsSuccess",
                schema: "gc",
                table: "ActionLogs",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSuccess",
                schema: "gc",
                table: "ActionLogs");

            migrationBuilder.AddColumn<string>(
                name: "ErrorCode",
                schema: "gc",
                table: "ActionLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "gc",
                table: "ActionLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
