using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MentraAI.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserProfileForTrackRecommender : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WeeklyHours",
                table: "UserProfiles");

            migrationBuilder.RenameColumn(
                name: "InterestsJson",
                table: "UserProfiles",
                newName: "RemoteWork");

            migrationBuilder.RenameColumn(
                name: "CareerGoals",
                table: "UserProfiles",
                newName: "OrgSize");

            migrationBuilder.RenameColumn(
                name: "Background",
                table: "UserProfiles",
                newName: "Industry");

            migrationBuilder.AddColumn<string>(
                name: "AISelect",
                table: "UserProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Age",
                table: "UserProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EdLevel",
                table: "UserProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Employment",
                table: "UserProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FutureSkillsJson",
                table: "UserProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "WorkExp",
                table: "UserProfiles",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "YearsCode",
                table: "UserProfiles",
                type: "float",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "PassingScore",
                table: "QuizAttempts",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AISelect",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "Age",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "EdLevel",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "Employment",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "FutureSkillsJson",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "WorkExp",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "YearsCode",
                table: "UserProfiles");

            migrationBuilder.RenameColumn(
                name: "RemoteWork",
                table: "UserProfiles",
                newName: "InterestsJson");

            migrationBuilder.RenameColumn(
                name: "OrgSize",
                table: "UserProfiles",
                newName: "CareerGoals");

            migrationBuilder.RenameColumn(
                name: "Industry",
                table: "UserProfiles",
                newName: "Background");

            migrationBuilder.AddColumn<int>(
                name: "WeeklyHours",
                table: "UserProfiles",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PassingScore",
                table: "QuizAttempts",
                type: "int",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);
        }
    }
}
