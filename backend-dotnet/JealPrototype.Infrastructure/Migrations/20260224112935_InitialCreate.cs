using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace JealPrototype.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "dealership",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    logo_url = table.Column<string>(type: "text", nullable: true),
                    address = table.Column<string>(type: "text", nullable: false),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    hours = table.Column<string>(type: "text", nullable: true),
                    about = table.Column<string>(type: "text", nullable: true),
                    website_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    finance_policy = table.Column<string>(type: "text", nullable: true),
                    warranty_policy = table.Column<string>(type: "text", nullable: true),
                    hero_background_image = table.Column<string>(type: "text", nullable: true),
                    hero_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    hero_video_url = table.Column<string>(type: "text", nullable: true),
                    hero_carousel_images = table.Column<List<string>>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    hero_title = table.Column<string>(type: "text", nullable: true),
                    hero_subtitle = table.Column<string>(type: "text", nullable: true),
                    theme_color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    secondary_theme_color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    body_background_color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    font_family = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    navigation_config = table.Column<string>(type: "jsonb", nullable: true),
                    facebook_url = table.Column<string>(type: "text", nullable: true),
                    instagram_url = table.Column<string>(type: "text", nullable: true),
                    finance_promo_image = table.Column<string>(type: "text", nullable: true),
                    finance_promo_text = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    warranty_promo_image = table.Column<string>(type: "text", nullable: true),
                    warranty_promo_text = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dealership", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "app_user",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    full_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    user_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    dealership_id = table.Column<int>(type: "integer", nullable: true),
                    permissions = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    created_by = table.Column<int>(type: "integer", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_app_user", x => x.id);
                    table.ForeignKey(
                        name: "FK_app_user_dealership_dealership_id",
                        column: x => x.dealership_id,
                        principalTable: "dealership",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "blog",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    dealership_id = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    image_url = table.Column<string>(type: "text", nullable: true),
                    published_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    author = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_blog", x => x.id);
                    table.ForeignKey(
                        name: "FK_blog_dealership_dealership_id",
                        column: x => x.dealership_id,
                        principalTable: "dealership",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "blog_post",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    dealership_id = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    slug = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    excerpt = table.Column<string>(type: "text", nullable: true),
                    featured_image_url = table.Column<string>(type: "text", nullable: true),
                    author_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    published_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_blog_post", x => x.id);
                    table.ForeignKey(
                        name: "FK_blog_post_dealership_dealership_id",
                        column: x => x.dealership_id,
                        principalTable: "dealership",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "dealership_easycars_credentials",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    dealership_id = table.Column<int>(type: "integer", nullable: false),
                    account_number_encrypted = table.Column<string>(type: "text", nullable: false),
                    account_secret_encrypted = table.Column<string>(type: "text", nullable: false),
                    encryption_iv = table.Column<string>(type: "text", nullable: false),
                    environment = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    yard_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    last_synced_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dealership_easycars_credentials", x => x.id);
                    table.CheckConstraint("CK_easycars_credentials_environment", "environment IN ('Test', 'Production')");
                    table.ForeignKey(
                        name: "FK_dealership_easycars_credentials_dealership_dealership_id",
                        column: x => x.dealership_id,
                        principalTable: "dealership",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "design_templates",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    dealership_id = table.Column<int>(type: "integer", nullable: true),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    theme_color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    secondary_theme_color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    body_background_color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    font_family = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_preset = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_design_templates", x => x.id);
                    table.ForeignKey(
                        name: "FK_design_templates_dealership_dealership_id",
                        column: x => x.dealership_id,
                        principalTable: "dealership",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hero_media",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    dealership_id = table.Column<int>(type: "integer", nullable: false),
                    media_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    media_url = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hero_media", x => x.id);
                    table.ForeignKey(
                        name: "FK_hero_media_dealership_dealership_id",
                        column: x => x.dealership_id,
                        principalTable: "dealership",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "promotional_panel",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    dealership_id = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    image_url = table.Column<string>(type: "text", nullable: true),
                    link_url = table.Column<string>(type: "text", nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promotional_panel", x => x.id);
                    table.ForeignKey(
                        name: "FK_promotional_panel_dealership_dealership_id",
                        column: x => x.dealership_id,
                        principalTable: "dealership",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sales_request",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    dealership_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    make = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    kilometers = table.Column<int>(type: "integer", nullable: false),
                    additional_message = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sales_request", x => x.id);
                    table.ForeignKey(
                        name: "FK_sales_request_dealership_dealership_id",
                        column: x => x.dealership_id,
                        principalTable: "dealership",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vehicle",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    dealership_id = table.Column<int>(type: "integer", nullable: false),
                    make = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    mileage = table.Column<int>(type: "integer", nullable: false),
                    condition = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    images = table.Column<List<string>>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    easycars_stock_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    easycars_yard_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    easycars_vin = table.Column<string>(type: "character varying(17)", maxLength: 17, nullable: true),
                    easycars_raw_data = table.Column<string>(type: "jsonb", nullable: true),
                    data_source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Manual"),
                    last_synced_from_easycars = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    exterior_color = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    interior_color = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    body = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    fuel_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    gear_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    engine_capacity = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    door_count = table.Column<int>(type: "integer", nullable: true),
                    features = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle", x => x.id);
                    table.CheckConstraint("CK_vehicle_data_source", "data_source IN ('Manual', 'EasyCars', 'Import')");
                    table.ForeignKey(
                        name: "FK_vehicle_dealership_dealership_id",
                        column: x => x.dealership_id,
                        principalTable: "dealership",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "easycars_sync_logs",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    dealership_id = table.Column<int>(type: "integer", nullable: false),
                    credential_id = table.Column<int>(type: "integer", nullable: true),
                    sync_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    sync_direction = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    records_processed = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    records_created = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    records_updated = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    records_failed = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    error_details = table.Column<string>(type: "text", nullable: true),
                    request_payload = table.Column<string>(type: "jsonb", nullable: true),
                    response_summary = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_easycars_sync_logs", x => x.id);
                    table.CheckConstraint("CK_sync_logs_completed_at", "completed_at IS NULL OR completed_at >= started_at");
                    table.CheckConstraint("CK_sync_logs_status", "status IN ('Success', 'Failed', 'Warning', 'InProgress')");
                    table.CheckConstraint("CK_sync_logs_sync_direction", "sync_direction IN ('Inbound', 'Outbound')");
                    table.CheckConstraint("CK_sync_logs_sync_type", "sync_type IN ('Stock', 'Lead')");
                    table.ForeignKey(
                        name: "FK_easycars_sync_logs_dealership_dealership_id",
                        column: x => x.dealership_id,
                        principalTable: "dealership",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_easycars_sync_logs_dealership_easycars_credentials_credenti~",
                        column: x => x.credential_id,
                        principalTable: "dealership_easycars_credentials",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "lead",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    dealership_id = table.Column<int>(type: "integer", nullable: false),
                    vehicle_id = table.Column<int>(type: "integer", nullable: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    easycars_lead_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    easycars_customer_no = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    easycars_raw_data = table.Column<string>(type: "jsonb", nullable: true),
                    data_source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Manual"),
                    last_synced_to_easycars = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_synced_from_easycars = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    vehicle_interest_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    finance_interested = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    rating = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lead", x => x.id);
                    table.CheckConstraint("CK_lead_data_source", "data_source IN ('Manual', 'EasyCars', 'WebForm')");
                    table.ForeignKey(
                        name: "FK_lead_dealership_dealership_id",
                        column: x => x.dealership_id,
                        principalTable: "dealership",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_lead_vehicle_vehicle_id",
                        column: x => x.vehicle_id,
                        principalTable: "vehicle",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "idx_app_user_dealership_id",
                table: "app_user",
                column: "dealership_id");

            migrationBuilder.CreateIndex(
                name: "idx_app_user_username",
                table: "app_user",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_blog_dealership_id",
                table: "blog",
                column: "dealership_id");

            migrationBuilder.CreateIndex(
                name: "idx_blog_post_dealership_id",
                table: "blog_post",
                column: "dealership_id");

            migrationBuilder.CreateIndex(
                name: "idx_blog_post_slug",
                table: "blog_post",
                column: "slug");

            migrationBuilder.CreateIndex(
                name: "IX_blog_post_dealership_id_slug",
                table: "blog_post",
                columns: new[] { "dealership_id", "slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_dealership_website_url",
                table: "dealership",
                column: "website_url",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_easycars_credentials_active",
                table: "dealership_easycars_credentials",
                column: "is_active",
                filter: "is_active = true");

            migrationBuilder.CreateIndex(
                name: "idx_easycars_credentials_dealership",
                table: "dealership_easycars_credentials",
                column: "dealership_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_design_templates_dealership_id",
                table: "design_templates",
                column: "dealership_id");

            migrationBuilder.CreateIndex(
                name: "idx_sync_logs_dealership",
                table: "easycars_sync_logs",
                column: "dealership_id");

            migrationBuilder.CreateIndex(
                name: "idx_sync_logs_started_at",
                table: "easycars_sync_logs",
                column: "started_at",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "idx_sync_logs_status",
                table: "easycars_sync_logs",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_sync_logs_sync_type",
                table: "easycars_sync_logs",
                column: "sync_type");

            migrationBuilder.CreateIndex(
                name: "IX_easycars_sync_logs_credential_id",
                table: "easycars_sync_logs",
                column: "credential_id");

            migrationBuilder.CreateIndex(
                name: "IX_hero_media_dealership_id",
                table: "hero_media",
                column: "dealership_id");

            migrationBuilder.CreateIndex(
                name: "idx_lead_created_at",
                table: "lead",
                column: "created_at",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "idx_lead_dealership_id",
                table: "lead",
                column: "dealership_id");

            migrationBuilder.CreateIndex(
                name: "idx_leads_data_source",
                table: "lead",
                column: "data_source");

            migrationBuilder.CreateIndex(
                name: "idx_leads_easycars_lead",
                table: "lead",
                column: "easycars_lead_number",
                filter: "easycars_lead_number IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_lead_vehicle_id",
                table: "lead",
                column: "vehicle_id");

            migrationBuilder.CreateIndex(
                name: "IX_promotional_panel_dealership_id",
                table: "promotional_panel",
                column: "dealership_id");

            migrationBuilder.CreateIndex(
                name: "idx_sales_request_created_at",
                table: "sales_request",
                column: "created_at",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "idx_sales_request_dealership_id",
                table: "sales_request",
                column: "dealership_id");

            migrationBuilder.CreateIndex(
                name: "idx_vehicle_dealership_id",
                table: "vehicle",
                column: "dealership_id");

            migrationBuilder.CreateIndex(
                name: "idx_vehicle_dealership_status",
                table: "vehicle",
                columns: new[] { "dealership_id", "status" });

            migrationBuilder.CreateIndex(
                name: "idx_vehicle_status",
                table: "vehicle",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_vehicles_data_source",
                table: "vehicle",
                column: "data_source");

            migrationBuilder.CreateIndex(
                name: "idx_vehicles_easycars_stock",
                table: "vehicle",
                column: "easycars_stock_number",
                filter: "easycars_stock_number IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "idx_vehicles_easycars_vin",
                table: "vehicle",
                column: "easycars_vin",
                filter: "easycars_vin IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "app_user");

            migrationBuilder.DropTable(
                name: "blog");

            migrationBuilder.DropTable(
                name: "blog_post");

            migrationBuilder.DropTable(
                name: "design_templates");

            migrationBuilder.DropTable(
                name: "easycars_sync_logs");

            migrationBuilder.DropTable(
                name: "hero_media");

            migrationBuilder.DropTable(
                name: "lead");

            migrationBuilder.DropTable(
                name: "promotional_panel");

            migrationBuilder.DropTable(
                name: "sales_request");

            migrationBuilder.DropTable(
                name: "dealership_easycars_credentials");

            migrationBuilder.DropTable(
                name: "vehicle");

            migrationBuilder.DropTable(
                name: "dealership");
        }
    }
}
