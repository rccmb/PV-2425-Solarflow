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
            migrationBuilder.DropForeignKey(
                name: "FK_Batteries_Users_userId",
                table: "Batteries");

            migrationBuilder.DropIndex(
                name: "IX_Batteries_userId",
                table: "Batteries");

            migrationBuilder.DropColumn(
                name: "value",
                table: "Batteries");

            migrationBuilder.RenameColumn(
                name: "userId",
                table: "Batteries",
                newName: "user_id");

            migrationBuilder.AddColumn<string>(
                name: "api_key",
                table: "Batteries",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "auto_optimization",
                table: "Batteries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "charge_level",
                table: "Batteries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "charging_mode",
                table: "Batteries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "emergency_mode",
                table: "Batteries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "last_update",
                table: "Batteries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

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
            migrationBuilder.DropIndex(
                name: "IX_Batteries_api_key",
                table: "Batteries");

            migrationBuilder.DropIndex(
                name: "IX_Batteries_user_id",
                table: "Batteries");

            migrationBuilder.DropColumn(
                name: "api_key",
                table: "Batteries");

            migrationBuilder.DropColumn(
                name: "auto_optimization",
                table: "Batteries");

            migrationBuilder.DropColumn(
                name: "charge_level",
                table: "Batteries");

            migrationBuilder.DropColumn(
                name: "charging_mode",
                table: "Batteries");

            migrationBuilder.DropColumn(
                name: "emergency_mode",
                table: "Batteries");

            migrationBuilder.DropColumn(
                name: "last_update",
                table: "Batteries");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "Batteries",
                newName: "userId");

            migrationBuilder.AddColumn<int>(
                name: "value",
                table: "Batteries",
                type: "int",
                nullable: false,
                defaultValueSql: "0");

            migrationBuilder.CreateIndex(
                name: "IX_Batteries_userId",
                table: "Batteries",
                column: "userId");

            migrationBuilder.AddForeignKey(
                name: "FK_Batteries_Users_userId",
                table: "Batteries",
                column: "userId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
