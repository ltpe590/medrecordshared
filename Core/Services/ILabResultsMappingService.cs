// Application/Services/ILabResultsMappingService.cs
using Core.DTOs;
using Domain.Models;

namespace Core.Services
{
    public interface ILabResultsMappingService
    {
        LabResultsDto MapToDto(LabResults domainModel);
        Task<LabResults> MapToDomain(LabResultsDto dto);
    }
}