// Application/Services/LabResultsMappingService.cs
using Core.DTOs;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Domain.Models;

namespace Core.Services
{
    public class LabResultsMappingService : ILabResultsMappingService
    {
        private readonly ITestCatalogRepository _testCatalogRepository;

        public LabResultsMappingService(ITestCatalogRepository testCatalogRepository)
        {
            _testCatalogRepository = testCatalogRepository;
        }

        public LabResultsDto MapToDto(LabResults domainModel)
        {
            return new LabResultsDto
            {
                LabId = domainModel.LabId,
                ResultValue = domainModel.ResultValue,
                VisitId = domainModel.VisitId,
                CreatedAt = domainModel.CreatedAt,
                TestId = domainModel.TestId,
                TestName = domainModel.TestCatalog?.TestName ?? string.Empty,
                TestUnit = domainModel.TestCatalog?.TestUnit ?? string.Empty,
                NormalRange = domainModel.TestCatalog?.NormalRange ?? string.Empty
            };
        }

        public async Task<LabResults> MapToDomain(LabResultsDto dto)
        {
            var testCatalog = await _testCatalogRepository.GetByIdAsync(dto.TestId)
                              ?? throw new Exception($"TestCatalog {dto.TestId} not found");

            return new LabResults
            {
                LabId = dto.LabId,
                ResultValue = dto.ResultValue,
                VisitId = dto.VisitId,
                CreatedAt = dto.CreatedAt,
                TestId = dto.TestId,
                TestCatalog = testCatalog
            };
        }
    }
}