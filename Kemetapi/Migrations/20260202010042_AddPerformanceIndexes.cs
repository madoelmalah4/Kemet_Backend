using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kemet_api.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Trips_CreatedAt",
                table: "Trips",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Days_TripId_DayNumber",
                table: "Days",
                columns: new[] { "TripId", "DayNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_DayActivities_DayId_StartTime",
                table: "DayActivities",
                columns: new[] { "DayId", "StartTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Trips_CreatedAt",
                table: "Trips");

            migrationBuilder.DropIndex(
                name: "IX_Days_TripId_DayNumber",
                table: "Days");

            migrationBuilder.DropIndex(
                name: "IX_DayActivities_DayId_StartTime",
                table: "DayActivities");
        }
    }
}
