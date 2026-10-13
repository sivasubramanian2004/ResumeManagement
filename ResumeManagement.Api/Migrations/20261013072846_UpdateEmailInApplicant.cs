using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResumeManagement.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEmailInApplicant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Applicants_Email",
                table: "Applicants");

            migrationBuilder.CreateIndex(
                name: "IX_Applicants_Email",
                table: "Applicants",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Applicants_Email",
                table: "Applicants");

            migrationBuilder.CreateIndex(
                name: "IX_Applicants_Email",
                table: "Applicants",
                column: "Email",
                unique: true,
                filter: "[IsDeleted] = 0");
        }
    }
}
