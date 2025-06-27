
using System.Text.Json.Serialization;

namespace DevLifeBackend.DTOs
{
    public class GitHubAccessTokenRequestDto
    {
        [JsonPropertyName("client_id")]
        public string ClientId { get; set; } = default!;

        [JsonPropertyName("client_secret")]
        public string ClientSecret { get; set; } = default!;

        [JsonPropertyName("code")]
        public string Code { get; set; } = default!;
    }

    public class GitHubAccessTokenResponseDto
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = default!;
    }

    public class GitHubUserDto
    {
        [JsonPropertyName("login")]
        public string Login { get; set; } = default!;

        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
    public record AnalyzeRepoRequest(string Owner, string RepoName, string? PersonalAccessToken);
}