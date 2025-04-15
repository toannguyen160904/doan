using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace doan.Migrations
{
    /// <inheritdoc />
    public partial class addflashcardtable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Flashcards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GrammarStructureId = table.Column<int>(type: "int", nullable: true),
                    VocabularyId = table.Column<int>(type: "int", nullable: true),
                    BaihocId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Flashcards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Flashcards_Baihoc_BaihocId",
                        column: x => x.BaihocId,
                        principalTable: "Baihoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Flashcards_nguphap_GrammarStructureId",
                        column: x => x.GrammarStructureId,
                        principalTable: "nguphap",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Flashcards_tuvung_VocabularyId",
                        column: x => x.VocabularyId,
                        principalTable: "tuvung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Flashcards_BaihocId",
                table: "Flashcards",
                column: "BaihocId");

            migrationBuilder.CreateIndex(
                name: "IX_Flashcards_GrammarStructureId",
                table: "Flashcards",
                column: "GrammarStructureId",
                unique: true,
                filter: "[GrammarStructureId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Flashcards_VocabularyId",
                table: "Flashcards",
                column: "VocabularyId",
                unique: true,
                filter: "[VocabularyId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Flashcards");
        }
    }
}
