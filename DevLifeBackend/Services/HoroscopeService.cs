
using OpenAI;
using OpenAI.Chat;
using Serilog;
using DevLifeBackend.Settings; 
using Microsoft.Extensions.Options;

namespace DevLifeBackend.Services
{
    public interface IHoroscopeService
    {
        Task<string> GetHoroscopeAsync(string? zodiacSign);
    }

    public class HoroscopeService : IHoroscopeService
    {
        private readonly OpenAIClient _openAIClient;
        private readonly ILogger<HoroscopeService> _logger;

        public HoroscopeService(OpenAIClient openAIClient, ILogger<HoroscopeService> logger)
        {
            _openAIClient = openAIClient;
            _logger = logger;
        }

        public async Task<string> GetHoroscopeAsync(string? zodiacSign)
        {
            if (string.IsNullOrEmpty(zodiacSign) || zodiacSign == "Unknown")
            {
                _logger.LogWarning("Zodiac sign is '{ZodiacSign}', returning fallback horoscope.", zodiacSign);
                return GetFallbackHoroscope(zodiacSign);
            }

            try
            {
                var prompt =
                    $"You are a fun and witty astrologer for software developers. " +
                    $"Write a short, quirky, one-paragraph horoscope for the zodiac sign '{zodiacSign}'. " +
                    "The horoscope must be related to programming, code, or tech life. Be creative.";

                _logger.LogInformation("Requesting developer horoscope from OpenAI for {ZodiacSign}", zodiacSign);
                var chatRequest = new ChatRequest(new[] { new Message(Role.System, prompt) }, "gpt-3.5-turbo", maxTokens: 150);
                var response = await _openAIClient.ChatEndpoint.GetCompletionAsync(chatRequest);
                var aiHoroscope = response.FirstChoice.Message.ToString();

                if (!string.IsNullOrWhiteSpace(aiHoroscope))
                {
                    _logger.LogInformation("Successfully received developer horoscope from OpenAI for {ZodiacSign}", zodiacSign);
                    return aiHoroscope;
                }

                _logger.LogWarning("OpenAI returned an empty response. Using fallback horoscope.");
                return GetFallbackHoroscope(zodiacSign);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OpenAI call failed for {ZodiacSign}. Using fallback horoscope.", zodiacSign);
                return GetFallbackHoroscope(zodiacSign);
            }
        }

        private string GetFallbackHoroscope(string? zodiacSign)
        {
            _logger.LogInformation("Providing fallback horoscope for {ZodiacSign}", zodiacSign);
            var horoscopes = new Dictionary<string, string> {
                { "Aries", "Aries: Today is a great day to tackle that complex algorithm. Your energetic spirit is perfectly aligned for problem-solving!" },
                { "Taurus", "Taurus: Patience is your virtue. Take time to refactor some old code. You'll find satisfaction in cleaning things up." },
                { "Gemini", "Gemini: Your communicative skills are sharp today. Perfect for writing clear documentation or pair programming." },
                { "Cancer", "Cancer: You're in a protective mood. A good day to focus on security audits and writing robust tests." },
                { "Leo", "Leo: Time to shine! Present your latest feature in the team meeting. Your confidence will win everyone over." },
                { "Virgo", "Virgo: Your attention to detail is at its peak. Hunt down those pesky bugs that no one else can find." },
                { "Libra", "Libra: Strive for balance in your codebase. A good day to work on harmonizing the UI with the backend logic." },
                { "Scorpio", "Scorpio: Your focus is intense. A perfect day for deep-diving into a new technology or framework." },
                { "Sagittarius", "Sagittarius: Your adventurous spirit calls for exploration. Try out a new library or a different programming language." },
                { "Capricorn", "Capricorn: Discipline is your strength. A great day for optimizing performance and making your application run faster." },
                { "Aquarius", "Aquarius: Your innovative ideas are flowing. Brainstorm a new side project or a creative solution to an old problem." },
                { "Pisces", "Pisces: Your intuition is high. Trust your gut when debugging or making architectural decisions." },
                { "Unknown", "The stars are mysterious today, but one thing is clear: a good day of coding awaits!" }
            };
            return horoscopes.GetValueOrDefault(zodiacSign ?? "Unknown", horoscopes["Unknown"]);
        }
    }
}