using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace doan.Migrations
{
    /// <inheritdoc />
    public partial class AddSRSFieldsToUserVocabularyProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "NextReviewDate",
                table: "UserVocabularyProgresses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReviewLevel",
                table: "UserVocabularyProgresses",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NextReviewDate",
                table: "UserVocabularyProgresses");

            migrationBuilder.DropColumn(
                name: "ReviewLevel",
                table: "UserVocabularyProgresses");
        }
    }
}
