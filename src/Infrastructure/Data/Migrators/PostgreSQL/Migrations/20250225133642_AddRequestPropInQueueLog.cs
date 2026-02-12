using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestPropInQueueLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "dead_letter_queue");

            migrationBuilder.CreateTable(
                name: "queue_log",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(26)", nullable: false),
                    request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    request = table.Column<object>(type: "jsonb", nullable: true),
                    error_detail = table.Column<object>(type: "jsonb", nullable: true),
                    retry_count = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("pk_queue_log", x => x.id);
                }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "queue_log");

            migrationBuilder.CreateTable(
                name: "dead_letter_queue",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(26)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    error_detail = table.Column<object>(type: "jsonb", nullable: true),
                    request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    retry_count = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dead_letter_queue", x => x.id);
                }
            );
        }
    }
}
