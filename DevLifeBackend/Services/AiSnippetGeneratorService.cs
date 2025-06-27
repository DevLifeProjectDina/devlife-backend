using OpenAI;
using OpenAI.Chat;
using Serilog;

namespace DevLifeBackend.Services
{
    public interface IAiSnippetGeneratorService
    {
        Task<string?> GenerateBuggySnippetAsync(string correctCode, string language);
        Task<string?> GenerateCorrectSnippetAsync(string description, string language);
    }

    public class AiSnippetGeneratorService : IAiSnippetGeneratorService
    {
        private readonly OpenAIClient _openAIClient;
        private readonly ILogger<AiSnippetGeneratorService> _logger;

        public AiSnippetGeneratorService(OpenAIClient openAIClient, ILogger<AiSnippetGeneratorService> logger)
        {
            _openAIClient = openAIClient;
            _logger = logger;
        }

        public async Task<string?> GenerateBuggySnippetAsync(string correctCode, string language)
        {
            var prompt =
                $"You are a code assistant. I will provide you with a working piece of code in {language}. " +
                "Your task is to introduce one single, subtle bug into it. " +
                "Return ONLY the complete, buggy code block. Do not add any explanation or surrounding text. " +
                $"Here is the correct code:\n\n{correctCode}";

            _logger.LogInformation("Requesting buggy code generation from OpenAI for language: {Language}", language);

            try
            {
                var chatRequest = new ChatRequest(new[] { new Message(Role.System, prompt) }, "gpt-3.5-turbo");
                var response = await _openAIClient.ChatEndpoint.GetCompletionAsync(chatRequest);
                var buggyCode = response.FirstChoice.Message.ToString();

                if (!string.IsNullOrWhiteSpace(buggyCode))
                {
                    _logger.LogInformation("Successfully generated buggy code snippet.");
                }
                else
                {
                    _logger.LogWarning("OpenAI returned a successful but empty response for buggy code generation.");
                }

                return buggyCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during OpenAI buggy code generation.");
                return null;
            }
        }

        public async Task<string?> GenerateCorrectSnippetAsync(string description, string language)
        {
            var prompt =
                $"Based on the following programming challenge description, write a correct and complete code solution in {language}. " +
                "Return ONLY the raw code block, without any explanation, comments, or markdown fences like ```. " +
                $"Description: \"{description}\"";

            _logger.LogInformation("Requesting correct code solution from OpenAI for language: {Language}", language);

            try
            {
                var chatRequest = new ChatRequest(new[] { new Message(Role.User, prompt) }, "gpt-3.5-turbo");
                var response = await _openAIClient.ChatEndpoint.GetCompletionAsync(chatRequest);
                var correctCode = response.FirstChoice.Message.ToString();

                if (!string.IsNullOrWhiteSpace(correctCode))
                {
                    _logger.LogInformation("Successfully generated correct code solution.");
                }
                else
                {
                    _logger.LogWarning("OpenAI returned a successful but empty response for correct code generation.");
                }

                return correctCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during OpenAI correct code generation.");
                return null;
            }
        }
    }
}