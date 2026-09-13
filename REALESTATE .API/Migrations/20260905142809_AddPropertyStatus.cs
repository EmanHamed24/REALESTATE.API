using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace REALESTATE_.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Properties",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Properties",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
