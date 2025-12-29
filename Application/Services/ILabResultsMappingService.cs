// Application/Services/ILabResultsMappingService.cs
using System.Threading.Tasks;
using Application.DTOs;
using Domain.Models;

namespace Application.Services
{
    public interface ILabResultsMappingService
    {
        LabResultsDto MapToDto(LabResults domainModel);
        Task<LabResults> MapToDomain(LabResultsDto dto);
    }
}