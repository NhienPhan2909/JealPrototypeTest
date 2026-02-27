using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JealPrototype.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Story3_5_Add_SyncType_Index : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateIndex(
                name: "idx_easycar_sync_logs_dealership_type_time",
                table: "easycar_sync_logs",
                columns: new[] { "dealership_id", "sync_type", "synced_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_easycar_sync_logs_dealership_type_time",
                table: "easycar_sync_logs");

            migrationBuilder.UpdateData(
                table: "system_settings",
                keyColumn: "key",
                keyValue: "easycar_image_sync_enabled",
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 2, 26, 21, 40, 57, 601, DateTimeKind.Utc).AddTicks(893), new DateTime(2026, 2, 26, 21, 40, 57, 601, DateTimeKind.Utc).AddTicks(893) });

            migrationBuilder.UpdateData(
                table: "system_settings",
                keyColumn: "key",
                keyValue: "easycar_sync_concurrency",
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 2, 26, 21, 40, 57, 601, DateTimeKind.Utc).AddTicks(892), new DateTime(2026, 2, 26, 21, 40, 57, 601, DateTimeKind.Utc).AddTicks(893) });

            migrationBuilder.UpdateData(
                table: "system_settings",
                keyColumn: "key",
                keyValue: "easycar_sync_cron",
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 2, 26, 21, 40, 57, 601, DateTimeKind.Utc).AddTicks(891), new DateTime(2026, 2, 26, 21, 40, 57, 601, DateTimeKind.Utc).AddTicks(892) });

            migrationBuilder.UpdateData(
                table: "system_settings",
                keyColumn: "key",
                keyValue: "easycar_sync_enabled",
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 2, 26, 21, 40, 57, 601, DateTimeKind.Utc).AddTicks(888), new DateTime(2026, 2, 26, 21, 40, 57, 601, DateTimeKind.Utc).AddTicks(890) });
        }
    }
}
