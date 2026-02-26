using System;

namespace Mind_Manager.src.Domain.Interfaces;

public interface IAnamnesis
{
    Task<Anamnesis> CreateAnamnesisAsync(Anamnesis anamnesis);
    Task<Anamnesis?> GetAnamnesisByIdAsync(Guid anamnesisId);
    Task<IEnumerable<Anamnesis>> GetAnamnesisByPatientIdAsync(Guid patientId);
    Task<bool> UpdateAnamnesisAsync(Guid anamnesisId, Anamnesis updatedAnamnesis);
    Task<bool> DeleteAnamnesisAsync(Guid anamnesisId);
}
