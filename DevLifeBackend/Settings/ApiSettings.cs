
namespace DevLifeBackend.Settings
{
    public class ApiSettings
    {
        public required string OpenAiImageGeneration { get; set; }
        public required string CodewarsChallenge { get; set; }
        public required string HoroscopeApi { get; set; }
        public required string GitHubTokenEndpoint { get; set; }
        public required string GitHubUserEndpoint { get; set; }
        public required string GitHubAuthorizeEndpoint { get; set; }
    }
}