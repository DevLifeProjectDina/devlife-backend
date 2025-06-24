// File: Services/Judge0Service.cs
using System.Text;
using System.Text.Json;
using DevLifeBackend.DTOs;

namespace DevLifeBackend.Services
{
    public interface IJudge0Service
    {
        Task<Judge0SubmissionResultDto?> SubmitCodeAsync(string language, string sourceCode);
    }

    public class Judge0Service : IJudge0Service
    {
        private readonly IHttpClientFactory _clientFactory;

        // Mapping our app's language names to Judge0's internal language IDs
        private readonly Dictionary<string, int> _languageIds = new()
        {
            { ".NET", 51 },     // C#
            { "Python", 71 },   // Python 3
            { "React", 93 },    // JavaScript
            { "Angular", 93 }   // JavaScript
        };

        public Judge0Service(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task<Judge0SubmissionResultDto?> SubmitCodeAsync(string language, string sourceCode)
        {
            if (!_languageIds.TryGetValue(language, out var languageId))
            {
                // Language not supported by our Judge0 setup
                return null;
            }

            var client = _clientFactory.CreateClient("Judge0Client");

            var submissionRequest = new Judge0SubmissionRequestDto
            {
                LanguageId = languageId,
                SourceCode = sourceCode
            };

            var jsonContent = JsonSerializer.Serialize(submissionRequest);
            var requestContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                // We use the synchronous endpoint by adding "?wait=true"
                var response = await client.PostAsync("submissions?wait=true", requestContent);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Judge0SubmissionResultDto>(responseJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error submitting code to Judge0: {ex.Message}");
                return null;
            }
        }
    }
}