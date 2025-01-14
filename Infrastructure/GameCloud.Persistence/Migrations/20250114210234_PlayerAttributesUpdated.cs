using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameCloud.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PlayerAttributesUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Attributes",
                schema: "gc",
                table: "Player");

            migrationBuilder.RenameColumn(
                name: "PlayerId",
                schema: "gc",
                table: "Player",
                newName: "Username");

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                schema: "gc",
                table: "Player",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "PlayerAttribute",
                schema: "gc",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    Collection = table.Column<string>(type: "text", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PermissionRead = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    PermissionWrite = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerAttribute", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerAttribute_Player_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "gc",
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerAttribute_PlayerId",
                schema: "gc",
                table: "PlayerAttribute",
                column: "PlayerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerAttribute",
                schema: "gc");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                schema: "gc",
                table: "Player");

            migrationBuilder.RenameColumn(
                name: "Username",
                schema: "gc",
                table: "Player",
                newName: "PlayerId");

            migrationBuilder.AddColumn<JsonDocument>(
                name: "Attributes",
                schema: "gc",
                table: "Player",
                type: "jsonb",
                nullable: false);
        }
    }
}
