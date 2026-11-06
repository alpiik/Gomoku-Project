using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.EF.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerTypes : Migration
    {
        /// <inheritdoc />
        // edited by hand: EF suggested defaultValue "", which is not a player type (EF would quietly read it
        // as the enum default, but the column would hold wrong data; DbStorageTests checks the real value).
        // Rows saved before this migration are A3/A4 games, and those were always human vs human.
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PlayerO",
                table: "SavedGames",
                type: "TEXT",
                nullable: false,
                defaultValue: "Human");

            migrationBuilder.AddColumn<string>(
                name: "PlayerX",
                table: "SavedGames",
                type: "TEXT",
                nullable: false,
                defaultValue: "Human");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlayerO",
                table: "SavedGames");

            migrationBuilder.DropColumn(
                name: "PlayerX",
                table: "SavedGames");
        }
    }
}
