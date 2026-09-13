using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace REALESTATE_.API.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerToProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OwnerId",
                table: "Properties",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Owners",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false),
                    Email = table.Column<string>(type: "longtext", nullable: false),
                    Password = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Owners", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_OwnerId",
                table: "Properties",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_Owners_OwnerId",
                table: "Properties",
                column: "OwnerId",
                principalTable: "Owners",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Properties_Owners_OwnerId",
                table: "Properties");

            migrationBuilder.DropTable(
                name: "Owners");

            migrationBuilder.DropIndex(
                name: "IX_Properties_OwnerId",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Properties");
        }
    }
}
