using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemovePropertiesInPermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "effective_from", table: "permission");

            migrationBuilder.DropColumn(name: "effective_to", table: "permission");

            migrationBuilder.DropColumn(name: "is_deleted", table: "permission");

            migrationBuilder.DropColumn(name: "status", table: "permission");

            migrationBuilder.DropColumn(name: "version", table: "permission");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "effective_from",
                table: "permission",
                type: "timestamp with time zone",
                nullable: true
            );

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "effective_to",
                table: "permission",
                type: "timestamp with time zone",
                nullable: true
            );

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "permission",
                type: "boolean",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "permission",
                type: "integer",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AddColumn<long>(
                name: "version",
                table: "permission",
                type: "bigint",
                nullable: false,
                defaultValue: 0L
            );
        }
    }
}
