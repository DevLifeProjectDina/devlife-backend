using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevLifeBackend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserAndSnippetModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Stack",
                table: "Users");

            migrationBuilder.AddColumn<string[]>(
                name: "Stacks",
                table: "Users",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<string>(
                name: "Difficulty",
                table: "CodeSnippets",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Stacks",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Difficulty",
                table: "CodeSnippets");

            migrationBuilder.AddColumn<string>(
                name: "Stack",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
