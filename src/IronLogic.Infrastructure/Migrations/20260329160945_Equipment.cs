using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IronLogic.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Equipment : Migration
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
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AddColumn<Guid>(
                name: "EquipmentId",
                table: "Exercises",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Equipment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    DateCreated = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    DateModified = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipment", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_EquipmentId",
                table: "Exercises",
                column: "EquipmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_Equipment_EquipmentId",
                table: "Exercises",
                column: "EquipmentId",
                principalTable: "Equipment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_Muscles_PrimaryMuscleId",
                table: "Exercises",
                column: "PrimaryMuscleId",
                principalTable: "Muscles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_Equipment_EquipmentId",
                table: "Exercises");

            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_Muscles_PrimaryMuscleId",
                table: "Exercises");

            migrationBuilder.DropTable(
                name: "Equipment");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_EquipmentId",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "EquipmentId",
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
    }
}
