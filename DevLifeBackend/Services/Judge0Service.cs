using System.Text;
using System.Text.Json;
using DevLifeBackend.DTOs;
using Serilog;

namespace DevLifeBackend.Services
{
    public interface IJudge0Service
    {
        Task<Judge0SubmissionResultDto?> SubmitCodeAsync(string language, string sourceCode);
    }

    public class Judge0Service : IJudge0Service
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<Judge0Service> _logger;

        private readonly Dictionary<string, int> _languageIds = new()
        {
            { ".NET", 51 },     
            { "Python", 71 }, 
            { "React", 93 },  
            { "Angular", 93 } 
        };

        public Judge0Service(IHttpClientFactory clientFactory, ILogger<Judge0Service> logger)
        {
            _clientFactory = clientFactory;
            _logger = logger;
        }

        public async Task<Judge0SubmissionResultDto?> SubmitCodeAsync(string language, string sourceCode)
        {
            if (!_languageIds.TryGetValue(language, out var languageId))
            {
                _logger.LogError("Attempted to submit code for an unsupported language: {Language}", language);
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
                _logger.LogInformation("Submitting code to Judge0 for language ID {LanguageId}", languageId);
                var response = await client.PostAsync("submissions?wait=true", requestContent);

                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Judge0 API returned a non-success status code {StatusCode}. Response: {ResponseContent}", response.StatusCode, responseContent);
                  
                    return JsonSerializer.Deserialize<Judge0SubmissionResultDto>(responseContent);
                }

                _logger.LogInformation("Successfully received execution result from Judge0.");
                return JsonSerializer.Deserialize<Judge0SubmissionResultDto>(responseContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred while submitting code to Judge0.");
                return null;
            }
        }
    }
}