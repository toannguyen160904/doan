using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace doanapi.Migrations
{
    /// <inheritdoc />
    public partial class update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tuvung_Baihoc_LessonId",
                schema: "public",
                table: "tuvung");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "public",
                table: "tuvung",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now() at time zone 'utc'",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_tuvung_Baihoc_LessonId",
                schema: "public",
                table: "tuvung",
                column: "LessonId",
                principalSchema: "public",
                principalTable: "Baihoc",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tuvung_Baihoc_LessonId",
                schema: "public",
                table: "tuvung");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "public",
                table: "tuvung",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now() at time zone 'utc'");

            migrationBuilder.AddForeignKey(
                name: "FK_tuvung_Baihoc_LessonId",
                schema: "public",
                table: "tuvung",
                column: "LessonId",
                principalSchema: "public",
                principalTable: "Baihoc",
                principalColumn: "Id");
        }
    }
}
