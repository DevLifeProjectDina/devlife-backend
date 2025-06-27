
namespace DevLifeBackend.DTOs
{
    public record ExcuseDto
    {
        public int Id { get; init; }
        public required string Text { get; init; }
        public required string Type { get; init; }
        public required List<string> MeetingCategories { get; init; }
        public int BelievabilityScore { get; init; }
    }

    public record FavoriteExcuseRequestDto
    {
        public required int ExcuseId { get; init; }
    }
}