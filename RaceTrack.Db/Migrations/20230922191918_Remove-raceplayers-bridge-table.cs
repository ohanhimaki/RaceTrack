using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaceTrack.Db.Migrations
{
    /// <inheritdoc />
    public partial class Removeraceplayersbridgetable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RacePlayer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RacePlayer",
                columns: table => new
                {
                    RaceId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlayerId = table.Column<int>(type: "INTEGER", nullable: false),
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RacePlayer", x => new { x.RaceId, x.PlayerId });
                    table.ForeignKey(
                        name: "FK_RacePlayer_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RacePlayer_Races_RaceId",
                        column: x => x.RaceId,
                        principalTable: "Races",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RacePlayer_PlayerId",
                table: "RacePlayer",
                column: "PlayerId");
        }
    }
}
