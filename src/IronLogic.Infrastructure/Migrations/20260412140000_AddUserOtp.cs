using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IronLogic.Infrastructure.Migrations;

/// <summary>
/// Creates the UserOtps table used for email-address verification OTP storage.
/// </summary>
public partial class AddUserOtp : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "UserOtps",
            columns: table => new
            {
                Id = table.Column<string>(type: "TEXT", nullable: false),
                UserId = table.Column<string>(type: "TEXT", nullable: false),
                Code = table.Column<string>(type: "TEXT", maxLength: 6, nullable: false),
                Token = table.Column<string>(type: "TEXT", nullable: false),
                ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                IsUsed = table.Column<bool>(type: "INTEGER", nullable: false),
                DateCreated = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                DateModified = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserOtps", x => x.Id);
                table.ForeignKey(
                    name: "FK_UserOtps_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_UserOtps_UserId",
            table: "UserOtps",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_UserOtps_UserId_Code",
            table: "UserOtps",
            columns: new[] { "UserId", "Code" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "UserOtps");
    }
}
