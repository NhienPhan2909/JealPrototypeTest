using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace JealPrototype.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Story3_6_Lead_StatusSyncFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "last_known_easycars_status",
                table: "lead",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "status_synced_at",
                table: "lead",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "lead_status_conflicts",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    dealership_id = table.Column<int>(type: "integer", nullable: false),
                    lead_id = table.Column<int>(type: "integer", nullable: false),
                    easycars_lead_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    local_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    remote_status = table.Column<int>(type: "integer", nullable: false),
                    detected_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_resolved = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    resolved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    resolved_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    resolution = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lead_status_conflicts", x => x.id);
                });

            migrationBuilder.UpdateData(
                table: "system_settings",
                keyColumn: "key",
                keyValue: "easycar_image_sync_enabled",
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 2, 26, 23, 10, 21, 568, DateTimeKind.Utc).AddTicks(3466), new DateTime(2026, 2, 26, 23, 10, 21, 568, DateTimeKind.Utc).AddTicks(3466) });

            migrationBuilder.UpdateData(
                table: "system_settings",
                keyColumn: "key",
                keyValue: "easycar_sync_concurrency",
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 2, 26, 23, 10, 21, 568, DateTimeKind.Utc).AddTicks(3465), new DateTime(2026, 2, 26, 23, 10, 21, 568, DateTimeKind.Utc).AddTicks(3465) });

            migrationBuilder.UpdateData(
                table: "system_settings",
                keyColumn: "key",
                keyValue: "easycar_sync_cron",
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 2, 26, 23, 10, 21, 568, DateTimeKind.Utc).AddTicks(3464), new DateTime(2026, 2, 26, 23, 10, 21, 568, DateTimeKind.Utc).AddTicks(3465) });

            migrationBuilder.UpdateData(
                table: "system_settings",
                keyColumn: "key",
                keyValue: "easycar_sync_enabled",
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 2, 26, 23, 10, 21, 568, DateTimeKind.Utc).AddTicks(3461), new DateTime(2026, 2, 26, 23, 10, 21, 568, DateTimeKind.Utc).AddTicks(3463) });

            migrationBuilder.CreateIndex(
                name: "idx_lead_status_conflicts_dealership",
                table: "lead_status_conflicts",
                column: "dealership_id");

            migrationBuilder.CreateIndex(
                name: "idx_lead_status_conflicts_dealership_unresolved",
                table: "lead_status_conflicts",
                columns: new[] { "dealership_id", "is_resolved" });

            migrationBuilder.CreateIndex(
                name: "idx_lead_status_conflicts_lead",
                table: "lead_status_conflicts",
                column: "lead_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "lead_status_conflicts");

            migrationBuilder.DropColumn(
                name: "last_known_easycars_status",
                table: "lead");

            migrationBuilder.DropColumn(
                name: "status_synced_at",
                table: "lead");

            migrationBuilder.UpdateData(
                table: "system_settings",
                keyColumn: "key",
                keyValue: "easycar_image_sync_enabled",
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 2, 26, 22, 6, 39, 113, DateTimeKind.Utc).AddTicks(8675), new DateTime(2026, 2, 26, 22, 6, 39, 113, DateTimeKind.Utc).AddTicks(8675) });

            migrationBuilder.UpdateData(
                table: "system_settings",
                keyColumn: "key",
                keyValue: "easycar_sync_concurrency",
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 2, 26, 22, 6, 39, 113, DateTimeKind.Utc).AddTicks(8674), new DateTime(2026, 2, 26, 22, 6, 39, 113, DateTimeKind.Utc).AddTicks(8675) });

            migrationBuilder.UpdateData(
                table: "system_settings",
                keyColumn: "key",
                keyValue: "easycar_sync_cron",
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 2, 26, 22, 6, 39, 113, DateTimeKind.Utc).AddTicks(8674), new DateTime(2026, 2, 26, 22, 6, 39, 113, DateTimeKind.Utc).AddTicks(8674) });

            migrationBuilder.UpdateData(
                table: "system_settings",
                keyColumn: "key",
                keyValue: "easycar_sync_enabled",
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 2, 26, 22, 6, 39, 113, DateTimeKind.Utc).AddTicks(8670), new DateTime(2026, 2, 26, 22, 6, 39, 113, DateTimeKind.Utc).AddTicks(8672) });
        }
    }
}
