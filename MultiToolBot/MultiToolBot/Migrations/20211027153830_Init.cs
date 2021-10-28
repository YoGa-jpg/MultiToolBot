using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MultiToolBot.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DiscordGuilds",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsJoined = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordGuilds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Dequeued",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Uri = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GuildId = table.Column<decimal>(type: "decimal(20,0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dequeued", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dequeued_DiscordGuilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "DiscordGuilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Queued",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Uri = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GuildId = table.Column<decimal>(type: "decimal(20,0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Queued", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Queued_DiscordGuilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "DiscordGuilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Dequeued_GuildId",
                table: "Dequeued",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_Queued_GuildId",
                table: "Queued",
                column: "GuildId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Dequeued");

            migrationBuilder.DropTable(
                name: "Queued");

            migrationBuilder.DropTable(
                name: "DiscordGuilds");
        }
    }
}
