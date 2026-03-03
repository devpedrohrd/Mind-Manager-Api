namespace Mind_Manager.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Mind_Manager.Domain.Entities;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.NameTranslation;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Configura os mapeamentos de enums nativos do PostgreSQL no NpgsqlDbContextOptionsBuilder.
    /// Isso popula as EnumDefinitions usadas pelo EF Core para type mapping
    /// E também faz o NpgsqlDataSourceManager criar o DataSource com MapEnum automaticamente.
    /// </summary>
    public static void ConfigureEnums(NpgsqlDbContextOptionsBuilder npgsqlOptions)
    {
        var nt = new NpgsqlNullNameTranslator();
        npgsqlOptions.MapEnum<UserRole>(nameTranslator: nt);
        npgsqlOptions.MapEnum<Gender>(nameTranslator: nt);
        npgsqlOptions.MapEnum<CreatedBy>(nameTranslator: nt);
        npgsqlOptions.MapEnum<PatientType>(nameTranslator: nt);
        npgsqlOptions.MapEnum<Education>(nameTranslator: nt);
        npgsqlOptions.MapEnum<Courses>(nameTranslator: nt);
        npgsqlOptions.MapEnum<Status>(nameTranslator: nt);
        npgsqlOptions.MapEnum<TypeAppointment>(nameTranslator: nt);
        npgsqlOptions.MapEnum<ActivityType>(nameTranslator: nt);
        npgsqlOptions.MapEnum<PsychologicalDisorder>(nameTranslator: nt);
        npgsqlOptions.MapEnum<Difficulty>(nameTranslator: nt);
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<PsychologistProfile> PsychologistProfiles { get; set; } = null!;
    public DbSet<PatientProfile> PatientProfiles { get; set; } = null!;
    public DbSet<Appointment> Appointments { get; set; } = null!;
    public DbSet<Session> Sessions { get; set; } = null!;
    public DbSet<Anamnesis> Anamneses { get; set; } = null!;
    public DbSet<EmailSchedule> EmailSchedules { get; set; } = null!;

    public override int SaveChanges()
    {
        NormalizeDateTimesToUtc();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        NormalizeDateTimesToUtc();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Converte todas as propriedades DateTime com Kind=Unspecified para Kind=Utc
    /// antes de salvar no PostgreSQL (Npgsql exige Kind=Utc para timestamp with time zone).
    /// </summary>
    private void NormalizeDateTimesToUtc()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            foreach (var property in entry.Properties)
            {
                if (property.CurrentValue is DateTime dt && dt.Kind == DateTimeKind.Unspecified)
                {
                    property.CurrentValue = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                }
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Registrar tipos ENUM nativos do PostgreSQL (PascalCase = nomes C#)
        var nt = new NpgsqlNullNameTranslator();
        modelBuilder.HasPostgresEnum<UserRole>(nameTranslator: nt);
        modelBuilder.HasPostgresEnum<Gender>(nameTranslator: nt);
        modelBuilder.HasPostgresEnum<CreatedBy>(nameTranslator: nt);
        modelBuilder.HasPostgresEnum<PatientType>(nameTranslator: nt);
        modelBuilder.HasPostgresEnum<Education>(nameTranslator: nt);
        modelBuilder.HasPostgresEnum<Courses>(nameTranslator: nt);
        modelBuilder.HasPostgresEnum<Status>(nameTranslator: nt);
        modelBuilder.HasPostgresEnum<TypeAppointment>(nameTranslator: nt);
        modelBuilder.HasPostgresEnum<ActivityType>(nameTranslator: nt);
        modelBuilder.HasPostgresEnum<PsychologicalDisorder>(nameTranslator: nt);
        modelBuilder.HasPostgresEnum<Difficulty>(nameTranslator: nt);

        // --- USER ---
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Propriedades obrigatórias
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.PasswordHash)
                .IsRequired();

            entity.Property(e => e.Phone)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            entity.Property(e => e.Role)
                .HasColumnType("UserRole")
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .IsRequired(false);

            // Índice único em Email
            entity.HasIndex(e => e.Email).IsUnique();

            // Relacionamentos 1:1 com cascade delete
            entity.HasOne(u => u.PsychologistProfile)
                .WithOne(p => p.User)
                .HasForeignKey<PsychologistProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(u => u.PatientProfile)
                .WithOne(p => p.User)
                .HasForeignKey<PatientProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // --- PATIENT PROFILE ---
        modelBuilder.Entity<PatientProfile>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.UserId).IsRequired();

            entity.Property(e => e.Registration)
                .HasMaxLength(100);

            entity.Property(e => e.Series)
                .HasMaxLength(50);

            entity.Property(e => e.Gender)
                .HasColumnType("Gender")
                .IsRequired();

            entity.Property(e => e.PatientType)
                .HasColumnType("PatientType")
                .IsRequired();

            entity.Property(e => e.CreatedBy)
                .HasColumnType("CreatedBy")
                .IsRequired();

            entity.Property(e => e.Education)
                .HasColumnType("Education");

            entity.Property(e => e.Course)
                .HasColumnType("Courses");

            // Arrays de enums (PostgreSQL) — Npgsql lida nativamente com enum[]
            entity.Property(e => e.Disorders)
                .HasColumnType("PsychologicalDisorder[]");
            entity.Property(e => e.Disorders)
                .Metadata.SetValueComparer(new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<PsychologicalDisorder>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));

            // Arrays de enums (PostgreSQL) — Npgsql lida nativamente com enum[]
            entity.Property(e => e.Difficulties)
                .HasColumnType("Difficulty[]");
            entity.Property(e => e.Difficulties)
                .Metadata.SetValueComparer(new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<Difficulty>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));

            // Relacionamento: quem criou o perfil do paciente
            entity.HasOne(p => p.CreatedByUser)
                .WithMany()
                .HasForeignKey(p => p.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Relacionamentos N:1
            entity.HasOne(p => p.User)
                .WithOne(u => u.PatientProfile)
                .HasForeignKey<PatientProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(p => p.Anamneses)
                .WithOne(a => a.Patient)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(p => p.Appointments)
                .WithOne(a => a.Patient)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(p => p.Sessions)
                .WithOne(s => s.Patient)
                .HasForeignKey(s => s.PatientId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // --- PSYCHOLOGIST PROFILE ---
        modelBuilder.Entity<PsychologistProfile>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.UserId).IsRequired();

            entity.Property(e => e.Crp)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.Specialty)
                .IsRequired()
                .HasMaxLength(100);

            // Relacionamento 1:1
            entity.HasOne(p => p.User)
                .WithOne(u => u.PsychologistProfile)
                .HasForeignKey<PsychologistProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relacionamentos 1:N
            entity.HasMany(p => p.Appointments)
                .WithOne(a => a.Psychologist)
                .HasForeignKey(a => a.PsychologistId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(p => p.Sessions)
                .WithOne(s => s.Psychologist)
                .HasForeignKey(s => s.PsychologistId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // --- APPOINTMENT ---
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.PsychologistId).IsRequired();

            entity.Property(e => e.PatientId).IsRequired(false);

            entity.Property(e => e.AppointmentDate).IsRequired();

            entity.Property(e => e.Status)
                .HasColumnType("Status")
                .IsRequired();

            entity.Property(e => e.Type)
                .HasColumnType("TypeAppointment");

            entity.Property(e => e.ActivityType)
                .HasColumnType("ActivityType");

            entity.Property(e => e.Reason)
                .HasMaxLength(500);

            entity.Property(e => e.Observation)
                .HasMaxLength(1000);

            entity.Property(e => e.Objective)
                .HasMaxLength(500);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Índice para busca de agenda
            entity.HasIndex(e => new { e.PsychologistId, e.AppointmentDate });

            // Relacionamentos
            entity.HasOne(a => a.Psychologist)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PsychologistId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(a => a.Session)
                .WithOne(s => s.Appointment)
                .HasForeignKey<Session>(s => s.AppointmentId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(a => a.EmailSchedules)
                .WithOne(e => e.Appointment)
                .HasForeignKey(e => e.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // --- SESSION ---
        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.PsychologistId).IsRequired();

            entity.Property(e => e.PatientId).IsRequired();

            entity.Property(e => e.SessionDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.Complaint)
                .HasMaxLength(1000);

            entity.Property(e => e.Intervention)
                .HasMaxLength(1000);

            entity.Property(e => e.Referrals)
                .HasMaxLength(500);

            // Relacionamentos
            entity.HasOne(s => s.Psychologist)
                .WithMany(p => p.Sessions)
                .HasForeignKey(s => s.PsychologistId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(s => s.Patient)
                .WithMany(p => p.Sessions)
                .HasForeignKey(s => s.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(s => s.Appointment)
                .WithOne(a => a.Session)
                .HasForeignKey<Session>(s => s.AppointmentId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // --- ANAMNESIS ---
        modelBuilder.Entity<Anamnesis>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.PatientId).IsRequired();

            entity.Property(e => e.CreatedByPsychologistId).IsRequired();

            entity.Property(e => e.FamilyHistory)
                .HasMaxLength(1000);

            entity.Property(e => e.Infancy)
                .HasMaxLength(1000);

            entity.Property(e => e.Adolescence)
                .HasMaxLength(1000);

            entity.Property(e => e.Illnesses)
                .HasMaxLength(1000);

            entity.Property(e => e.Accompaniment)
                .HasMaxLength(1000);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relacionamento com Paciente
            entity.HasOne(a => a.Patient)
                .WithMany(p => p.Anamneses)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relacionamento com Psicólogo criador (isolamento de dados)
            entity.HasOne(a => a.CreatedByPsychologist)
                .WithMany()
                .HasForeignKey(a => a.CreatedByPsychologistId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // --- EMAIL SCHEDULE ---
        modelBuilder.Entity<EmailSchedule>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.AppointmentId).IsRequired();

            entity.Property(e => e.SendAt).IsRequired();

            entity.Property(e => e.IsSent)
                .IsRequired()
                .HasDefaultValue(false);

            // Índice para worker de disparos
            entity.HasIndex(e => new { e.SendAt, e.IsSent });

            // Relacionamento
            entity.HasOne(e => e.Appointment)
                .WithMany(a => a.EmailSchedules)
                .HasForeignKey(e => e.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
