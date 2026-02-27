using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JealPrototype.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Story3_5_SyncLog_SyncType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "sync_type",
                table: "easycar_sync_logs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "sync_type",
                table: "easycar_sync_logs");

            migrationBuilder.UpdateData(
                table: "system_settings",
                keyColumn: "key",
                keyValue: "easycar_image_sync_enabled",
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 2, 25, 13, 17, 20, 936, DateTimeKind.Utc).AddTicks(8866), new DateTime(2026, 2, 25, 13, 17, 20, 936, DateTimeKind.Utc).AddTicks(8866) });

            migrationBuilder.UpdateData(
                table: "system_settings",
                keyColumn: "key",
                keyValue: "easycar_sync_concurrency",
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 2, 25, 13, 17, 20, 936, DateTimeKind.Utc).AddTicks(8865), new DateTime(2026, 2, 25, 13, 17, 20, 936, DateTimeKind.Utc).AddTicks(8865) });

            migrationBuilder.UpdateData(
                table: "system_settings",
                keyColumn: "key",
                keyValue: "easycar_sync_cron",
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 2, 25, 13, 17, 20, 936, DateTimeKind.Utc).AddTicks(8864), new DateTime(2026, 2, 25, 13, 17, 20, 936, DateTimeKind.Utc).AddTicks(8864) });

            migrationBuilder.UpdateData(
                table: "system_settings",
                keyColumn: "key",
                keyValue: "easycar_sync_enabled",
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 2, 25, 13, 17, 20, 936, DateTimeKind.Utc).AddTicks(8861), new DateTime(2026, 2, 25, 13, 17, 20, 936, DateTimeKind.Utc).AddTicks(8863) });
        }
    }
}
