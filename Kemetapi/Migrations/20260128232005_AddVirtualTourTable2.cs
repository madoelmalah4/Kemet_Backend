using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kemet_api.Migrations
{
    /// <inheritdoc />
    public partial class AddVirtualTourTable2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VirtualTours",
                columns: table => new
                {
                    Vr_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Destination_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Vr_urlImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VirtualTours", x => x.Vr_id);
                    table.ForeignKey(
                        name: "FK_VirtualTours_Destinations_Destination_id",
                        column: x => x.Destination_id,
                        principalTable: "Destinations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VirtualTours_Destination_id",
                table: "VirtualTours",
                column: "Destination_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VirtualTours");
        }
    }
}
