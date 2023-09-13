using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatingTelegramBot.Migrations
{
    /// <inheritdoc />
    public partial class UserViewAddedWasShown : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "WasShown",
                table: "UserViews",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WasShown",
                table: "UserViews");
        }
    }
}
