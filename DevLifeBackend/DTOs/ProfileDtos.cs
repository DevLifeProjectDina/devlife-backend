// File: DTOs/ProfileDtos.cs
namespace DevLifeBackend.DTOs
{
    public record CharacterCustomizationDto
    {
        public string? Hat { get; init; }
        public string? ShirtColor { get; init; }
        public string? Pet { get; init; }
    }
}