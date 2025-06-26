// File: Services/AiSnippetGeneratorService.cs
using OpenAI;
using OpenAI.Chat;

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

        public AiSnippetGeneratorService(OpenAIClient openAIClient)
        {
            _openAIClient = openAIClient;
        }

        public async Task<string?> GenerateBuggySnippetAsync(string correctCode, string language)
        {
            var prompt =
                $"You are a code assistant. I will provide you with a working piece of code in {language}. " +
                "Your task is to introduce one single, subtle bug into it. " +
                "Return ONLY the complete, buggy code block. Do not add any explanation or surrounding text. " +
                $"Here is the correct code:\n\n{correctCode}";

            try
            {
                // FIX: Pass the model name as a string
                var chatRequest = new ChatRequest(new[] { new Message(Role.System, prompt) }, "gpt-3.5-turbo");
                var response = await _openAIClient.ChatEndpoint.GetCompletionAsync(chatRequest);
                return response.FirstChoice.Message.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during OpenAI buggy code generation: {ex.Message}");
                return null;
            }
        }

        public async Task<string?> GenerateCorrectSnippetAsync(string description, string language)
        {
            var prompt =
                $"Based on the following programming challenge description, write a correct and complete code solution in {language}. " +
                "Return ONLY the raw code block, without any explanation, comments, or markdown fences like ```. " +
                $"Description: \"{description}\"";

            try
            {
                // FIX: Pass the model name as a string
                var chatRequest = new ChatRequest(new[] { new Message(Role.User, prompt) }, "gpt-3.5-turbo");
                var response = await _openAIClient.ChatEndpoint.GetCompletionAsync(chatRequest);
                return response.FirstChoice.Message.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during OpenAI correct code generation: {ex.Message}");
                return null;
            }
        }
    }
}