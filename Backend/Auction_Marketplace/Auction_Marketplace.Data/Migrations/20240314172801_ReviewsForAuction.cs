using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuctionMarketplace.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReviewsForAuction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Causes_CauseId",
                table: "Reviews");

            migrationBuilder.RenameColumn(
                name: "CauseId",
                table: "Reviews",
                newName: "AuctionId");

            migrationBuilder.RenameIndex(
                name: "IX_Reviews_CauseId",
                table: "Reviews",
                newName: "IX_Reviews_AuctionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Auctions_AuctionId",
                table: "Reviews",
                column: "AuctionId",
                principalTable: "Auctions",
                principalColumn: "AuctionId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Auctions_AuctionId",
                table: "Reviews");

            migrationBuilder.RenameColumn(
                name: "AuctionId",
                table: "Reviews",
                newName: "CauseId");

            migrationBuilder.RenameIndex(
                name: "IX_Reviews_AuctionId",
                table: "Reviews",
                newName: "IX_Reviews_CauseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Causes_CauseId",
                table: "Reviews",
                column: "CauseId",
                principalTable: "Causes",
                principalColumn: "CauseId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
