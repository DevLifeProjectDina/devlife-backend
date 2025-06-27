
namespace DevLifeBackend.DTOs
{
    public record DatingProfileDto
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public int Age { get; init; }
        public required string[] Stacks { get; init; }
        public required string Bio { get; init; }
    }

    public record ChatRequestDto
    {
        public required string ProfileId { get; init; }
        public required string Message { get; init; }
    }
}