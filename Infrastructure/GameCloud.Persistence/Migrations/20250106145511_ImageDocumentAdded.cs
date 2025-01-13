using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameCloud.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ImageDocumentAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ImageId",
                schema: "gc",
                table: "Games",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ProfilePhotoId",
                schema: "gc",
                table: "Developers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "ImageDocuments",
                schema: "gc",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    Width = table.Column<int>(type: "integer", nullable: false),
                    Height = table.Column<int>(type: "integer", nullable: false),
                    FileType = table.Column<string>(type: "text", nullable: false),
                    StorageProvider = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageDocuments", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Developers_ProfilePhotoId",
                schema: "gc",
                table: "Developers",
                column: "ProfilePhotoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Developers_ImageDocuments_ProfilePhotoId",
                schema: "gc",
                table: "Developers",
                column: "ProfilePhotoId",
                principalSchema: "gc",
                principalTable: "ImageDocuments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Developers_ImageDocuments_ProfilePhotoId",
                schema: "gc",
                table: "Developers");

            migrationBuilder.DropTable(
                name: "ImageDocuments",
                schema: "gc");

            migrationBuilder.DropIndex(
                name: "IX_Developers_ProfilePhotoId",
                schema: "gc",
                table: "Developers");

            migrationBuilder.DropColumn(
                name: "ImageId",
                schema: "gc",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "ProfilePhotoId",
                schema: "gc",
                table: "Developers");
        }
    }
}
