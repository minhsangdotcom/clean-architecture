using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeUserNameAndEmailFromTextToCiText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "username",
                table: "user",
                type: "citext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text"
            );

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "user",
                type: "citext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "username",
                table: "user",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "citext"
            );

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "user",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "citext"
            );
        }
    }
}
