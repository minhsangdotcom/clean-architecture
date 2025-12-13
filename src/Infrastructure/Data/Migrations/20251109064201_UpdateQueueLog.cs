using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateQueueLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "error_detail", table: "queue_log");

            migrationBuilder.DropColumn(name: "request", table: "queue_log");

            migrationBuilder.AddColumn<string>(
                name: "error_detail_json",
                table: "queue_log",
                type: "text",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "request_json",
                table: "queue_log",
                type: "text",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "response_json",
                table: "queue_log",
                type: "text",
                nullable: true
            );

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "queue_log",
                type: "integer",
                nullable: false,
                defaultValue: 0
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "error_detail_json", table: "queue_log");

            migrationBuilder.DropColumn(name: "request_json", table: "queue_log");

            migrationBuilder.DropColumn(name: "response_json", table: "queue_log");

            migrationBuilder.DropColumn(name: "status", table: "queue_log");

            migrationBuilder.AddColumn<object>(
                name: "error_detail",
                table: "queue_log",
                type: "jsonb",
                nullable: true
            );

            migrationBuilder.AddColumn<object>(
                name: "request",
                table: "queue_log",
                type: "jsonb",
                nullable: true
            );
        }
    }
}
