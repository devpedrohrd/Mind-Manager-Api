using System;
using Mind_Manager.Domain.Entities;
using Mind_Manager.src.Domain.DTO;

namespace Mind_Manager.src.Domain.Interfaces;

public interface IPatient
{
    Task<PatientProfile> CreatePatientProfileAsync(PatientProfile createPatientProfileDto);
    Task<PatientProfile> UpdatePatientProfileAsync(PatientProfile updatePatientProfileDto);
    Task<PatientProfile?> GetPatientProfileByIdAsync(Guid patientId);
    Task<PatientProfile?> GetPatientProfileByUserIdAsync(Guid userId);
    Task<SearchPatientsResponse?> GetPatientsByFilterAsync(SearchPatientsQuery filters);
    Task<IEnumerable<PatientProfile>> GetAllPatientProfilesAsync();
}
