using Mind_Manager.src.Domain.Interfaces;

namespace Mind_Manager.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IPsychologist Psychologists { get; }
    IPatient Patients { get; }

    IAppointment Appointments { get; }
    src.Domain.Interfaces.ISession Sessions { get; }

    IAnamnesis Anamnesis { get; }
    IEmailSchedule EmailSchedules { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default);
}
