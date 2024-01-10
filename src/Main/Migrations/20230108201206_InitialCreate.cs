using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Main.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customer",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    twitterid = table.Column<string>(name: "twitter_id", type: "nvarchar(128)", nullable: false),
                    twitteraccesstoken = table.Column<string>(name: "twitter_accesstoken", type: "nvarchar(128)", nullable: true),
                    twitterrefreshtoken = table.Column<string>(name: "twitter_refreshtoken", type: "nvarchar(128)", nullable: true),
                    twitchaccesstoken = table.Column<string>(name: "twitch_accesstoken", type: "nvarchar(128)", nullable: true),
                    twitchrefreshtoken = table.Column<string>(name: "twitch_refreshtoken", type: "nvarchar(128)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customer", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Session",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    twitterid = table.Column<string>(name: "twitter_id", type: "nvarchar(256)", nullable: true),
                    loginsession = table.Column<string>(type: "nvarchar(64)", nullable: true),
                    twitterstate = table.Column<string>(name: "twitter_state", type: "nvarchar(500)", nullable: true),
                    twittercodechallenge = table.Column<string>(name: "twitter_code_challenge", type: "nvarchar(128)", nullable: true),
                    twitchstate = table.Column<string>(name: "twitch_state", type: "nvarchar(500)", nullable: true),
                    dt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Session", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Streamer",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    twitterid = table.Column<string>(name: "twitter_id", type: "nvarchar(256)", nullable: false),
                    num = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(256)", nullable: false),
                    twitter = table.Column<string>(type: "nvarchar(256)", nullable: true),
                    youtube = table.Column<string>(type: "nvarchar(256)", nullable: true),
                    twitch = table.Column<string>(type: "nvarchar(256)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Streamer", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Customer");

            migrationBuilder.DropTable(
                name: "Session");

            migrationBuilder.DropTable(
                name: "Streamer");
        }
    }
}
