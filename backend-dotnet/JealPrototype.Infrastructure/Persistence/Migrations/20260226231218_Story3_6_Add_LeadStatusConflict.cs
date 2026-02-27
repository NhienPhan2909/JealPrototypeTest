using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JealPrototype.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Story3_6_Add_LeadStatusConflict : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "system_settings",
                keyColumn: "key",
                keyValue: "easycar_image_sync_enabled",
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 2, 26, 23, 12, 17, 703, DateTimeKind.Utc).AddTicks(5981), new DateTime(2026, 2, 26, 23, 12, 17, 703, DateTimeKind.Utc).AddTicks(5981) });

            migrationBuilder.UpdateData(
                table: "system_settings",
                keyColumn: "key",
                keyValue: "easycar_sync_concurrency",
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 2, 26, 23, 12, 17, 703, DateTimeKind.Utc).AddTicks(5980), new DateTime(2026, 2, 26, 23, 12, 17, 703, DateTimeKind.Utc).AddTicks(5980) });

            migrationBuilder.UpdateData(
                table: "system_settings",
                keyColumn: "key",
                keyValue: "easycar_sync_cron",
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 2, 26, 23, 12, 17, 703, DateTimeKind.Utc).AddTicks(5979), new DateTime(2026, 2, 26, 23, 12, 17, 703, DateTimeKind.Utc).AddTicks(5979) });

            migrationBuilder.UpdateData(
                table: "system_settings",
                keyColumn: "key",
                keyValue: "easycar_sync_enabled",
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 2, 26, 23, 12, 17, 703, DateTimeKind.Utc).AddTicks(5975), new DateTime(2026, 2, 26, 23, 12, 17, 703, DateTimeKind.Utc).AddTicks(5978) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}
