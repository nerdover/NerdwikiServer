using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NerdwikiServer.Migrations
{
    /// <inheritdoc />
    public partial class CoverFeat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cover",
                table: "Lesson",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cover",
                table: "Lesson");
        }
    }
}
