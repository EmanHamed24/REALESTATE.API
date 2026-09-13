using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace REALESTATE_.API.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Owners",
                type: "longtext",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "Owners");
        }
    }
}
