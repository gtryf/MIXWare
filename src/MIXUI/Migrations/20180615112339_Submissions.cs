using Microsoft.EntityFrameworkCore.Migrations;

namespace MIXUI.Migrations
{
    public partial class Submissions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Submissions",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    IdentityId = table.Column<string>(nullable: true),
                    Status = table.Column<string>(nullable: false, defaultValue: "New"),
                    Successful = table.Column<bool>(nullable: false),
                    WordCount = table.Column<int>(nullable: false),
                    AssemblyFileId = table.Column<string>(nullable: true),
                    ListingFileId = table.Column<string>(nullable: true),
                    SymbolFileId = table.Column<string>(nullable: true),
                    ErrorsStr = table.Column<string>(nullable: true),
                    WarningsStr = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Submissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Submissions_AspNetUsers_IdentityId",
                        column: x => x.IdentityId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_IdentityId",
                table: "Submissions",
                column: "IdentityId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Submissions");
        }
    }
}
