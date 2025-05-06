using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace doan.Migrations
{
    /// <inheritdoc />
    public partial class FixUserLearningPlanRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserLearningPlanId",
                table: "tuvung",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tuvung_UserLearningPlanId",
                table: "tuvung",
                column: "UserLearningPlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_tuvung_UserLearningPlans_UserLearningPlanId",
                table: "tuvung",
                column: "UserLearningPlanId",
                principalTable: "UserLearningPlans",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tuvung_UserLearningPlans_UserLearningPlanId",
                table: "tuvung");

            migrationBuilder.DropIndex(
                name: "IX_tuvung_UserLearningPlanId",
                table: "tuvung");

            migrationBuilder.DropColumn(
                name: "UserLearningPlanId",
                table: "tuvung");
        }
    }
}
