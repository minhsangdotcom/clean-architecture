using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRegions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "created_by",
                table: "province",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "updated_at",
                table: "province",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_by",
                table: "province",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                table: "district",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "updated_at",
                table: "district",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_by",
                table: "district",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                table: "commune",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "updated_at",
                table: "commune",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_by",
                table: "commune",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_by",
                table: "province");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "province");

            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "province");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "district");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "district");

            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "district");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "commune");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "commune");

            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "commune");
        }
    }
}
