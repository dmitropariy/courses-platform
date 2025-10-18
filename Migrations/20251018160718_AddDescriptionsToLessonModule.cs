using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace courses_platform.Migrations
{
    /// <inheritdoc />
    public partial class AddDescriptionsToLessonModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ModuleDescription",
                table: "Modules",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LessonDescription",
                table: "Lessons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModuleDescription",
                table: "Modules");

            migrationBuilder.DropColumn(
                name: "LessonDescription",
                table: "Lessons");
        }
    }
}
