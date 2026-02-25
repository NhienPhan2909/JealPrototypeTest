using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JealPrototype.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddClientIdClientSecretToEasyCarsCredentials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "client_id_encrypted",
                table: "dealership_easycars_credentials",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "client_secret_encrypted",
                table: "dealership_easycars_credentials",
                type: "text",
                nullable: false,
                defaultValue: "");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "client_id_encrypted",
                table: "dealership_easycars_credentials");

            migrationBuilder.DropColumn(
                name: "client_secret_encrypted",
                table: "dealership_easycars_credentials");

            migrationBuilder.UpdateData(
                table: "system_settings",
                keyColumn: "key",
                keyValue: "easycar_image_sync_enabled",
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 2, 25, 7, 35, 39, 965, DateTimeKind.Utc).AddTicks(1794), new DateTime(2026, 2, 25, 7, 35, 39, 965, DateTimeKind.Utc).AddTicks(1795) });

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
        }
    }
}
