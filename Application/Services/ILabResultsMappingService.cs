using Domain.Models;
using Application.DTOs;


namespace Application.Services
{
    public interface ILabResultsMappingService
    {
        LabResultsDto MapToDto(LabResults domainModel);
        LabResults MapToDomain(LabResultsDto dto);
    }

    public class LabResultsMappingService : ILabResultsMappingService
    {
        public LabResultsDto MapToDto(LabResults domainModel)
        {
            return new LabResultsDto
            {
                LabId = domainModel.LabId,
                TestId = domainModel.TestId,
                ResultValue = domainModel.ResultValue,
                VisitId = domainModel.VisitId,
                // Map navigation properties as needed
            };
        }

        public LabResults MapToDomain(LabResultsDto dto)
        {
            return new LabResults
            {
                LabId = dto.LabId,
                TestId = dto.TestId,
                ResultValue = dto.ResultValue,
                VisitId = dto.VisitId,
                // Map navigation properties as needed
            };
        }
    }
}