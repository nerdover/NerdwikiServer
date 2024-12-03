using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NerdwikiServer.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lesson_ToTopic",
                table: "Lesson");

            migrationBuilder.AddForeignKey(
                name: "FK_Lesson_ToTopic",
                table: "Lesson",
                column: "TopicId",
                principalTable: "Topic",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lesson_ToTopic",
                table: "Lesson");

            migrationBuilder.AddForeignKey(
                name: "FK_Lesson_ToTopic",
                table: "Lesson",
                column: "TopicId",
                principalTable: "Topic",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
