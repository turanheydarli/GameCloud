using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameCloud.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ActionLogUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Parameters",
                schema: "gc",
                table: "ActionLogs");

            migrationBuilder.AlterColumn<JsonDocument>(
                name: "Result",
                schema: "gc",
                table: "ActionLogs",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(JsonDocument),
                oldType: "jsonb");

            migrationBuilder.AddColumn<JsonDocument>(
                name: "Payload",
                schema: "gc",
                table: "ActionLogs",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Payload",
                schema: "gc",
                table: "ActionLogs");

            migrationBuilder.AlterColumn<JsonDocument>(
                name: "Result",
                schema: "gc",
                table: "ActionLogs",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(JsonDocument),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AddColumn<JsonDocument>(
                name: "Parameters",
                schema: "gc",
                table: "ActionLogs",
                type: "jsonb",
                nullable: false);
        }
    }
}
