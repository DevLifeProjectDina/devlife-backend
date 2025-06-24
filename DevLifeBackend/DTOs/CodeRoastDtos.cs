// File: DTOs/CodeRoastDtos.cs
namespace DevLifeBackend.DTOs
{
    public record CodeRoastChallengeDto
    {
        public required string Name { get; init; }
        public required string Description { get; init; }
        public required string Language { get; init; }
        public required string Source { get; init; }
    }

    // DTO for submitting a solution
    public record SubmitRoastSolutionDto
    {
        public required string Language { get; init; }
        public required string SourceCode { get; init; }
    }

    // DTO for the final combined result
    public record RoastResultDto
    {
        public required string ExecutionStatus { get; init; } // e.g., "Accepted", "Wrong Answer"
        public required string AiRoast { get; init; }
        public string? ExecutionOutput { get; init; } // stdout or stderr from Judge0
    }
}