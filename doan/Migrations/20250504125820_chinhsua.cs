using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace doan.Migrations
{
    /// <inheritdoc />
    public partial class chinhsua : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Baihoc_Name",
                table: "Baihoc");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Baihoc",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Baihoc_Name_LevelId",
                table: "Baihoc",
                columns: new[] { "Name", "LevelId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Baihoc_Name_LevelId",
                table: "Baihoc");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Baihoc");

            migrationBuilder.CreateIndex(
                name: "IX_Baihoc_Name",
                table: "Baihoc",
                column: "Name",
                unique: true);
        }
    }
}
