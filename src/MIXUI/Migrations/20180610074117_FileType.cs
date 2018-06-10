using Microsoft.EntityFrameworkCore.Migrations;

namespace MIXUI.Migrations
{
    public partial class FileType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Files",
                nullable: false,
                defaultValue: "Source");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Files");
        }
    }
}
