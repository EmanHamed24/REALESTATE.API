using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace REALESTATE_.API.Migrations
{
    /// <inheritdoc />
    public partial class ChangeFavoriteOwnerToBuyer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Favorites_Owners_OwnerId",
                table: "Favorites");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "Favorites",
                newName: "BuyerId");

            migrationBuilder.RenameIndex(
                name: "IX_Favorites_OwnerId_PropertyId",
                table: "Favorites",
                newName: "IX_Favorites_BuyerId_PropertyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Favorites_Owners_BuyerId",
                table: "Favorites",
                column: "BuyerId",
                principalTable: "Owners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Favorites_Owners_BuyerId",
                table: "Favorites");

            migrationBuilder.RenameColumn(
                name: "BuyerId",
                table: "Favorites",
                newName: "OwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_Favorites_BuyerId_PropertyId",
                table: "Favorites",
                newName: "IX_Favorites_OwnerId_PropertyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Favorites_Owners_OwnerId",
                table: "Favorites",
                column: "OwnerId",
                principalTable: "Owners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
