using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatingTelegramBot.Migrations
{
    /// <inheritdoc />
    public partial class UserDirectionMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Direction",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Direction",
                table: "Users");
        }
    }
}
