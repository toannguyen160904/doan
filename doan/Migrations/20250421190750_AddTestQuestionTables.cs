using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace doan.Migrations
{
    /// <inheritdoc />
    public partial class AddTestQuestionTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tuvung_Baihoc_LessonId",
                table: "tuvung");

            migrationBuilder.AlterColumn<string>(
                name: "Tuvung",
                table: "tuvung",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Nghia",
                table: "tuvung",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "LessonId",
                table: "tuvung",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "TestQuestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CorrectAnswer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Choices = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Topic = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Level = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    Explanation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestQuestions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TestResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TotalScore = table.Column<int>(type: "int", nullable: false),
                    CorrectAnswers = table.Column<int>(type: "int", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SuggestedLevel = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestResults_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserAnswers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TestResultId = table.Column<int>(type: "int", nullable: false),
                    TestQuestionId = table.Column<int>(type: "int", nullable: false),
                    SelectedAnswer = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAnswers_TestQuestions_TestQuestionId",
                        column: x => x.TestQuestionId,
                        principalTable: "TestQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserAnswers_TestResults_TestResultId",
                        column: x => x.TestResultId,
                        principalTable: "TestResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TestResults_UserId",
                table: "TestResults",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_TestQuestionId",
                table: "UserAnswers",
                column: "TestQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_TestResultId",
                table: "UserAnswers",
                column: "TestResultId");

            migrationBuilder.AddForeignKey(
                name: "FK_tuvung_Baihoc_LessonId",
                table: "tuvung",
                column: "LessonId",
                principalTable: "Baihoc",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tuvung_Baihoc_LessonId",
                table: "tuvung");

            migrationBuilder.DropTable(
                name: "UserAnswers");

            migrationBuilder.DropTable(
                name: "TestQuestions");

            migrationBuilder.DropTable(
                name: "TestResults");

            migrationBuilder.AlterColumn<string>(
                name: "Tuvung",
                table: "tuvung",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Nghia",
                table: "tuvung",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "LessonId",
                table: "tuvung",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_tuvung_Baihoc_LessonId",
                table: "tuvung",
                column: "LessonId",
                principalTable: "Baihoc",
                principalColumn: "Id");
        }
    }
}
