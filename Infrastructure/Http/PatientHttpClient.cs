using Core.DTOs;
using Core.Interfaces;

namespace Infrastructure.Http;

public sealed class PatientHttpClient : IPatientHttpClient
{
    public async Task<PatientDto?> GetPatientAsync(int id, CancellationToken ct = default)
    {
        // real HTTP call here
    }
}