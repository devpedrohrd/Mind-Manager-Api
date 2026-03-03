using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mind_Manager.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:ActivityType", "Group,Lecture,Seminar,Meeting,DiscussionCircle")
                .Annotation("Npgsql:Enum:Courses", "Fisica,Quimica,Ads,Eletrotecnica,Administracao,Informatica")
                .Annotation("Npgsql:Enum:CreatedBy", "Patient,Psychologist")
                .Annotation("Npgsql:Enum:Difficulty", "Avaliation,OrganizationOnStudies,Concentration,Memory,Tdah,Comunication,Relationship,Other")
                .Annotation("Npgsql:Enum:Education", "Medio,Superior,PosGraduacao,Tecnico,Mestrado")
                .Annotation("Npgsql:Enum:Gender", "Male,Female,Other")
                .Annotation("Npgsql:Enum:PatientType", "Student,Contractor,Guardian,Teacher")
                .Annotation("Npgsql:Enum:PsychologicalDisorder", "Depression,GeneralizedAnxiety,BipolarDisorder,Borderline,Schizophrenia,OCD,PTSD,ADHD,Autism,EatingDisorder,SubstanceAbuse,PersonalityDisorder,PanicDisorder,Psychosis,Other")
                .Annotation("Npgsql:Enum:Status", "Scheduled,Confirmed,InProgress,Completed,Canceled,NoShow")
                .Annotation("Npgsql:Enum:TypeAppointment", "Session,CollectiveActivities,AdministrativeRecords")
                .Annotation("Npgsql:Enum:UserRole", "Admin,Client,Psychologist");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Role = table.Column<int>(type: "UserRole", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PatientProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Registration = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Series = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BirthDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Gender = table.Column<int>(type: "Gender", nullable: false),
                    PatientType = table.Column<int>(type: "PatientType", nullable: false),
                    Education = table.Column<int>(type: "Education", nullable: true),
                    Course = table.Column<int>(type: "Courses", nullable: true),
                    CreatedBy = table.Column<int>(type: "CreatedBy", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Disorders = table.Column<int[]>(type: "PsychologicalDisorder[]", nullable: false),
                    Difficulties = table.Column<int[]>(type: "Difficulty[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatientProfiles_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PatientProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PsychologistProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Crp = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Specialty = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PsychologistProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PsychologistProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Anamneses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByPsychologistId = table.Column<Guid>(type: "uuid", nullable: false),
                    FamilyHistory = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Infancy = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Adolescence = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Illnesses = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Accompaniment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Anamneses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Anamneses_PatientProfiles_PatientId",
                        column: x => x.PatientId,
                        principalTable: "PatientProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Anamneses_PsychologistProfiles_CreatedByPsychologistId",
                        column: x => x.CreatedByPsychologistId,
                        principalTable: "PsychologistProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PsychologistId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: true),
                    AppointmentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "Status", nullable: false),
                    Type = table.Column<int>(type: "TypeAppointment", nullable: true),
                    ActivityType = table.Column<int>(type: "ActivityType", nullable: true),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Observation = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Objective = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Appointments_PatientProfiles_PatientId",
                        column: x => x.PatientId,
                        principalTable: "PatientProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Appointments_PsychologistProfiles_PsychologistId",
                        column: x => x.PsychologistId,
                        principalTable: "PsychologistProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmailSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    SendAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsSent = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailSchedules_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PsychologistId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Complaint = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Intervention = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Referrals = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SessionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sessions_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Sessions_PatientProfiles_PatientId",
                        column: x => x.PatientId,
                        principalTable: "PatientProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Sessions_PsychologistProfiles_PsychologistId",
                        column: x => x.PsychologistId,
                        principalTable: "PsychologistProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Anamneses_CreatedByPsychologistId",
                table: "Anamneses",
                column: "CreatedByPsychologistId");

            migrationBuilder.CreateIndex(
                name: "IX_Anamneses_PatientId",
                table: "Anamneses",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PatientId",
                table: "Appointments",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PsychologistId_AppointmentDate",
                table: "Appointments",
                columns: new[] { "PsychologistId", "AppointmentDate" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailSchedules_AppointmentId",
                table: "EmailSchedules",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailSchedules_SendAt_IsSent",
                table: "EmailSchedules",
                columns: new[] { "SendAt", "IsSent" });

            migrationBuilder.CreateIndex(
                name: "IX_PatientProfiles_CreatedByUserId",
                table: "PatientProfiles",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientProfiles_UserId",
                table: "PatientProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PsychologistProfiles_UserId",
                table: "PsychologistProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_AppointmentId",
                table: "Sessions",
                column: "AppointmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_PatientId",
                table: "Sessions",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_PsychologistId",
                table: "Sessions",
                column: "PsychologistId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Anamneses");

            migrationBuilder.DropTable(
                name: "EmailSchedules");

            migrationBuilder.DropTable(
                name: "Sessions");

            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "PatientProfiles");

            migrationBuilder.DropTable(
                name: "PsychologistProfiles");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
