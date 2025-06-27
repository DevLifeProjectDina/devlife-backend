
using System.Text.Json.Serialization;

namespace DevLifeBackend.DTOs
{
    public class Judge0SubmissionRequestDto
    {
        [JsonPropertyName("source_code")]
        public string SourceCode { get; set; } = default!;

        [JsonPropertyName("language_id")]
        public int LanguageId { get; set; }
    }

    public class Judge0SubmissionResultDto
    {
        [JsonPropertyName("stdout")]
        public string? Stdout { get; set; }

        [JsonPropertyName("stderr")]
        public string? Stderr { get; set; }

        [JsonPropertyName("compile_output")]
        public string? CompileOutput { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("status")]
        public Judge0StatusDto? Status { get; set; }
    }

    public class Judge0StatusDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; } = default!;
    }
}