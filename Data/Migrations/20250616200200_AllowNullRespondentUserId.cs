using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VoxPopuli.Data.Migrations
{
    /// <inheritdoc />
    public partial class AllowNullRespondentUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RespondentUserId",
                table: "Responses",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
