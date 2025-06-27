
namespace DevLifeBackend.DTOs
{
    public record GenerateSnippetRequestDto
    {
        public required string Language { get; init; }
        public required string Difficulty { get; init; }
    }
}