using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mangalith.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuotaAndRateLimitingSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RateLimitEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Endpoint = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RequestCount = table.Column<int>(type: "integer", nullable: false),
                    WindowStartUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastRequestUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RateLimitEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RateLimitEntries_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserQuotas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    StorageUsedBytes = table.Column<long>(type: "bigint", nullable: false),
                    FilesUploadedToday = table.Column<int>(type: "integer", nullable: false),
                    MangasCreated = table.Column<int>(type: "integer", nullable: false),
                    LastResetDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserQuotas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserQuotas_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RateLimitEntries_Endpoint",
                table: "RateLimitEntries",
                column: "Endpoint");

            migrationBuilder.CreateIndex(
                name: "IX_RateLimitEntries_LastRequestUtc",
                table: "RateLimitEntries",
                column: "LastRequestUtc");

            migrationBuilder.CreateIndex(
                name: "IX_RateLimitEntries_UserId",
                table: "RateLimitEntries",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RateLimitEntries_UserId_Endpoint",
                table: "RateLimitEntries",
                columns: new[] { "UserId", "Endpoint" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RateLimitEntries_WindowStartUtc",
                table: "RateLimitEntries",
                column: "WindowStartUtc");

            migrationBuilder.CreateIndex(
                name: "IX_UserQuotas_LastResetDate",
                table: "UserQuotas",
                column: "LastResetDate");

            migrationBuilder.CreateIndex(
                name: "IX_UserQuotas_StorageUsedBytes",
                table: "UserQuotas",
                column: "StorageUsedBytes");

            migrationBuilder.CreateIndex(
                name: "IX_UserQuotas_UserId",
                table: "UserQuotas",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RateLimitEntries");

            migrationBuilder.DropTable(
                name: "UserQuotas");
        }
    }
}
