using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MentraAI.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PassingScore",
                table: "QuizAttempts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TimeLimitMinutes",
                table: "QuizAttempts",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PassingScore",
                table: "QuizAttempts");

            migrationBuilder.DropColumn(
                name: "TimeLimitMinutes",
                table: "QuizAttempts");
        }
    }
}
