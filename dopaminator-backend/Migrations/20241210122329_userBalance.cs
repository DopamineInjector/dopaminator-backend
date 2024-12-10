using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dopaminator_backend.Migrations
{
    /// <inheritdoc />
    public partial class userBalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WalletId",
                table: "Users");

            migrationBuilder.AddColumn<float>(
                name: "Balance",
                table: "Users",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Balance",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "WalletId",
                table: "Users",
                type: "text",
                nullable: true);
        }
    }
}
