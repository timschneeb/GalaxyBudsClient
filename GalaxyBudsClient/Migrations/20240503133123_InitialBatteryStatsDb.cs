using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GalaxyBudsClient.Migrations
{
    /// <inheritdoc />
    public partial class InitialBatteryStatsDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Records",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlacementL = table.Column<int>(type: "INTEGER", nullable: false),
                    PlacementR = table.Column<int>(type: "INTEGER", nullable: false),
                    BatteryL = table.Column<int>(type: "INTEGER", nullable: true),
                    BatteryR = table.Column<int>(type: "INTEGER", nullable: true),
                    BatteryCase = table.Column<int>(type: "INTEGER", nullable: true),
                    IsChargingL = table.Column<bool>(type: "INTEGER", nullable: true),
                    IsChargingR = table.Column<bool>(type: "INTEGER", nullable: true),
                    IsChargingCase = table.Column<bool>(type: "INTEGER", nullable: true),
                    HostDevice = table.Column<int>(type: "INTEGER", nullable: true),
                    NoiseControlMode = table.Column<int>(type: "INTEGER", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Records", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Records");
        }
    }
}
