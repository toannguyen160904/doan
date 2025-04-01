using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace doan.Migrations
{
    /// <inheritdoc />
    public partial class table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Levels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Levels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Baihoc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    LevelId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LevelId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Baihoc", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Baihoc_Levels_LevelId",
                        column: x => x.LevelId,
                        principalTable: "Levels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Baihoc_Levels_LevelId1",
                        column: x => x.LevelId1,
                        principalTable: "Levels",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "nguphap",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CongThuc = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    GiaiThich = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CauViDu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LessonId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nguphap", x => x.Id);
                    table.ForeignKey(
                        name: "FK_nguphap_Baihoc_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Baihoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tuvung",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tuvung = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PhatAm = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    AmHan = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    HanTu = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Nghia = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    LessonId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tuvung", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tuvung_Baihoc_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Baihoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Baihoc_LevelId",
                table: "Baihoc",
                column: "LevelId");

            migrationBuilder.CreateIndex(
                name: "IX_Baihoc_LevelId1",
                table: "Baihoc",
                column: "LevelId1");

            migrationBuilder.CreateIndex(
                name: "IX_nguphap_LessonId",
                table: "nguphap",
                column: "LessonId");

            migrationBuilder.CreateIndex(
                name: "IX_tuvung_LessonId",
                table: "tuvung",
                column: "LessonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "nguphap");

            migrationBuilder.DropTable(
                name: "tuvung");

            migrationBuilder.DropTable(
                name: "Baihoc");

            migrationBuilder.DropTable(
                name: "Levels");
        }
    }
}
