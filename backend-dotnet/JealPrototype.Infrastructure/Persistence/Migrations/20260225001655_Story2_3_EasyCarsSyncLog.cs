using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace JealPrototype.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Story2_3_EasyCarsSyncLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_easycars_sync_logs_dealership_dealership_id",
                table: "easycars_sync_logs");

            migrationBuilder.DropForeignKey(
                name: "FK_easycars_sync_logs_dealership_easycars_credentials_credenti~",
                table: "easycars_sync_logs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_easycars_sync_logs",
                table: "easycars_sync_logs");

            migrationBuilder.DropIndex(
                name: "idx_sync_logs_started_at",
                table: "easycars_sync_logs");

            migrationBuilder.DropIndex(
                name: "idx_sync_logs_sync_type",
                table: "easycars_sync_logs");

            migrationBuilder.DropIndex(
                name: "IX_easycars_sync_logs_credential_id",
                table: "easycars_sync_logs");

            migrationBuilder.DropCheckConstraint(
                name: "CK_sync_logs_completed_at",
                table: "easycars_sync_logs");

            migrationBuilder.DropCheckConstraint(
                name: "CK_sync_logs_status",
                table: "easycars_sync_logs");

            migrationBuilder.DropCheckConstraint(
                name: "CK_sync_logs_sync_direction",
                table: "easycars_sync_logs");

            migrationBuilder.DropCheckConstraint(
                name: "CK_sync_logs_sync_type",
                table: "easycars_sync_logs");

            migrationBuilder.DropColumn(
                name: "completed_at",
                table: "easycars_sync_logs");

            migrationBuilder.DropColumn(
                name: "credential_id",
                table: "easycars_sync_logs");

            migrationBuilder.DropColumn(
                name: "error_details",
                table: "easycars_sync_logs");

            migrationBuilder.DropColumn(
                name: "error_message",
                table: "easycars_sync_logs");

            migrationBuilder.DropColumn(
                name: "records_created",
                table: "easycars_sync_logs");

            migrationBuilder.DropColumn(
                name: "request_payload",
                table: "easycars_sync_logs");

            migrationBuilder.DropColumn(
                name: "response_summary",
                table: "easycars_sync_logs");

            migrationBuilder.DropColumn(
                name: "started_at",
                table: "easycars_sync_logs");

            migrationBuilder.DropColumn(
                name: "sync_direction",
                table: "easycars_sync_logs");

            migrationBuilder.DropColumn(
                name: "sync_type",
                table: "easycars_sync_logs");

            migrationBuilder.RenameTable(
                name: "easycars_sync_logs",
                newName: "easycar_sync_logs");

            migrationBuilder.RenameColumn(
                name: "records_updated",
                table: "easycar_sync_logs",
                newName: "items_succeeded");

            migrationBuilder.RenameColumn(
                name: "records_processed",
                table: "easycar_sync_logs",
                newName: "items_processed");

            migrationBuilder.RenameColumn(
                name: "records_failed",
                table: "easycar_sync_logs",
                newName: "items_failed");

            migrationBuilder.RenameIndex(
                name: "idx_sync_logs_status",
                table: "easycar_sync_logs",
                newName: "idx_easycar_sync_logs_status");

            migrationBuilder.RenameIndex(
                name: "idx_sync_logs_dealership",
                table: "easycar_sync_logs",
                newName: "idx_easycar_sync_logs_dealership_id");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "easycar_sync_logs",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "NOW()");

            migrationBuilder.AddColumn<string>(
                name: "api_version",
                table: "easycar_sync_logs",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "1.0");

            migrationBuilder.AddColumn<long>(
                name: "duration_ms",
                table: "easycar_sync_logs",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "error_messages",
                table: "easycar_sync_logs",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<DateTime>(
                name: "synced_at",
                table: "easycar_sync_logs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "easycar_sync_logs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_easycar_sync_logs",
                table: "easycar_sync_logs",
                column: "id");

            migrationBuilder.CreateTable(
                name: "easycar_stock_data",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    vehicle_id = table.Column<int>(type: "integer", nullable: false),
                    stock_item_json = table.Column<string>(type: "jsonb", nullable: false),
                    synced_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    api_version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "1.0"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_easycar_stock_data", x => x.id);
                    table.ForeignKey(
                        name: "FK_easycar_stock_data_vehicle_vehicle_id",
                        column: x => x.vehicle_id,
                        principalTable: "vehicle",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_easycar_sync_logs_synced_at",
                table: "easycar_sync_logs",
                column: "synced_at");

            migrationBuilder.CreateIndex(
                name: "idx_easycar_stock_data_synced_at",
                table: "easycar_stock_data",
                column: "synced_at");

            migrationBuilder.CreateIndex(
                name: "idx_easycar_stock_data_vehicle_id_unique",
                table: "easycar_stock_data",
                column: "vehicle_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_easycar_sync_logs_dealership_dealership_id",
                table: "easycar_sync_logs",
                column: "dealership_id",
                principalTable: "dealership",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_easycar_sync_logs_dealership_dealership_id",
                table: "easycar_sync_logs");

            migrationBuilder.DropTable(
                name: "easycar_stock_data");

            migrationBuilder.DropPrimaryKey(
                name: "PK_easycar_sync_logs",
                table: "easycar_sync_logs");

            migrationBuilder.DropIndex(
                name: "idx_easycar_sync_logs_synced_at",
                table: "easycar_sync_logs");

            migrationBuilder.DropColumn(
                name: "api_version",
                table: "easycar_sync_logs");

            migrationBuilder.DropColumn(
                name: "duration_ms",
                table: "easycar_sync_logs");

            migrationBuilder.DropColumn(
                name: "error_messages",
                table: "easycar_sync_logs");

            migrationBuilder.DropColumn(
                name: "synced_at",
                table: "easycar_sync_logs");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "easycar_sync_logs");

            migrationBuilder.RenameTable(
                name: "easycar_sync_logs",
                newName: "easycars_sync_logs");

            migrationBuilder.RenameColumn(
                name: "items_succeeded",
                table: "easycars_sync_logs",
                newName: "records_updated");

            migrationBuilder.RenameColumn(
                name: "items_processed",
                table: "easycars_sync_logs",
                newName: "records_processed");

            migrationBuilder.RenameColumn(
                name: "items_failed",
                table: "easycars_sync_logs",
                newName: "records_failed");

            migrationBuilder.RenameIndex(
                name: "idx_easycar_sync_logs_status",
                table: "easycars_sync_logs",
                newName: "idx_sync_logs_status");

            migrationBuilder.RenameIndex(
                name: "idx_easycar_sync_logs_dealership_id",
                table: "easycars_sync_logs",
                newName: "idx_sync_logs_dealership");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "easycars_sync_logs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<DateTime>(
                name: "completed_at",
                table: "easycars_sync_logs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "credential_id",
                table: "easycars_sync_logs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "error_details",
                table: "easycars_sync_logs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "error_message",
                table: "easycars_sync_logs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "records_created",
                table: "easycars_sync_logs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "request_payload",
                table: "easycars_sync_logs",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "response_summary",
                table: "easycars_sync_logs",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "started_at",
                table: "easycars_sync_logs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<string>(
                name: "sync_direction",
                table: "easycars_sync_logs",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "sync_type",
                table: "easycars_sync_logs",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_easycars_sync_logs",
                table: "easycars_sync_logs",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "idx_sync_logs_started_at",
                table: "easycars_sync_logs",
                column: "started_at",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "idx_sync_logs_sync_type",
                table: "easycars_sync_logs",
                column: "sync_type");

            migrationBuilder.CreateIndex(
                name: "IX_easycars_sync_logs_credential_id",
                table: "easycars_sync_logs",
                column: "credential_id");

            migrationBuilder.AddCheckConstraint(
                name: "CK_sync_logs_completed_at",
                table: "easycars_sync_logs",
                sql: "completed_at IS NULL OR completed_at >= started_at");

            migrationBuilder.AddCheckConstraint(
                name: "CK_sync_logs_status",
                table: "easycars_sync_logs",
                sql: "status IN ('Success', 'Failed', 'Warning', 'InProgress')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_sync_logs_sync_direction",
                table: "easycars_sync_logs",
                sql: "sync_direction IN ('Inbound', 'Outbound')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_sync_logs_sync_type",
                table: "easycars_sync_logs",
                sql: "sync_type IN ('Stock', 'Lead')");

            migrationBuilder.AddForeignKey(
                name: "FK_easycars_sync_logs_dealership_dealership_id",
                table: "easycars_sync_logs",
                column: "dealership_id",
                principalTable: "dealership",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_easycars_sync_logs_dealership_easycars_credentials_credenti~",
                table: "easycars_sync_logs",
                column: "credential_id",
                principalTable: "dealership_easycars_credentials",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
