// File: DTOs/Judge0Dtos.cs
using System.Text.Json.Serialization;

namespace DevLifeBackend.DTOs
{
    // This is what we SEND to Judge0
    public class Judge0SubmissionRequestDto
    {
        [JsonPropertyName("source_code")]
        public string SourceCode { get; set; } = default!;

        [JsonPropertyName("language_id")]
        public int LanguageId { get; set; }
    }

    // This is what we GET BACK from Judge0
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