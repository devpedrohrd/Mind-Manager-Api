using Microsoft.EntityFrameworkCore;
using Mind_Manager.Domain.Interfaces;
using Mind_Manager.Infrastructure.Persistence;
using Mind_Manager.src.Domain.Interfaces;
using Mind_Manager.src.Infrastructure.Repository;
namespace Mind_Manager.Infrastructure.UnitOfWork;

public class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
{
    private readonly ApplicationDbContext _context = context;
    private IUserRepository? _userRepository;

    private IPsychologist? _psychologistRepository;

    private IPatient? _patientRepository;

    private IAppointment? _appointmentRepository;
    private src.Domain.Interfaces.ISession? _sessionRepository;
    private IAnamnesis? _anamnesisRepository;
    private IEmailSchedule? _emailScheduleRepository;

    public IUserRepository Users => _userRepository ??= new UserRepository(_context);

    public IPsychologist Psychologists => _psychologistRepository ??= new PsychologistRepository(_context);

    public IPatient Patients => _patientRepository ??= new PatientRepository(_context);

    public IAppointment Appointments => _appointmentRepository ??= new AppointmentRepository(_context);

    public src.Domain.Interfaces.ISession Sessions => _sessionRepository ??= new SessionRepository(_context);
    public IAnamnesis Anamnesis => _anamnesisRepository ??= new AnamnesisRepository(_context);
    public IEmailSchedule EmailSchedules => _emailScheduleRepository ??= new EmailScheduleRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                await action();
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
