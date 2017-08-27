using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace NadekoBot.Migrations
{
    public partial class GuildConfigMusicInfoIds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ulong>(
                name: "InfoChannelId",
                table: "GuildConfigs",
                nullable: true);

            migrationBuilder.AddColumn<ulong>(
                name: "MusicChannelId",
                table: "GuildConfigs",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InfoChannelId",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "MusicChannelId",
                table: "GuildConfigs");
        }
    }
}
