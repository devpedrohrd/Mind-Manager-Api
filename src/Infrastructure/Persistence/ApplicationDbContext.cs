namespace Mind_Manager.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Mind_Manager.Domain.Entities;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<PsychologistProfile> PsychologistProfiles { get; set; } = null!;
    public DbSet<PatientProfile> PatientProfiles { get; set; } = null!;
    public DbSet<Appointment> Appointments { get; set; } = null!;
    public DbSet<Session> Sessions { get; set; } = null!;
    public DbSet<Anamnesis> Anamneses { get; set; } = null!;
    public DbSet<EmailSchedule> EmailSchedules { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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
                .IsRequired();

            entity.Property(e => e.PatientType)
                .IsRequired();

            entity.Property(e => e.Education);

            entity.Property(e => e.Course);

            entity.Property(e => e.CreatedBy)
                .IsRequired();

            // Arrays de enums (PostgreSQL) - convertendo para int[] para armazenamento eficiente
            entity.Property(e => e.Disorders)
                .HasConversion(
                    v => v.Select(d => (int)d).ToArray(),
                    v => v.Select(i => (PsychologicalDisorder)i).ToList()
                )
                .Metadata.SetValueComparer(new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<PsychologicalDisorder>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));

            entity.Property(e => e.Difficulties)
                .HasConversion(
                    v => v.Select(d => (int)d).ToArray(),
                    v => v.Select(i => (Difficulty)i).ToList()
                )
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

            entity.Property(e => e.AppointmentDate).IsRequired();

            entity.Property(e => e.Status)
                .IsRequired();

            entity.Property(e => e.Type);

            entity.Property(e => e.ActivityType);

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

            // Relacionamento
            entity.HasOne(a => a.Patient)
                .WithMany(p => p.Anamneses)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Cascade);
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
