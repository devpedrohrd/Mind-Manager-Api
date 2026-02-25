using Mind_Manager.src.Domain.Interfaces;

namespace Mind_Manager.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IPsychologist Psychologists { get; }
    IPatient Patients { get; }

    IAppointment Appointments { get; }
    src.Domain.Interfaces.ISession Sessions { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
