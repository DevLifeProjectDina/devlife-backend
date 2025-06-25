using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevLifeBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddWinStreakToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WinStreak",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WinStreak",
                table: "Users");
        }
    }
}
