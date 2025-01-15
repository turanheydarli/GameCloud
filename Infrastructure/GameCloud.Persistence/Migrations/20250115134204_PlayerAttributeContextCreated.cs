using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameCloud.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PlayerAttributeContextCreated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayerAttribute_Player_PlayerId",
                schema: "gc",
                table: "PlayerAttribute");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlayerAttribute",
                schema: "gc",
                table: "PlayerAttribute");

            migrationBuilder.RenameTable(
                name: "PlayerAttribute",
                schema: "gc",
                newName: "Attributes",
                newSchema: "gc");

            migrationBuilder.RenameIndex(
                name: "IX_PlayerAttribute_PlayerId",
                schema: "gc",
                table: "Attributes",
                newName: "IX_Attributes_PlayerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Attributes",
                schema: "gc",
                table: "Attributes",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Attributes_Player_PlayerId",
                schema: "gc",
                table: "Attributes",
                column: "PlayerId",
                principalSchema: "gc",
                principalTable: "Player",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attributes_Player_PlayerId",
                schema: "gc",
                table: "Attributes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Attributes",
                schema: "gc",
                table: "Attributes");

            migrationBuilder.RenameTable(
                name: "Attributes",
                schema: "gc",
                newName: "PlayerAttribute",
                newSchema: "gc");

            migrationBuilder.RenameIndex(
                name: "IX_Attributes_PlayerId",
                schema: "gc",
                table: "PlayerAttribute",
                newName: "IX_PlayerAttribute_PlayerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlayerAttribute",
                schema: "gc",
                table: "PlayerAttribute",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerAttribute_Player_PlayerId",
                schema: "gc",
                table: "PlayerAttribute",
                column: "PlayerId",
                principalSchema: "gc",
                principalTable: "Player",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
