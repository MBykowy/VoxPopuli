using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VoxPopuli.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixCascadeDeleteIssue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Responses_Surveys_SurveyId",
                table: "Responses");

            migrationBuilder.AddForeignKey(
                name: "FK_Responses_Surveys_SurveyId",
                table: "Responses",
                column: "SurveyId",
                principalTable: "Surveys",
                principalColumn: "SurveyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Responses_Surveys_SurveyId",
                table: "Responses");

            migrationBuilder.AddForeignKey(
                name: "FK_Responses_Surveys_SurveyId",
                table: "Responses",
                column: "SurveyId",
                principalTable: "Surveys",
                principalColumn: "SurveyId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
