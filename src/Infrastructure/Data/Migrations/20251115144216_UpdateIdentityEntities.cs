using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIdentityEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_user_claim_role_claim_role_claim_id",
                table: "user_claim"
            );

            migrationBuilder.DropTable(name: "user_reset_password");

            migrationBuilder.DropTable(name: "user_token");

            migrationBuilder.DropIndex(name: "ix_user_claim_role_claim_id", table: "user_claim");

            migrationBuilder.DropIndex(name: "ix_user_email", table: "user");

            migrationBuilder.DropIndex(name: "ix_user_username", table: "user");

            migrationBuilder.DropIndex(name: "ix_role_name", table: "role");

            migrationBuilder.DropColumn(name: "role_claim_id", table: "user_claim");

            migrationBuilder.DropColumn(name: "type", table: "user_claim");

            migrationBuilder.DropColumn(name: "address_commune", table: "user");

            migrationBuilder.DropColumn(name: "address_commune_id", table: "user");

            migrationBuilder.DropColumn(name: "address_district", table: "user");

            migrationBuilder.DropColumn(name: "address_district_id", table: "user");

            migrationBuilder.DropColumn(name: "address_province", table: "user");

            migrationBuilder.DropColumn(name: "address_province_id", table: "user");

            migrationBuilder.DropColumn(name: "address_street", table: "user");

            migrationBuilder.RenameColumn(
                name: "day_of_birth",
                table: "user",
                newName: "date_of_birth"
            );

            migrationBuilder.DropColumn(name: "guard", table: "role");

            migrationBuilder.AddColumn<string>(
                name: "updated_by",
                table: "role",
                type: "text",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                table: "user_claim",
                type: "text",
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "updated_at",
                table: "user_claim",
                type: "timestamp with time zone",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "updated_by",
                table: "user_claim",
                type: "text",
                nullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "username",
                table: "user",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "citext"
            );

            migrationBuilder.AlterColumn<string>(
                name: "phone_number",
                table: "user",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text"
            );

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "user",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "citext"
            );

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "role",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "citext"
            );

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                table: "role",
                type: "text",
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "updated_at",
                table: "role",
                type: "timestamp with time zone",
                nullable: true
            );

            migrationBuilder.AddColumn<long>(
                name: "version",
                table: "role",
                type: "bigint",
                nullable: false,
                defaultValue: 0L
            );

            migrationBuilder.CreateTable(
                name: "permission",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(26)", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    group = table.Column<string>(type: "text", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    effective_from = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    effective_to = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    version = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_permission", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "user_password_reset",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(26)", nullable: false),
                    token = table.Column<string>(type: "text", nullable: false),
                    expiry = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    user_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_password_reset", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_password_reset_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "user_refresh_token",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(26)", nullable: false),
                    token = table.Column<string>(type: "text", nullable: true),
                    client_ip = table.Column<string>(type: "text", nullable: true),
                    user_agent = table.Column<string>(type: "text", nullable: true),
                    family_id = table.Column<string>(type: "text", nullable: true),
                    is_blocked = table.Column<bool>(type: "boolean", nullable: false),
                    user_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    expired_time = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    created_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_refresh_token", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_refresh_token_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "role_permission",
                columns: table => new
                {
                    role_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    permission_id = table.Column<string>(
                        type: "character varying(26)",
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role_permission", x => new { x.permission_id, x.role_id });
                    table.ForeignKey(
                        name: "fk_role_permission_permission_permission_id",
                        column: x => x.permission_id,
                        principalTable: "permission",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "fk_role_permission_role_role_id",
                        column: x => x.role_id,
                        principalTable: "role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "user_permission",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    permission_id = table.Column<string>(
                        type: "character varying(26)",
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_permission", x => new { x.permission_id, x.user_id });
                    table.ForeignKey(
                        name: "fk_user_permission_permission_permission_id",
                        column: x => x.permission_id,
                        principalTable: "permission",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "fk_user_permission_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "ix_role_permission_role_id",
                table: "role_permission",
                column: "role_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_user_password_reset_user_id",
                table: "user_password_reset",
                column: "user_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_user_permission_user_id",
                table: "user_permission",
                column: "user_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_user_refresh_token_user_id",
                table: "user_refresh_token",
                column: "user_id"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "role_permission");

            migrationBuilder.DropTable(name: "user_password_reset");

            migrationBuilder.DropTable(name: "user_permission");

            migrationBuilder.DropTable(name: "user_refresh_token");

            migrationBuilder.DropTable(name: "permission");

            migrationBuilder.DropColumn(name: "created_by", table: "user_claim");

            migrationBuilder.DropColumn(name: "updated_at", table: "user_claim");

            migrationBuilder.DropColumn(name: "updated_by", table: "user_claim");

            migrationBuilder.DropColumn(name: "created_by", table: "role");

            migrationBuilder.DropColumn(name: "updated_at", table: "role");

            migrationBuilder.DropColumn(name: "version", table: "role");

            migrationBuilder.RenameColumn(
                name: "date_of_birth",
                table: "user",
                newName: "day_of_birth"
            );

            migrationBuilder.DropColumn(name: "updated_by", table: "role");

            migrationBuilder.AddColumn<string>(
                name: "guard",
                table: "role",
                type: "text",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "role_claim_id",
                table: "user_claim",
                type: "character varying(26)",
                nullable: true
            );

            migrationBuilder.AddColumn<int>(
                name: "type",
                table: "user_claim",
                type: "integer",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AlterColumn<string>(
                name: "username",
                table: "user",
                type: "citext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text"
            );

            migrationBuilder.AlterColumn<string>(
                name: "phone_number",
                table: "user",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "user",
                type: "citext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text"
            );

            migrationBuilder.AddColumn<string>(
                name: "address_commune",
                table: "user",
                type: "text",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "address_commune_id",
                table: "user",
                type: "character varying(26)",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "address_district",
                table: "user",
                type: "text",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "address_district_id",
                table: "user",
                type: "character varying(26)",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "address_province",
                table: "user",
                type: "text",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "address_province_id",
                table: "user",
                type: "character varying(26)",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "address_street",
                table: "user",
                type: "text",
                nullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "role",
                type: "citext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text"
            );

            migrationBuilder.CreateTable(
                name: "user_reset_password",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(26)", nullable: false),
                    user_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    expiry = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    token = table.Column<string>(type: "text", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_reset_password", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_reset_password_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "user_token",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(26)", nullable: false),
                    user_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    client_ip = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    expired_time = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    family_id = table.Column<string>(type: "text", nullable: true),
                    is_blocked = table.Column<bool>(type: "boolean", nullable: false),
                    refresh_token = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    user_agent = table.Column<string>(type: "text", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_token", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_token_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "ix_user_claim_role_claim_id",
                table: "user_claim",
                column: "role_claim_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_user_email",
                table: "user",
                column: "email",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_user_username",
                table: "user",
                column: "username",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_role_name",
                table: "role",
                column: "name",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_user_reset_password_user_id",
                table: "user_reset_password",
                column: "user_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_user_token_user_id",
                table: "user_token",
                column: "user_id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_user_claim_role_claim_role_claim_id",
                table: "user_claim",
                column: "role_claim_id",
                principalTable: "role_claim",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade
            );
        }
    }
}
