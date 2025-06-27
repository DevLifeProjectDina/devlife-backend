
using System.Text.Json.Serialization;

namespace DevLifeBackend.DTOs
{

    public class SimpleHoroscopeDto
    {
        [JsonPropertyName("horoscope")]
        public string? Horoscope { get; set; }
    }
}