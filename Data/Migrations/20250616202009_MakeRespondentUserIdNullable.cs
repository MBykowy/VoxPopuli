using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VoxPopuli.Data.Migrations
{
    /// <inheritdoc />
    public partial class MakeRespondentUserIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Responses_AspNetUsers_RespondentUserId",
                table: "Responses");

            migrationBuilder.AlterColumn<string>(
                name: "RespondentUserId",
                table: "Responses",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_Responses_AspNetUsers_RespondentUserId",
                table: "Responses",
                column: "RespondentUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Responses_AspNetUsers_RespondentUserId",
                table: "Responses");

            migrationBuilder.AlterColumn<string>(
                name: "RespondentUserId",
                table: "Responses",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Responses_AspNetUsers_RespondentUserId",
                table: "Responses",
                column: "RespondentUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
