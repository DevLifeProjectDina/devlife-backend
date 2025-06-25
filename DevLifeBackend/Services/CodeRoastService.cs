// File: Services/CodeRoastService.cs
using DevLifeBackend.DTOs;
using OpenAI;
using OpenAI.Chat;

namespace DevLifeBackend.Services
{
    public interface ICodeRoastService
    {
        Task<CodeRoastChallengeDto?> GetChallengeAsync(string language, string difficulty);
        Task<RoastResultDto?> GetRoastAnalysisAsync(string language, string sourceCode);
    }

    public class CodeRoastService : ICodeRoastService
    {
        private readonly ICodewarsService _codewarsService;
        private readonly IJudge0Service _judge0Service;
        private readonly OpenAIClient _openAIClient;

        public CodeRoastService(ICodewarsService codewarsService, IJudge0Service judge0Service, OpenAIClient openAIClient)
        {
            _codewarsService = codewarsService;
            _judge0Service = judge0Service;
            _openAIClient = openAIClient;
        }

        public async Task<CodeRoastChallengeDto?> GetChallengeAsync(string language, string difficulty)
        {
            var task = await _codewarsService.GetRandomTaskAsync(language, difficulty);
            if (task == null) return null;

            return new CodeRoastChallengeDto
            {
                Name = task.Name,
                Description = task.Description,
                Language = task.Language,
                Source = task.Source
            };
        }

        public async Task<RoastResultDto?> GetRoastAnalysisAsync(string language, string sourceCode)
        {
            // Step 1: Execute the code via Judge0
            var executionResult = await _judge0Service.SubmitCodeAsync(language, sourceCode);
            if (executionResult == null)
            {
                return new RoastResultDto
                {
                    ExecutionStatus = "Execution Service Failed",
                    AiRoast = "I couldn't even run your code to roast it. The execution service might be down.",
                    ExecutionOutput = "N/A"
                };
            }

            var executionStatus = executionResult.Status?.Description ?? "Unknown Status";
            var executionOutput = executionResult.Stderr ?? executionResult.CompileOutput ?? executionResult.Message ?? executionResult.Stdout;

            // Step 2: Generate a roast from OpenAI based on the result
            var prompt = executionStatus == "Accepted"
                ? $"This code was submitted for a programming challenge in {language} and it works correctly. Write a short, witty, and slightly humorous compliment about it. Be a bit sarcastic, like a senior developer reviewing a junior's code. Return only the compliment. The code is:\n\n{sourceCode}"
                : $"This code was submitted for a programming challenge in {language} and it failed. The execution status is '{executionStatus}' and the error output is '{executionOutput}'. Write a short, witty, and funny 'roast' of this code. Be sarcastic but not mean. Return only the roast. The code is:\n\n{sourceCode}";

            string aiRoast = "The AI is speechless. It has seen it all now.";
            try
            {
                var chatRequest = new ChatRequest(new[] { new Message(Role.System, prompt) }, "gpt-3.5-turbo");
                var response = await _openAIClient.ChatEndpoint.GetCompletionAsync(chatRequest);
                aiRoast = response.FirstChoice.Message.ToString() ?? aiRoast;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OpenAI roast failed: {ex.Message}");
            }

            // Step 3: Combine and return the final result
            return new RoastResultDto
            {
                ExecutionStatus = executionStatus,
                AiRoast = aiRoast,
                ExecutionOutput = executionOutput
            };
        }
    }
}