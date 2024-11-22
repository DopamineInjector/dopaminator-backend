using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dopaminator_backend.Migrations
{
    /// <inheritdoc />
    public partial class deleteJwtAddWallet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JwtToken",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "WalletId",
                table: "Users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WalletId",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "JwtToken",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
