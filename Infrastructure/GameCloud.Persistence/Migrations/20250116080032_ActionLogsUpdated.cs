using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameCloud.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ActionLogsUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Version",
                schema: "gc",
                table: "FunctionConfigs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                schema: "gc",
                table: "ActionLogs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ErrorCode",
                schema: "gc",
                table: "ActionLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                schema: "gc",
                table: "ActionLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ExecutionTimeMs",
                schema: "gc",
                table: "ActionLogs",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<Dictionary<string, string>>(
                name: "Metadata",
                schema: "gc",
                table: "ActionLogs",
                type: "hstore",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PayloadSizeBytes",
                schema: "gc",
                table: "ActionLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ResultSizeBytes",
                schema: "gc",
                table: "ActionLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RetryCount",
                schema: "gc",
                table: "ActionLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAt",
                schema: "gc",
                table: "ActionLogs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "gc",
                table: "ActionLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "TotalLatencyMs",
                schema: "gc",
                table: "ActionLogs",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Version",
                schema: "gc",
                table: "FunctionConfigs");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                schema: "gc",
                table: "ActionLogs");

            migrationBuilder.DropColumn(
                name: "ErrorCode",
                schema: "gc",
                table: "ActionLogs");

            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                schema: "gc",
                table: "ActionLogs");

            migrationBuilder.DropColumn(
                name: "ExecutionTimeMs",
                schema: "gc",
                table: "ActionLogs");

            migrationBuilder.DropColumn(
                name: "Metadata",
                schema: "gc",
                table: "ActionLogs");

            migrationBuilder.DropColumn(
                name: "PayloadSizeBytes",
                schema: "gc",
                table: "ActionLogs");

            migrationBuilder.DropColumn(
                name: "ResultSizeBytes",
                schema: "gc",
                table: "ActionLogs");

            migrationBuilder.DropColumn(
                name: "RetryCount",
                schema: "gc",
                table: "ActionLogs");

            migrationBuilder.DropColumn(
                name: "StartedAt",
                schema: "gc",
                table: "ActionLogs");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "gc",
                table: "ActionLogs");

            migrationBuilder.DropColumn(
                name: "TotalLatencyMs",
                schema: "gc",
                table: "ActionLogs");
        }
    }
}
