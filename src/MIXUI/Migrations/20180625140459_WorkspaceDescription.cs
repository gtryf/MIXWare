using Microsoft.EntityFrameworkCore.Migrations;

namespace MIXUI.Migrations
{
    public partial class WorkspaceDescription : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Workspaces",
                maxLength: 2000,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Workspaces");
        }
    }
}
