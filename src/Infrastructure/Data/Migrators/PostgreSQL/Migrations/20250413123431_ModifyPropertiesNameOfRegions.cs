using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ModifyPropertiesNameOfRegions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "name_en",
                table: "province",
                newName: "english_name"
            );

            migrationBuilder.RenameColumn(
                name: "full_name_en",
                table: "province",
                newName: "english_full_name"
            );

            migrationBuilder.RenameColumn(
                name: "name_en",
                table: "district",
                newName: "english_name"
            );

            migrationBuilder.RenameColumn(
                name: "full_name_en",
                table: "district",
                newName: "english_full_name"
            );

            migrationBuilder.RenameColumn(
                name: "name_en",
                table: "commune",
                newName: "english_name"
            );

            migrationBuilder.RenameColumn(
                name: "full_name_en",
                table: "commune",
                newName: "english_full_name"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "english_name",
                table: "province",
                newName: "name_en"
            );

            migrationBuilder.RenameColumn(
                name: "english_full_name",
                table: "province",
                newName: "full_name_en"
            );

            migrationBuilder.RenameColumn(
                name: "english_name",
                table: "district",
                newName: "name_en"
            );

            migrationBuilder.RenameColumn(
                name: "english_full_name",
                table: "district",
                newName: "full_name_en"
            );

            migrationBuilder.RenameColumn(
                name: "english_name",
                table: "commune",
                newName: "name_en"
            );

            migrationBuilder.RenameColumn(
                name: "english_full_name",
                table: "commune",
                newName: "full_name_en"
            );
        }
    }
}
