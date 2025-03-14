using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameCloud.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedRoomServices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Rooms",
                schema: "gc",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    GameId = table.Column<string>(type: "text", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    Metadata = table.Column<string>(type: "text", nullable: false),
                    PlayerIds = table.Column<string>(type: "text", nullable: false),
                    SpectatorIds = table.Column<string>(type: "text", nullable: false),
                    MaxPlayers = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CurrentTurnUserId = table.Column<string>(type: "text", nullable: true),
                    TurnNumber = table.Column<int>(type: "integer", nullable: false),
                    IsPrivate = table.Column<bool>(type: "boolean", nullable: false),
                    Label = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoomConfigs",
                schema: "gc",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MinPlayers = table.Column<int>(type: "integer", nullable: false),
                    MaxPlayers = table.Column<int>(type: "integer", nullable: false),
                    TurnTimerInSeconds = table.Column<int>(type: "integer", nullable: false),
                    AllowSpectators = table.Column<bool>(type: "boolean", nullable: false),
                    PersistState = table.Column<bool>(type: "boolean", nullable: false),
                    CustomConfig = table.Column<string>(type: "text", nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoomConfigs_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalSchema: "gc",
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoomConfigs_RoomId",
                schema: "gc",
                table: "RoomConfigs",
                column: "RoomId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_CreatorId",
                schema: "gc",
                table: "Rooms",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_GameId",
                schema: "gc",
                table: "Rooms",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_State",
                schema: "gc",
                table: "Rooms",
                column: "State");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoomConfigs",
                schema: "gc");

            migrationBuilder.DropTable(
                name: "Rooms",
                schema: "gc");
        }
    }
}
