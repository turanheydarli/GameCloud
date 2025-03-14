using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameCloud.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LeaderboardEntitiesAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Leaderboards",
                schema: "gc",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GameId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    DisplayName = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Operator = table.Column<int>(type: "integer", nullable: false),
                    ResetStrategy = table.Column<string>(type: "text", nullable: true),
                    MaxSize = table.Column<int>(type: "integer", nullable: true),
                    Metadata = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    StartTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Category = table.Column<string>(type: "text", nullable: true),
                    SubCategory = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leaderboards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Leaderboards_Games_GameId",
                        column: x => x.GameId,
                        principalSchema: "gc",
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LeaderboardArchives",
                schema: "gc",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaderboardId = table.Column<Guid>(type: "uuid", nullable: false),
                    Period = table.Column<string>(type: "text", nullable: false),
                    StartTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Rankings = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    Statistics = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaderboardArchives", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaderboardArchives_Leaderboards_LeaderboardId",
                        column: x => x.LeaderboardId,
                        principalSchema: "gc",
                        principalTable: "Leaderboards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LeaderboardRecords",
                schema: "gc",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaderboardId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Score = table.Column<long>(type: "bigint", nullable: false),
                    Rank = table.Column<int>(type: "integer", nullable: true),
                    SubScore = table.Column<long>(type: "bigint", nullable: false),
                    UpdateCount = table.Column<int>(type: "integer", nullable: false),
                    Metadata = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaderboardRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaderboardRecords_Leaderboards_LeaderboardId",
                        column: x => x.LeaderboardId,
                        principalSchema: "gc",
                        principalTable: "Leaderboards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeaderboardRecords_Player_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "gc",
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeaderboardArchives_LeaderboardId",
                schema: "gc",
                table: "LeaderboardArchives",
                column: "LeaderboardId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaderboardRecords_LeaderboardId",
                schema: "gc",
                table: "LeaderboardRecords",
                column: "LeaderboardId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaderboardRecords_LeaderboardId_PlayerId",
                schema: "gc",
                table: "LeaderboardRecords",
                columns: new[] { "LeaderboardId", "PlayerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeaderboardRecords_LeaderboardId_Rank",
                schema: "gc",
                table: "LeaderboardRecords",
                columns: new[] { "LeaderboardId", "Rank" });

            migrationBuilder.CreateIndex(
                name: "IX_LeaderboardRecords_PlayerId",
                schema: "gc",
                table: "LeaderboardRecords",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Leaderboards_Category",
                schema: "gc",
                table: "Leaderboards",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Leaderboards_GameId",
                schema: "gc",
                table: "Leaderboards",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_Leaderboards_GameId_Name",
                schema: "gc",
                table: "Leaderboards",
                columns: new[] { "GameId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Leaderboards_IsActive",
                schema: "gc",
                table: "Leaderboards",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Leaderboards_Name",
                schema: "gc",
                table: "Leaderboards",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LeaderboardArchives",
                schema: "gc");

            migrationBuilder.DropTable(
                name: "LeaderboardRecords",
                schema: "gc");

            migrationBuilder.DropTable(
                name: "Leaderboards",
                schema: "gc");
        }
    }
}
