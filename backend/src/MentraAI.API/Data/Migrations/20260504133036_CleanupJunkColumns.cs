using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MentraAI.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class CleanupJunkColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserStageProgress_Roadmaps_RoadmapId1",
                table: "UserStageProgress");

            migrationBuilder.DropIndex(
                name: "IX_UserStageProgress_RoadmapId1",
                table: "UserStageProgress");

            migrationBuilder.DropColumn(
                name: "WeeklyHours",
                table: "UserTracks");

            migrationBuilder.DropColumn(
                name: "RoadmapId1",
                table: "UserStageProgress");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WeeklyHours",
                table: "UserTracks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RoadmapId1",
                table: "UserStageProgress",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserStageProgress_RoadmapId1",
                table: "UserStageProgress",
                column: "RoadmapId1");

            migrationBuilder.AddForeignKey(
                name: "FK_UserStageProgress_Roadmaps_RoadmapId1",
                table: "UserStageProgress",
                column: "RoadmapId1",
                principalTable: "Roadmaps",
                principalColumn: "Id");
        }
    }
}
