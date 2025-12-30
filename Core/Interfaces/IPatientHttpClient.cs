using Core.DTOs;

namespace Core.Interfaces;

public interface IPatientHttpClient
{
    Task<PatientDto?> GetPatientAsync(int id, CancellationToken ct = default);
}