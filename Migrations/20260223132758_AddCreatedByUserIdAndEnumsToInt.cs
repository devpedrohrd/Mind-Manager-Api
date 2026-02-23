using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mind_Manager.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedByUserIdAndEnumsToInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Users: Role varchar → integer
            migrationBuilder.Sql(
                @"ALTER TABLE ""Users"" ALTER COLUMN ""Role"" TYPE integer USING ""Role""::integer;");

            // PatientProfiles: enums simples varchar → integer
            migrationBuilder.Sql(
                @"ALTER TABLE ""PatientProfiles"" ALTER COLUMN ""PatientType"" TYPE integer USING ""PatientType""::integer;");
            migrationBuilder.Sql(
                @"ALTER TABLE ""PatientProfiles"" ALTER COLUMN ""Gender"" TYPE integer USING ""Gender""::integer;");
            migrationBuilder.Sql(
                @"ALTER TABLE ""PatientProfiles"" ALTER COLUMN ""Education"" TYPE integer USING ""Education""::integer;");
            migrationBuilder.Sql(
                @"ALTER TABLE ""PatientProfiles"" ALTER COLUMN ""CreatedBy"" TYPE integer USING ""CreatedBy""::integer;");
            migrationBuilder.Sql(
                @"ALTER TABLE ""PatientProfiles"" ALTER COLUMN ""Course"" TYPE integer USING ""Course""::integer;");

            // PatientProfiles: arrays de enums text[] → integer[]
            migrationBuilder.Sql(
                @"ALTER TABLE ""PatientProfiles"" ALTER COLUMN ""Disorders"" TYPE integer[] USING ""Disorders""::integer[];");
            migrationBuilder.Sql(
                @"ALTER TABLE ""PatientProfiles"" ALTER COLUMN ""Difficulties"" TYPE integer[] USING ""Difficulties""::integer[];");

            // Nova coluna: CreatedByUserId
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "PatientProfiles",
                type: "uuid",
                nullable: true);

            // Appointments: enums varchar → integer
            migrationBuilder.Sql(
                @"ALTER TABLE ""Appointments"" ALTER COLUMN ""Type"" TYPE integer USING ""Type""::integer;");
            migrationBuilder.Sql(
                @"ALTER TABLE ""Appointments"" ALTER COLUMN ""Status"" TYPE integer USING ""Status""::integer;");
            migrationBuilder.Sql(
                @"ALTER TABLE ""Appointments"" ALTER COLUMN ""ActivityType"" TYPE integer USING ""ActivityType""::integer;");

            migrationBuilder.CreateIndex(
                name: "IX_PatientProfiles_CreatedByUserId",
                table: "PatientProfiles",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PatientProfiles_Users_CreatedByUserId",
                table: "PatientProfiles",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PatientProfiles_Users_CreatedByUserId",
                table: "PatientProfiles");

            migrationBuilder.DropIndex(
                name: "IX_PatientProfiles_CreatedByUserId",
                table: "PatientProfiles");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "PatientProfiles");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "Users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "PatientType",
                table: "PatientProfiles",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Gender",
                table: "PatientProfiles",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Education",
                table: "PatientProfiles",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string[]>(
                name: "Disorders",
                table: "PatientProfiles",
                type: "text[]",
                nullable: false,
                oldClrType: typeof(int[]),
                oldType: "integer[]");

            migrationBuilder.AlterColumn<string[]>(
                name: "Difficulties",
                table: "PatientProfiles",
                type: "text[]",
                nullable: false,
                oldClrType: typeof(int[]),
                oldType: "integer[]");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "PatientProfiles",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Course",
                table: "PatientProfiles",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Appointments",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Appointments",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "ActivityType",
                table: "Appointments",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
