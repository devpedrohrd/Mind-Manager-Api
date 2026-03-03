using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mind_Manager.Migrations
{
    /// <inheritdoc />
    public partial class AddStateMachineAndDataIsolation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByPsychologistId",
                table: "Anamneses",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Anamneses_CreatedByPsychologistId",
                table: "Anamneses",
                column: "CreatedByPsychologistId");

            migrationBuilder.AddForeignKey(
                name: "FK_Anamneses_PsychologistProfiles_CreatedByPsychologistId",
                table: "Anamneses",
                column: "CreatedByPsychologistId",
                principalTable: "PsychologistProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Anamneses_PsychologistProfiles_CreatedByPsychologistId",
                table: "Anamneses");

            migrationBuilder.DropIndex(
                name: "IX_Anamneses_CreatedByPsychologistId",
                table: "Anamneses");

            migrationBuilder.DropColumn(
                name: "CreatedByPsychologistId",
                table: "Anamneses");
        }
    }
}
