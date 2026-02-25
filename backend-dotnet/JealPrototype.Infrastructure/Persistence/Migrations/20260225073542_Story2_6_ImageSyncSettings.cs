using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JealPrototype.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Story2_6_ImageSyncSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "system_settings",
                keyColumn: "key",
                keyValue: "easycar_sync_concurrency",
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 2, 25, 7, 35, 39, 965, DateTimeKind.Utc).AddTicks(1793), new DateTime(2026, 2, 25, 7, 35, 39, 965, DateTimeKind.Utc).AddTicks(1794) });

            migrationBuilder.UpdateData(
                table: "system_settings",
                keyColumn: "key",
                keyValue: "easycar_sync_cron",
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 2, 25, 7, 35, 39, 965, DateTimeKind.Utc).AddTicks(1792), new DateTime(2026, 2, 25, 7, 35, 39, 965, DateTimeKind.Utc).AddTicks(1793) });

            migrationBuilder.UpdateData(
                table: "system_settings",
                keyColumn: "key",
                keyValue: "easycar_sync_enabled",
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 2, 25, 7, 35, 39, 965, DateTimeKind.Utc).AddTicks(1788), new DateTime(2026, 2, 25, 7, 35, 39, 965, DateTimeKind.Utc).AddTicks(1791) });

            migrationBuilder.InsertData(
                table: "system_settings",
                columns: new[] { "key", "created_at", "description", "updated_at", "value" },
                values: new object[] { "easycar_image_sync_enabled", new DateTime(2026, 2, 25, 7, 35, 39, 965, DateTimeKind.Utc).AddTicks(1794), "Enable/disable image sync during stock sync (default: true)", new DateTime(2026, 2, 25, 7, 35, 39, 965, DateTimeKind.Utc).AddTicks(1795), "true" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "system_settings",
                keyColumn: "key",
                keyValue: "easycar_image_sync_enabled");

            migrationBuilder.UpdateData(
                table: "system_settings",
                keyColumn: "key",
                keyValue: "easycar_sync_concurrency",
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 2, 25, 1, 4, 32, 322, DateTimeKind.Utc).AddTicks(4808), new DateTime(2026, 2, 25, 1, 4, 32, 322, DateTimeKind.Utc).AddTicks(4809) });

            migrationBuilder.UpdateData(
                table: "system_settings",
                keyColumn: "key",
                keyValue: "easycar_sync_cron",
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 2, 25, 1, 4, 32, 322, DateTimeKind.Utc).AddTicks(4807), new DateTime(2026, 2, 25, 1, 4, 32, 322, DateTimeKind.Utc).AddTicks(4807) });

            migrationBuilder.UpdateData(
                table: "system_settings",
                keyColumn: "key",
                keyValue: "easycar_sync_enabled",
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 2, 25, 1, 4, 32, 322, DateTimeKind.Utc).AddTicks(4801), new DateTime(2026, 2, 25, 1, 4, 32, 322, DateTimeKind.Utc).AddTicks(4804) });
        }
    }
}
