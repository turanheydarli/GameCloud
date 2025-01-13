using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameCloud.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FunctionConfigUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:hstore", ",,");

            migrationBuilder.AddColumn<Dictionary<string, string>>(
                name: "Headers",
                schema: "gc",
                table: "FunctionConfigs",
                type: "hstore",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxRetries",
                schema: "gc",
                table: "FunctionConfigs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "Timeout",
                schema: "gc",
                table: "FunctionConfigs",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Headers",
                schema: "gc",
                table: "FunctionConfigs");

            migrationBuilder.DropColumn(
                name: "MaxRetries",
                schema: "gc",
                table: "FunctionConfigs");

            migrationBuilder.DropColumn(
                name: "Timeout",
                schema: "gc",
                table: "FunctionConfigs");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:hstore", ",,");
        }
    }
}
