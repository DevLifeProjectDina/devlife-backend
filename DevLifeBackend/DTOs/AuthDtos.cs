// File: DTOs/AuthDtos.cs
namespace DevLifeBackend.DTOs
{
    public record UserRegistrationDto
    {
        public required string Username { get; init; }
        public required string Name { get; init; }
        public required string Surname { get; init; }
        public required DateTime DateOfBirth { get; init; }
        public required string[] Stacks { get; init; }
        public required string ExperienceLevel { get; init; }
    }
    public record LoginDto
    {
        public required string Username { get; init; }
    }
}