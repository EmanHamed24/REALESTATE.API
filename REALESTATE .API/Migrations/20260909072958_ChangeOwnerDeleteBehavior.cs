using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace REALESTATE_.API.Migrations
{
    /// <inheritdoc />
    public partial class ChangeOwnerDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Properties_Owners_OwnerId",
                table: "Properties");

            migrationBuilder.AlterColumn<int>(
                name: "OwnerId",
                table: "Properties",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_Owners_OwnerId",
                table: "Properties",
                column: "OwnerId",
                principalTable: "Owners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Properties_Owners_OwnerId",
                table: "Properties");

            migrationBuilder.AlterColumn<int>(
                name: "OwnerId",
                table: "Properties",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_Owners_OwnerId",
                table: "Properties",
                column: "OwnerId",
                principalTable: "Owners",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
