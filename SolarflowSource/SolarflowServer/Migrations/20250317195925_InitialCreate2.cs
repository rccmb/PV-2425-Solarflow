using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolarflowServer.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Batteries",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    api_key = table.Column<string>(type: "nvarchar(450)", nullable: false, defaultValue: ""),
                    charge_level = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    charging_source = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: ""),
                    battery_mode = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: ""),
                    minimal_treshold = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    maximum_treshold = table.Column<int>(type: "int", nullable: false, defaultValue: 100),
                    spending_start_time = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "00:00"),
                    spending_end_time = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "09:00"),
                    last_update = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Batteries", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Batteries_api_key",
                table: "Batteries",
                column: "api_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Batteries_user_id",
                table: "Batteries",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Batteries");
        }
    }
}
