using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameCloud.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ImageDocumentUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Developers_ProfilePhotoId",
                schema: "gc",
                table: "Developers");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                schema: "gc",
                table: "ImageDocuments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "gc",
                table: "Games",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ImageId",
                schema: "gc",
                table: "Games",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "gc",
                table: "Games",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ProfilePhotoId",
                schema: "gc",
                table: "Developers",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "gc",
                table: "Developers",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                schema: "gc",
                table: "Developers",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "ImageVariant",
                schema: "gc",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    Width = table.Column<int>(type: "integer", nullable: false),
                    Height = table.Column<int>(type: "integer", nullable: false),
                    VariantType = table.Column<string>(type: "text", nullable: false),
                    ImageDocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageVariant", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImageVariant_ImageDocuments_ImageDocumentId",
                        column: x => x.ImageDocumentId,
                        principalSchema: "gc",
                        principalTable: "ImageDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Games_ImageId",
                schema: "gc",
                table: "Games",
                column: "ImageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Developers_ProfilePhotoId",
                schema: "gc",
                table: "Developers",
                column: "ProfilePhotoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImageVariant_ImageDocumentId",
                schema: "gc",
                table: "ImageVariant",
                column: "ImageDocumentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Games_ImageDocuments_ImageId",
                schema: "gc",
                table: "Games",
                column: "ImageId",
                principalSchema: "gc",
                principalTable: "ImageDocuments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Games_ImageDocuments_ImageId",
                schema: "gc",
                table: "Games");

            migrationBuilder.DropTable(
                name: "ImageVariant",
                schema: "gc");

            migrationBuilder.DropIndex(
                name: "IX_Games_ImageId",
                schema: "gc",
                table: "Games");

            migrationBuilder.DropIndex(
                name: "IX_Developers_ProfilePhotoId",
                schema: "gc",
                table: "Developers");

            migrationBuilder.DropColumn(
                name: "Type",
                schema: "gc",
                table: "ImageDocuments");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "gc",
                table: "Games",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<Guid>(
                name: "ImageId",
                schema: "gc",
                table: "Games",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "gc",
                table: "Games",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProfilePhotoId",
                schema: "gc",
                table: "Developers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "gc",
                table: "Developers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                schema: "gc",
                table: "Developers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_Developers_ProfilePhotoId",
                schema: "gc",
                table: "Developers",
                column: "ProfilePhotoId");
        }
    }
}
