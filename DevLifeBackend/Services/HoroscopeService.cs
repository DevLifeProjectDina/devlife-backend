// File: Services/HoroscopeService.cs
using System.Text.Json;
using DevLifeBackend.DTOs;
using OpenAI.Chat;
using OpenAI;

namespace DevLifeBackend.Services
{
    public interface IHoroscopeService
    {
        Task<string> GetHoroscopeAsync(string? zodiacSign);
    }

    public class HoroscopeService : IHoroscopeService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly OpenAIClient _openAIClient;

        // We now inject the base OpenAIClient
        public HoroscopeService(IHttpClientFactory clientFactory, OpenAIClient openAIClient)
        {
            _clientFactory = clientFactory;
            _openAIClient = openAIClient;
        }

        public async Task<string> GetHoroscopeAsync(string? zodiacSign)
        {
            var generalHoroscope = await GetGeneralHoroscopeAsync(zodiacSign);

            if (generalHoroscope.Source == "Fallback")
            {
                return generalHoroscope.Text;
            }

            try
            {
                var prompt =
                    "Rewrite the following general horoscope as a fun, quirky, and short piece of advice for a software developer. " +
                    "Keep the original astrological mood. For example, if the horoscope is about 'new beginnings', " +
                    "the advice could be about 'starting a new side project'. Return only the rewritten advice. " +
                    $"The horoscope is: \"{generalHoroscope.Text}\"";

                // The new way to make a chat completion request
                var chatRequest = new ChatRequest(new[]
                {
                    new Message(Role.System, prompt)
                });

                var response = await _openAIClient.ChatEndpoint.GetCompletionAsync(chatRequest);

                var rewrittenAdvice = response.FirstChoice.Message.Content;
                return rewrittenAdvice ?? generalHoroscope.Text;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OpenAI rewrite failed: {ex.Message}. Returning general horoscope.");
            }

            return generalHoroscope.Text;
        }

        // This private method remains mostly the same
        private async Task<(string Text, string Source)> GetGeneralHoroscopeAsync(string? zodiacSign)
        {
            // ... (The implementation of this method does not need to change)
            if (string.IsNullOrEmpty(zodiacSign) || zodiacSign == "Unknown")
            {
                return (GetFallbackHoroscope(zodiacSign), "Fallback");
            }
            try
            {
                var sign = zodiacSign.ToLower();
                var requestUrl = $"https://aztro.sameer.guru/?sign={sign}&day=today";
                var client = _clientFactory.CreateClient("HoroscopeClient");
                var response = await client.PostAsync(requestUrl, null);
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                var aztroData = JsonSerializer.Deserialize<AztroDto>(jsonString);
                return (aztroData?.Description ?? GetFallbackHoroscope(zodiacSign), "API");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Aztro API call failed: {ex.Message}. Using fallback horoscope.");
                return (GetFallbackHoroscope(zodiacSign), "Fallback");
            }
        }

        private string GetFallbackHoroscope(string? zodiacSign)
        {
            // ... (The implementation of this method does not need to change)
            var horoscopes = new Dictionary<string, string> {
                    { "Aries", "Today is a great day to tackle that complex algorithm. Your energetic spirit is perfectly aligned for problem-solving!" },
                    { "Taurus", "Patience is your virtue. Take time to refactor some old code. You'll find satisfaction in cleaning things up." },
                    { "Pisces", "Your intuition is high. Trust your gut when debugging or making architectural decisions." },
                    { "Unknown", "The stars are mysterious today, but one thing is clear: a good day of coding awaits!" }
                };
            return horoscopes.GetValueOrDefault(zodiacSign ?? "Unknown", horoscopes["Unknown"]);
        }
    }
}