using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace doan.Migrations
{
    /// <inheritdoc />
    public partial class AddLearningPlanTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserLearningPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DailyTarget = table.Column<int>(type: "int", nullable: false),
                    CompletedToday = table.Column<int>(type: "int", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLearningPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserLearningPlans_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserVocabularyProgresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    VocabularyId = table.Column<int>(type: "int", nullable: false),
                    IsLearned = table.Column<bool>(type: "bit", nullable: false),
                    LearnedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserVocabularyProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserVocabularyProgresses_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserVocabularyProgresses_tuvung_VocabularyId",
                        column: x => x.VocabularyId,
                        principalTable: "tuvung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserLearningPlans_UserId",
                table: "UserLearningPlans",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserVocabularyProgresses_UserId",
                table: "UserVocabularyProgresses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserVocabularyProgresses_VocabularyId",
                table: "UserVocabularyProgresses",
                column: "VocabularyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserLearningPlans");

            migrationBuilder.DropTable(
                name: "UserVocabularyProgresses");
        }
    }
}
