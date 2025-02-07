using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameCloud.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PlayerAndSessionsUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomId",
                schema: "gc",
                table: "Player",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeviceId",
                schema: "gc",
                table: "Player",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginAt",
                schema: "gc",
                table: "Player",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<JsonDocument>(
                name: "Metadata",
                schema: "gc",
                table: "Player",
                type: "jsonb",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomId",
                schema: "gc",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "DeviceId",
                schema: "gc",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "LastLoginAt",
                schema: "gc",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "Metadata",
                schema: "gc",
                table: "Player");
        }
    }
}
