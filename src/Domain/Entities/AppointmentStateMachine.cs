using Mind_Manager.Domain.Exceptions;

namespace Mind_Manager.Domain.Entities;

/// <summary>
/// Máquina de estados para transições de status de agendamentos (Appointments).
/// Define quais transições são válidas a partir de cada estado.
/// </summary>
public static class AppointmentStateMachine
{
    private static readonly Dictionary<Status, HashSet<Status>> _validTransitions = new()
    {
        [Status.Scheduled]   = [Status.Confirmed, Status.Canceled],
        [Status.Confirmed]   = [Status.InProgress, Status.Canceled, Status.NoShow],
        [Status.InProgress]  = [Status.Completed],
        [Status.Completed]   = [],
        [Status.Canceled]    = [],
        [Status.NoShow]      = [],
    };

    /// <summary>
    /// Verifica se a transição de <paramref name="from"/> para <paramref name="to"/> é válida.
    /// </summary>
    public static bool CanTransition(Status from, Status to)
    {
        return _validTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
    }

    /// <summary>
    /// Valida a transição e lança exceção caso seja inválida.
    /// </summary>
    /// <exception cref="BusinessException">Transição de status inválida.</exception>
    public static void ValidateTransition(Status from, Status to)
    {
        if (!CanTransition(from, to))
            throw new BusinessException($"Transição de status inválida: '{from}' → '{to}'. Transições permitidas a partir de '{from}': [{string.Join(", ", GetAllowedTransitions(from))}].");
    }

    /// <summary>
    /// Retorna os estados permitidos a partir do estado informado.
    /// </summary>
    public static IReadOnlySet<Status> GetAllowedTransitions(Status from)
    {
        return _validTransitions.TryGetValue(from, out var allowed) ? allowed : new HashSet<Status>();
    }

    /// <summary>
    /// Verifica se o status é um estado final (sem transições possíveis).
    /// </summary>
    public static bool IsFinalState(Status status)
    {
        return _validTransitions.TryGetValue(status, out var allowed) && allowed.Count == 0;
    }
}
