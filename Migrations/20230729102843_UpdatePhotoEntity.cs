using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatingTelegramBot.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePhotoEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Source",
                table: "Photos",
                newName: "Path");

            migrationBuilder.AddColumn<string>(
                name: "FileId",
                table: "Photos",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileId",
                table: "Photos");

            migrationBuilder.RenameColumn(
                name: "Path",
                table: "Photos",
                newName: "Source");
        }
    }
}
