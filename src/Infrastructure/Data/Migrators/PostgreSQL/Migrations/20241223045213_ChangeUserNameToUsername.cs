using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeUserNameToUsername : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(name: "user_name", table: "user", newName: "username");

            migrationBuilder.RenameIndex(
                name: "ix_user_user_name",
                table: "user",
                newName: "ix_user_username"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(name: "username", table: "user", newName: "user_name");

            migrationBuilder.RenameIndex(
                name: "ix_user_username",
                table: "user",
                newName: "ix_user_user_name"
            );
        }
    }
}
