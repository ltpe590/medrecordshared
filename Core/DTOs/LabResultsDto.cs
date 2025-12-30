namespace Core.DTOs
{
    public class LabResultsDto
    {
        public int LabId { get; set; }
        public int TestId { get; set; }
        public string ResultValue { get; set; } = string.Empty;
        public int VisitId { get; set; }
        public DateTime CreatedAt { get; set; }

        // DTO-specific properties
        public string TestName { get; set; } = string.Empty;
        public string TestUnit { get; set; } = string.Empty;
        public string NormalRange { get; set; } = string.Empty;
    }
}