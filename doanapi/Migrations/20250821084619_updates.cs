using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace doanapi.Migrations
{
    /// <inheritdoc />
    public partial class updates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Flashcards_Baihoc_BaihocId",
                schema: "public",
                table: "Flashcards");

            migrationBuilder.DropForeignKey(
                name: "FK_Flashcards_nguphap_GrammarStructureId",
                schema: "public",
                table: "Flashcards");

            migrationBuilder.DropForeignKey(
                name: "FK_Flashcards_tuvung_VocabularyId",
                schema: "public",
                table: "Flashcards");

            migrationBuilder.AddForeignKey(
                name: "FK_Flashcards_Baihoc_BaihocId",
                schema: "public",
                table: "Flashcards",
                column: "BaihocId",
                principalSchema: "public",
                principalTable: "Baihoc",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Flashcards_nguphap_GrammarStructureId",
                schema: "public",
                table: "Flashcards",
                column: "GrammarStructureId",
                principalSchema: "public",
                principalTable: "nguphap",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Flashcards_tuvung_VocabularyId",
                schema: "public",
                table: "Flashcards",
                column: "VocabularyId",
                principalSchema: "public",
                principalTable: "tuvung",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Flashcards_Baihoc_BaihocId",
                schema: "public",
                table: "Flashcards");

            migrationBuilder.DropForeignKey(
                name: "FK_Flashcards_nguphap_GrammarStructureId",
                schema: "public",
                table: "Flashcards");

            migrationBuilder.DropForeignKey(
                name: "FK_Flashcards_tuvung_VocabularyId",
                schema: "public",
                table: "Flashcards");

            migrationBuilder.AddForeignKey(
                name: "FK_Flashcards_Baihoc_BaihocId",
                schema: "public",
                table: "Flashcards",
                column: "BaihocId",
                principalSchema: "public",
                principalTable: "Baihoc",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Flashcards_nguphap_GrammarStructureId",
                schema: "public",
                table: "Flashcards",
                column: "GrammarStructureId",
                principalSchema: "public",
                principalTable: "nguphap",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Flashcards_tuvung_VocabularyId",
                schema: "public",
                table: "Flashcards",
                column: "VocabularyId",
                principalSchema: "public",
                principalTable: "tuvung",
                principalColumn: "Id");
        }
    }
}
