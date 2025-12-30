using Core.DTOs;
using Core.Interfaces;

namespace Core.Services;

public sealed class PatientService
{
    private readonly IPatientHttpClient _client;

    public PatientService(IPatientHttpClient client)
    {
        _client = client;
    }

    public async Task<PatientDto?> LoadAsync(int id)
        => await _client.GetPatientAsync(id);
}