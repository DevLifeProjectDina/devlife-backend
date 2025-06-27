using DevLifeBackend.Data;
using DevLifeBackend.DTOs;
using DevLifeBackend.Models;
using MongoDB.Driver;
using OpenAI;
using OpenAI.Chat;
using Serilog;

namespace DevLifeBackend.Services
{
    public interface IDatingService
    {
        Task<List<DatingProfileDto>> GetRandomProfilesAsync(int count);
        Task<string> GetChatResponseAsync(string profileId, string userMessage);
    }

    public class DatingService : IDatingService
    {
        private readonly MongoDbContext _mongoContext;
        private readonly OpenAIClient _openAIClient;
        private readonly ILogger<DatingService> _logger;

        public DatingService(MongoDbContext mongoContext, OpenAIClient openAIClient, ILogger<DatingService> logger)
        {
            _mongoContext = mongoContext;
            _openAIClient = openAIClient;
            _logger = logger;
        }

        public async Task<List<DatingProfileDto>> GetRandomProfilesAsync(int count)
        {
            _logger.LogInformation("Fetching {Count} random dating profiles from MongoDB.", count);
            var profiles = await _mongoContext.DatingProfiles.Find(_ => true).Limit(count).ToListAsync();

            return profiles.Select(p => new DatingProfileDto
            {
                Id = p.Id!,
                Name = p.Name,
                Age = p.Age,
                Stacks = p.Stacks,
                Bio = p.Bio
            }).ToList();
        }

        public async Task<string> GetChatResponseAsync(string profileId, string userMessage)
        {
            _logger.LogInformation("Requesting AI chat response for profile ID: {ProfileId}", profileId);

            var profile = await _mongoContext.DatingProfiles.Find(p => p.Id == profileId).FirstOrDefaultAsync();
            if (profile == null || string.IsNullOrEmpty(profile.CharacterPrompt))
            {
                _logger.LogWarning("Dating profile with ID {ProfileId} not found or has no character prompt.", profileId);
                return "I'm a bit busy refactoring my code right now, talk later!";
            }

            var prompt =
                $"{profile.CharacterPrompt} You just received a message from a potential match. " +
                $"The message is: '{userMessage}'. Write a short, in-character response.";

            _logger.LogInformation("Sending chat prompt to OpenAI for profile: {ProfileName}", profile.Name);

            try
            {
                var chatRequest = new ChatRequest(new[] { new Message(Role.System, prompt) }, "gpt-3.5-turbo");
                var response = await _openAIClient.ChatEndpoint.GetCompletionAsync(chatRequest);
                var aiReply = response.FirstChoice.Message.ToString() ?? "Sorry, my brain just hit a 404 error.";

                _logger.LogInformation("Successfully received chat reply from OpenAI for profile: {ProfileName}", profile.Name);
                return aiReply;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during OpenAI dating chat generation for profile ID: {ProfileId}", profileId);
                return "My servers are a bit overloaded, let's chat later!";
            }
        }
    }
}