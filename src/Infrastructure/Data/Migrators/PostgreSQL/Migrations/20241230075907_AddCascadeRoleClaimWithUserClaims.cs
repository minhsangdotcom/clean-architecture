using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCascadeRoleClaimWithUserClaims : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_user_claim_role_claim_role_claim_id",
                table: "user_claim"
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_user_claim_role_claim_role_claim_id",
                table: "user_claim"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_user_claim_role_claim_role_claim_id",
                table: "user_claim",
                column: "role_claim_id",
                principalTable: "role_claim",
                principalColumn: "id"
            );
        }
    }
}
