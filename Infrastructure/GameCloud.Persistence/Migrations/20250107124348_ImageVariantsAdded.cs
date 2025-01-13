using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameCloud.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ImageVariantsAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImageVariant_ImageDocuments_ImageDocumentId",
                schema: "gc",
                table: "ImageVariant");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ImageVariant",
                schema: "gc",
                table: "ImageVariant");

            migrationBuilder.RenameTable(
                name: "ImageVariant",
                schema: "gc",
                newName: "ImageVariants",
                newSchema: "gc");

            migrationBuilder.RenameIndex(
                name: "IX_ImageVariant_ImageDocumentId",
                schema: "gc",
                table: "ImageVariants",
                newName: "IX_ImageVariants_ImageDocumentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ImageVariants",
                schema: "gc",
                table: "ImageVariants",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ImageVariants_ImageDocuments_ImageDocumentId",
                schema: "gc",
                table: "ImageVariants",
                column: "ImageDocumentId",
                principalSchema: "gc",
                principalTable: "ImageDocuments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImageVariants_ImageDocuments_ImageDocumentId",
                schema: "gc",
                table: "ImageVariants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ImageVariants",
                schema: "gc",
                table: "ImageVariants");

            migrationBuilder.RenameTable(
                name: "ImageVariants",
                schema: "gc",
                newName: "ImageVariant",
                newSchema: "gc");

            migrationBuilder.RenameIndex(
                name: "IX_ImageVariants_ImageDocumentId",
                schema: "gc",
                table: "ImageVariant",
                newName: "IX_ImageVariant_ImageDocumentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ImageVariant",
                schema: "gc",
                table: "ImageVariant",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ImageVariant_ImageDocuments_ImageDocumentId",
                schema: "gc",
                table: "ImageVariant",
                column: "ImageDocumentId",
                principalSchema: "gc",
                principalTable: "ImageDocuments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
