using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IronLogic.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDefaultUserSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseSessions_Exercises_ExerciseId1",
                table: "ExerciseSessions");

            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_ExerciseSessions_ExerciseId1",
                table: "ExerciseSessions");

            migrationBuilder.DropColumn(
                name: "ExerciseId1",
                table: "ExerciseSessions");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "DateCreated", "DateModified", "Email", "PasswordHash", "Username" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), new DateTimeOffset(new DateTime(2026, 3, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 3, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "kohzadi90@gmail.com", null, "kohzadi90" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"));

            migrationBuilder.AddColumn<Guid>(
                name: "ExerciseId1",
                table: "ExerciseSessions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseSessions_ExerciseId1",
                table: "ExerciseSessions",
                column: "ExerciseId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ExerciseSessions_Exercises_ExerciseId1",
                table: "ExerciseSessions",
                column: "ExerciseId1",
                principalTable: "Exercises",
                principalColumn: "Id");
        }
    }
}
