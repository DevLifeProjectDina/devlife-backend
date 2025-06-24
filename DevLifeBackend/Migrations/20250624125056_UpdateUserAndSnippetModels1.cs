using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevLifeBackend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserAndSnippetModels1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "CodeSnippets",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Source",
                table: "CodeSnippets");
        }
    }
}
