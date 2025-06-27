
namespace DevLifeBackend.DTOs
{
    public record CodeRoastChallengeDto
    {
        public required string Name { get; init; }
        public required string Description { get; init; }
        public required string Language { get; init; }
        public required string Source { get; init; }
    }

    public record SubmitRoastSolutionDto
    {
        public required string Language { get; init; }
        public required string SourceCode { get; init; }
    }

    public record RoastResultDto
    {
        public required string ExecutionStatus { get; init; } 
        public required string AiRoast { get; init; }
        public string? ExecutionOutput { get; init; }
    }
}