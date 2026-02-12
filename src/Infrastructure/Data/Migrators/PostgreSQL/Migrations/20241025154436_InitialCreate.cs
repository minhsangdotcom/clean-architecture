using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:citext", ",,");

            migrationBuilder.CreateTable(
                name: "province",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(26)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    name_en = table.Column<string>(type: "text", nullable: false),
                    full_name = table.Column<string>(type: "text", nullable: false),
                    full_name_en = table.Column<string>(type: "text", nullable: false),
                    custom_name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_province", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "role",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(26)", nullable: false),
                    guard = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    name = table.Column<string>(type: "citext", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "district",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(26)", nullable: false),
                    province_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    name_en = table.Column<string>(type: "text", nullable: false),
                    full_name = table.Column<string>(type: "text", nullable: false),
                    full_name_en = table.Column<string>(type: "text", nullable: false),
                    custom_name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_district", x => x.id);
                    table.ForeignKey(
                        name: "fk_district_province_province_id",
                        column: x => x.province_id,
                        principalTable: "province",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "role_claim",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(26)", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: false),
                    claim_value = table.Column<string>(type: "text", nullable: false),
                    role_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role_claim", x => x.id);
                    table.ForeignKey(
                        name: "fk_role_claim_role_role_id",
                        column: x => x.role_id,
                        principalTable: "role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "commune",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(26)", nullable: false),
                    district_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    name_en = table.Column<string>(type: "text", nullable: false),
                    full_name = table.Column<string>(type: "text", nullable: false),
                    full_name_en = table.Column<string>(type: "text", nullable: false),
                    custom_name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_commune", x => x.id);
                    table.ForeignKey(
                        name: "fk_commune_district_district_id",
                        column: x => x.district_id,
                        principalTable: "district",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(26)", nullable: false),
                    first_name = table.Column<string>(type: "text", nullable: false),
                    last_name = table.Column<string>(type: "text", nullable: false),
                    user_name = table.Column<string>(type: "citext", nullable: false),
                    password = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "citext", nullable: false),
                    phone_number = table.Column<string>(type: "text", nullable: false),
                    day_of_birth = table.Column<DateTime>(type: "date", nullable: true),
                    gender = table.Column<int>(type: "integer", nullable: true),
                    address_street = table.Column<string>(type: "text", nullable: true),
                    address_province_id = table.Column<string>(type: "character varying(26)", nullable: true),
                    address_district_id = table.Column<string>(type: "character varying(26)", nullable: true),
                    address_commune_id = table.Column<string>(type: "character varying(26)", nullable: true),
                    avatar = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<byte>(type: "smallint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_commune_address_commune_id",
                        column: x => x.address_commune_id,
                        principalTable: "commune",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_user_district_address_district_id",
                        column: x => x.address_district_id,
                        principalTable: "district",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_user_province_address_province_id",
                        column: x => x.address_province_id,
                        principalTable: "province",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "user_claim",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(26)", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: false),
                    claim_value = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    role_claim_id = table.Column<string>(type: "character varying(26)", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_claim", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_claim_role_claim_role_claim_id",
                        column: x => x.role_claim_id,
                        principalTable: "role_claim",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_user_claim_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_reset_password",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(26)", nullable: false),
                    token = table.Column<string>(type: "text", nullable: false),
                    expiry = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    user_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_reset_password", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_reset_password_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_role",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    role_id = table.Column<string>(type: "character varying(26)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_role", x => new { x.role_id, x.user_id });
                    table.ForeignKey(
                        name: "fk_user_role_role_role_id",
                        column: x => x.role_id,
                        principalTable: "role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_role_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_token",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(26)", nullable: false),
                    refresh_token = table.Column<string>(type: "text", nullable: true),
                    client_ip = table.Column<string>(type: "text", nullable: true),
                    user_agent = table.Column<string>(type: "text", nullable: true),
                    family_id = table.Column<string>(type: "text", nullable: true),
                    is_blocked = table.Column<bool>(type: "boolean", nullable: false),
                    user_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    expired_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_token", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_token_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_commune_code",
                table: "commune",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_commune_district_id",
                table: "commune",
                column: "district_id");

            migrationBuilder.CreateIndex(
                name: "ix_district_code",
                table: "district",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_district_province_id",
                table: "district",
                column: "province_id");

            migrationBuilder.CreateIndex(
                name: "ix_province_code",
                table: "province",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_role_name",
                table: "role",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_role_claim_role_id",
                table: "role_claim",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_address_commune_id",
                table: "user",
                column: "address_commune_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_address_district_id",
                table: "user",
                column: "address_district_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_address_province_id",
                table: "user",
                column: "address_province_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_email",
                table: "user",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_id",
                table: "user",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_user_user_name",
                table: "user",
                column: "user_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_claim_role_claim_id",
                table: "user_claim",
                column: "role_claim_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_claim_user_id",
                table: "user_claim",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_reset_password_user_id",
                table: "user_reset_password",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_role_user_id",
                table: "user_role",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_token_user_id",
                table: "user_token",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_claim");

            migrationBuilder.DropTable(
                name: "user_reset_password");

            migrationBuilder.DropTable(
                name: "user_role");

            migrationBuilder.DropTable(
                name: "user_token");

            migrationBuilder.DropTable(
                name: "role_claim");

            migrationBuilder.DropTable(
                name: "user");

            migrationBuilder.DropTable(
                name: "role");

            migrationBuilder.DropTable(
                name: "commune");

            migrationBuilder.DropTable(
                name: "district");

            migrationBuilder.DropTable(
                name: "province");
        }
    }
}
