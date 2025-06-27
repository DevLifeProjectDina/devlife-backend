using DevLifeBackend.DTOs;
using OpenAI;
using OpenAI.Chat;
using Serilog;

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
        private readonly ILogger<CodeRoastService> _logger;

        public CodeRoastService(ICodewarsService codewarsService, IJudge0Service judge0Service, OpenAIClient openAIClient, ILogger<CodeRoastService> logger)
        {
            _codewarsService = codewarsService;
            _judge0Service = judge0Service;
            _openAIClient = openAIClient;
            _logger = logger;
        }

        public async Task<CodeRoastChallengeDto?> GetChallengeAsync(string language, string difficulty)
        {
            _logger.LogInformation("Fetching roast challenge from Codewars for language {Language} and difficulty {Difficulty}", language, difficulty);
            var task = await _codewarsService.GetRandomTaskAsync(language, difficulty);
            if (task == null)
            {
                _logger.LogWarning("No roast challenge found for language {Language} and difficulty {Difficulty}", language, difficulty);
                return null;
            }

            _logger.LogInformation("Successfully fetched roast challenge '{TaskName}' from source: {Source}", task.Name, task.Source);
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
            _logger.LogInformation("Starting roast analysis for language {Language}", language);

            _logger.LogInformation("Submitting code to Judge0 for execution...");
            var executionResult = await _judge0Service.SubmitCodeAsync(language, sourceCode);
            if (executionResult == null)
            {
                _logger.LogError("Judge0 execution failed or returned null.");
                return new RoastResultDto
                {
                    ExecutionStatus = "Execution Service Failed",
                    AiRoast = "I couldn't even run your code to roast it. The execution service might be down.",
                    ExecutionOutput = "N/A"
                };
            }

            var executionStatus = executionResult.Status?.Description ?? "Unknown Status";
            var executionOutput = executionResult.Stderr ?? executionResult.CompileOutput ?? executionResult.Message ?? executionResult.Stdout;
            _logger.LogInformation("Judge0 execution completed with status: {Status}", executionStatus);

            var prompt = executionStatus == "Accepted"
                ? $"This code was submitted for a programming challenge in {language} and it works correctly. Write a short, witty, and slightly humorous compliment about it. Be a bit sarcastic, like a senior developer reviewing a junior's code. Return only the compliment. The code is:\n\n{sourceCode}"
                : $"This code was submitted for a programming challenge in {language} and it failed. The execution status is '{executionStatus}' and the error output is '{executionOutput}'. Write a short, witty, and funny 'roast' of this code. Be sarcastic but not mean. Return only the roast. The code is:\n\n{sourceCode}";

            string aiRoast = "The AI is speechless. It has seen it all now.";
            try
            {
                _logger.LogInformation("Sending code to OpenAI for a roast/compliment.");
                var chatRequest = new ChatRequest(new[] { new Message(Role.System, prompt) }, "gpt-3.5-turbo");
                var response = await _openAIClient.ChatEndpoint.GetCompletionAsync(chatRequest);
                aiRoast = response.FirstChoice.Message.ToString() ?? aiRoast;
                _logger.LogInformation("Successfully received roast from OpenAI.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OpenAI roast generation failed.");
            }

            return new RoastResultDto
            {
                ExecutionStatus = executionStatus,
                AiRoast = aiRoast,
                ExecutionOutput = executionOutput
            };
        }
    }
}