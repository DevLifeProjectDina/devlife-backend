// File: DTOs/AztroDto.cs
using System.Text.Json.Serialization;

namespace DevLifeBackend.DTOs
{
    // This record maps to the JSON response from the Aztro API
    public record AztroDto
    {
        [JsonPropertyName("description")]
        public string Description { get; init; } = default!;

        [JsonPropertyName("mood")]
        public string Mood { get; init; } = default!;

        [JsonPropertyName("color")]
        public string Color { get; init; } = default!;

        [JsonPropertyName("lucky_number")]
        public string LuckyNumber { get; init; } = default!;
    }
}