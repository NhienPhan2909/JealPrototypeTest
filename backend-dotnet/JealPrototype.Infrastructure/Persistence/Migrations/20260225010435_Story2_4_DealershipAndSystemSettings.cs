using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace JealPrototype.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Story2_4_DealershipAndSystemSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "dealership_settings",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    dealership_id = table.Column<int>(type: "integer", nullable: false),
                    easycar_auto_sync_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dealership_settings", x => x.id);
                    table.ForeignKey(
                        name: "FK_dealership_settings_dealership_dealership_id",
                        column: x => x.dealership_id,
                        principalTable: "dealership",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "system_settings",
                columns: table => new
                {
                    key = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    value = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_system_settings", x => x.key);
                });

            migrationBuilder.InsertData(
                table: "system_settings",
                columns: new[] { "key", "created_at", "description", "updated_at", "value" },
                values: new object[,]
                {
                    { "easycar_sync_concurrency", new DateTime(2026, 2, 25, 1, 4, 32, 322, DateTimeKind.Utc).AddTicks(4808), "Max concurrent dealership syncs (1=sequential)", new DateTime(2026, 2, 25, 1, 4, 32, 322, DateTimeKind.Utc).AddTicks(4809), "1" },
                    { "easycar_sync_cron", new DateTime(2026, 2, 25, 1, 4, 32, 322, DateTimeKind.Utc).AddTicks(4807), "Cron expression for sync schedule (default: 2 AM daily)", new DateTime(2026, 2, 25, 1, 4, 32, 322, DateTimeKind.Utc).AddTicks(4807), "0 2 * * *" },
                    { "easycar_sync_enabled", new DateTime(2026, 2, 25, 1, 4, 32, 322, DateTimeKind.Utc).AddTicks(4801), "Global toggle for all EasyCars stock synchronization", new DateTime(2026, 2, 25, 1, 4, 32, 322, DateTimeKind.Utc).AddTicks(4804), "true" }
                });

            migrationBuilder.CreateIndex(
                name: "idx_dealership_settings_auto_sync",
                table: "dealership_settings",
                column: "easycar_auto_sync_enabled",
                filter: "easycar_auto_sync_enabled = TRUE");

            migrationBuilder.CreateIndex(
                name: "idx_dealership_settings_dealership_id",
                table: "dealership_settings",
                column: "dealership_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dealership_settings");

            migrationBuilder.DropTable(
                name: "system_settings");
        }
    }
}
