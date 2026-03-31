using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IronLogic.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkoutTrackingEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_Muscles_PrimaryMuscleId",
                table: "Exercises");

            migrationBuilder.AlterColumn<Guid>(
                name: "PrimaryMuscleId",
                table: "Exercises",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_Muscles_PrimaryMuscleId",
                table: "Exercises",
                column: "PrimaryMuscleId",
                principalTable: "Muscles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_Muscles_PrimaryMuscleId",
                table: "Exercises");

            migrationBuilder.AlterColumn<Guid>(
                name: "PrimaryMuscleId",
                table: "Exercises",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_Muscles_PrimaryMuscleId",
                table: "Exercises",
                column: "PrimaryMuscleId",
                principalTable: "Muscles",
                principalColumn: "Id");
        }
    }
}
