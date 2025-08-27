using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokemonApi.Migrations
{
    /// <inheritdoc />
    public partial class DeleteSubtypeColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubType",
                table: "Pokemons");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SubType",
                table: "Pokemons",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
