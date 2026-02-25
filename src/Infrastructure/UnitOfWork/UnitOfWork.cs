using Microsoft.EntityFrameworkCore.Storage;
using Mind_Manager.Domain.Interfaces;
using Mind_Manager.Infrastructure.Persistence;
using Mind_Manager.src.Domain.Interfaces;
using Mind_Manager.src.Infrastructure.Repository;

namespace Mind_Manager.Infrastructure.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;
    private IUserRepository? _userRepository;

    private IPsychologist? _psychologistRepository;

    private IPatient? _patientRepository;

    private IAppointment? _appointmentRepository;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IUserRepository Users => _userRepository ??= new UserRepository(_context);

    public IPsychologist Psychologists => _psychologistRepository ??= new PsychologistRepository(_context);

    public IPatient Patients => _patientRepository ??= new PatientRepository(_context);

    public IAppointment Appointments => _appointmentRepository ??= new AppointmentRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);
            if (_transaction != null)
                await _transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            _transaction.Dispose();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
