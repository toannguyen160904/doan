using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace doan.Migrations
{
    /// <inheritdoc />
    public partial class tabletentrung : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Baihoc_Name",
                table: "Baihoc",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Baihoc_Name",
                table: "Baihoc");
        }
    }
}
