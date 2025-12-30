// Application/Services/ILabResultsMappingService.cs
using Core.DTOs;
using Domain.Models;

namespace Core.Interfaces.Services
{
    public interface ILabResultsMappingService
    {
        LabResultsDto MapToDto(LabResults domainModel);
        Task<LabResults> MapToDomain(LabResultsDto dto);
    }
}