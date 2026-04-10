using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiMonitor.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApiLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StatusCode = table.Column<int>(type: "int", nullable: false),
                    RequestBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponseBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Method = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Path = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    InterfaceBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProviderName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Host = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    OriginHost = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    AlertSent = table.Column<bool>(type: "bit", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApiLogs_ProviderName",
                table: "ApiLogs",
                column: "ProviderName");

            migrationBuilder.CreateIndex(
                name: "IX_ApiLogs_ReceivedAt",
                table: "ApiLogs",
                column: "ReceivedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ApiLogs_StatusCode",
                table: "ApiLogs",
                column: "StatusCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiLogs");
        }
    }
}
