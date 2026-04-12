using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IronLogic.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddCommunicationHistory : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "CommunicationHistories",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                DateCreated = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                DateModified = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                UserId = table.Column<string>(type: "TEXT", nullable: false),
                Subject = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                Body = table.Column<string>(type: "TEXT", nullable: false),
                SentAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                Status = table.Column<string>(type: "TEXT", nullable: false),
                Type = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CommunicationHistories", x => x.Id);
                table.ForeignKey(
                    name: "FK_CommunicationHistories_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_CommunicationHistories_UserId",
            table: "CommunicationHistories",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_CommunicationHistories_SentAt",
            table: "CommunicationHistories",
            column: "SentAt");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "CommunicationHistories");
    }
}
