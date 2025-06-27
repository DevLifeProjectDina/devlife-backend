
using System.Text.Json.Serialization;

namespace DevLifeBackend.DTOs
{
    public record CodewarsChallengeDto
    {
        [JsonPropertyName("name")]
        public string Name { get; init; } = default!;

        [JsonPropertyName("description")]
        public string Description { get; init; } = default!;
    }
}