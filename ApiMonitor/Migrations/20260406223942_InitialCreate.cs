using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiMonitor.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProviderApis",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    HttpMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IntervalSeconds = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastStatusCode = table.Column<int>(type: "int", nullable: false),
                    LastAlertSentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderApis", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApiMonitorLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProviderApiId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StatusCode = table.Column<int>(type: "int", nullable: false),
                    ResponseJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponseTimeMs = table.Column<long>(type: "bigint", nullable: false),
                    CheckedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AlertSent = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiMonitorLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApiMonitorLogs_ProviderApis_ProviderApiId",
                        column: x => x.ProviderApiId,
                        principalTable: "ProviderApis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApiMonitorLogs_CheckedAt",
                table: "ApiMonitorLogs",
                column: "CheckedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ApiMonitorLogs_ProviderApiId",
                table: "ApiMonitorLogs",
                column: "ProviderApiId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiMonitorLogs");

            migrationBuilder.DropTable(
                name: "ProviderApis");
        }
    }
}
