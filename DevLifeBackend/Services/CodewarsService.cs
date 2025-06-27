using DevLifeBackend.DTOs;
using System.Text.Json;
using Serilog;

namespace DevLifeBackend.Services
{
    public class CodewarsTask
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string Language { get; set; }
        public required string Source { get; set; }
        public required string Difficulty { get; set; }
    }

    public interface ICodewarsService
    {
        Task<CodewarsTask?> GetRandomTaskAsync(string language, string difficulty);
    }

    public class CodewarsService : ICodewarsService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<CodewarsService> _logger;
        private static readonly Random _random = new();

        private readonly List<CodewarsTask> _fallbackTasks = new()
        {
            new CodewarsTask { Language = ".NET", Name = "Fallback: Sum of positive", Description = "You get an array of numbers, return the sum of all of the positives ones.", Source = "Fallback (Hardcoded)", Difficulty = "Junior" },
            new CodewarsTask { Language = ".NET", Name = "Fallback: Human Readable Time", Description = "Write a function, which takes a non-negative integer (seconds) as input and returns the time in a human-readable format (HH:MM:SS).", Source = "Fallback (Hardcoded)", Difficulty = "Senior" },
            new CodewarsTask { Language = "Python", Name = "Fallback: Reversed Strings", Description = "Complete the solution so that it reverses the string passed into it.", Source = "Fallback (Hardcoded)", Difficulty = "Junior" },
            new CodewarsTask { Language = "Python", Name = "Fallback: Valid Braces", Description = "Write a function that takes a string of braces, and determines if the order of the braces is valid.", Source = "Fallback (Hardcoded)", Difficulty = "Senior" },
            new CodewarsTask { Language = "Angular", Name = "Fallback: Is it a palindrome?", Description = "Write a function that checks if a given string (case-insensitive) is a palindrome.", Source = "Fallback (Hardcoded)", Difficulty = "Senior" },
            new CodewarsTask { Language = "React", Name = "Fallback: Convert a Number to a String", Description = "We need a function that can transform a number (integer) into a string.", Source = "Fallback (Hardcoded)", Difficulty = "Junior" }
        };

        private readonly Dictionary<string, Dictionary<string, List<string>>> _challengeSlugs = new()
        {
            { ".NET", new Dictionary<string, List<string>> {
                { "Junior", new List<string> { "even-or-odd", "opposite-number" } },
                { "Middle", new List<string> { "sum-of-positive" } },
                { "Senior", new List<string> { "human-readable-duration-format" } }
            }},
            { "Python", new Dictionary<string, List<string>> {
                { "Junior", new List<string> { "reversed-strings", "is-he-gonna-survive" } },
                { "Middle", new List<string> { "jennys-secret-message" } },
                { "Senior", new List<string> { "valid-braces" } }
            }},
            { "Angular", new Dictionary<string, List<string>> {
                { "Junior", new List<string> { "string-repeat", "remove-string-spaces" } },
                { "Middle", new List<string> { "sum-without-highest-and-lowest-number" } },
                { "Senior", new List<string> { "is-it-a-palindrome" } }
            }},
            { "React", new Dictionary<string, List<string>> {
                { "Junior", new List<string> { "convert-a-number-to-a-string", "function-1-hello-world" } },
                { "Middle", new List<string> { "grasshopper-summation" } },
                { "Senior", new List<string> { "find-the-odd-int" } }
            }}
        };

        public CodewarsService(IHttpClientFactory clientFactory, ILogger<CodewarsService> logger)
        {
            _clientFactory = clientFactory;
            _logger = logger;
        }

        public async Task<CodewarsTask?> GetRandomTaskAsync(string language, string difficulty)
        {
            try
            {
                if (!_challengeSlugs.ContainsKey(language) || !_challengeSlugs[language].ContainsKey(difficulty))
                {
                    _logger.LogWarning("No slugs defined for Language {Language} and Difficulty {Difficulty}. Using fallback.", language, difficulty);
                    return GetFallbackTask(language, difficulty);
                }

                var slugs = _challengeSlugs[language][difficulty];
                if (!slugs.Any())
                {
                    _logger.LogWarning("Slug list is empty for Language {Language} and Difficulty {Difficulty}. Using fallback.", language, difficulty);
                    return GetFallbackTask(language, difficulty);
                }

                var randomSlug = slugs[_random.Next(slugs.Count)];
                _logger.LogInformation("Attempting to fetch challenge '{Slug}' from Codewars API.", randomSlug);

                var client = _clientFactory.CreateClient("CodewarsClient");
                var response = await client.GetAsync($"https://www.codewars.com/api/v1/code-challenges/{randomSlug}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Codewars API returned non-success status: {StatusCode} for slug '{Slug}'. Using fallback.", response.StatusCode, randomSlug);
                    return GetFallbackTask(language, difficulty);
                }

                var jsonString = await response.Content.ReadAsStringAsync();
                var challengeDto = JsonSerializer.Deserialize<CodewarsChallengeDto>(jsonString);

                if (challengeDto != null)
                {
                    _logger.LogInformation("Successfully fetched and deserialized challenge '{Slug}' from Codewars API.", randomSlug);
                    return new CodewarsTask
                    {
                        Name = challengeDto.Name,
                        Description = challengeDto.Description,
                        Language = language,
                        Source = "Codewars API",
                        Difficulty = difficulty
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while fetching from Codewars API. Using fallback.");
                return GetFallbackTask(language, difficulty);
            }

            _logger.LogWarning("Failed to process Codewars API response. Using fallback.");
            return GetFallbackTask(language, difficulty);
        }

        private CodewarsTask? GetFallbackTask(string language, string difficulty)
        {
            _logger.LogInformation("Attempting to find a fallback task for Language {Language} and Difficulty {Difficulty}", language, difficulty);
            var suitableTasks = _fallbackTasks.Where(c => c.Language == language && c.Difficulty == difficulty).ToList();
            if (!suitableTasks.Any())
            {
                _logger.LogError("No fallback task found for Language {Language} and Difficulty {Difficulty}", language, difficulty);
                return null;
            }

            return suitableTasks[_random.Next(suitableTasks.Count)];
        }
    }
}