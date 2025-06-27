using DevLifeBackend.Data;
using DevLifeBackend.DTOs;
using DevLifeBackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Serilog;

namespace DevLifeBackend.Services
{
    public interface ICasinoService
    {
        Task<CasinoChallengeDto?> GetRandomChallengeAsync(string language, string difficulty, ISession session);
        Task<BetResultDto> ProcessBetAsync(int userId, CasinoBetDto bet, string correctAnswer);
        Task<IEnumerable<LeaderboardEntryDto>> GetLeaderboardAsync();
        Task<CasinoChallengeDto?> GetDailyChallengeAsync(string[] userStacks, ISession session);
    }

    public class CasinoService : ICasinoService
    {
        private readonly ApplicationDbContext _postgresContext;
        private readonly MongoDbContext _mongoContext;
        private readonly IDailyFeatureService _dailyFeatureService;
        private readonly ILogger<CasinoService> _logger;
        private static readonly Random _random = new();

        public CasinoService(ApplicationDbContext postgresContext, MongoDbContext mongoContext, IDailyFeatureService dailyFeatureService, ILogger<CasinoService> logger)
        {
            _postgresContext = postgresContext;
            _mongoContext = mongoContext;
            _dailyFeatureService = dailyFeatureService;
            _logger = logger;
        }

        public async Task<CasinoChallengeDto?> GetRandomChallengeAsync(string language, string difficulty, ISession session)
        {
            _logger.LogInformation("Fetching random casino challenge for Language: {Language}, Difficulty: {Difficulty}", language, difficulty);
            var snippets = await _mongoContext.CodeSnippets
                .Find(s => s.Language == language && s.Difficulty == difficulty)
                .ToListAsync();

            if (!snippets.Any())
            {
                _logger.LogWarning("No random casino challenge found for Language: {Language}, Difficulty: {Difficulty}", language, difficulty);
                return null;
            }

            var randomSnippet = snippets[_random.Next(snippets.Count)];
            _logger.LogInformation("Found random challenge with ID: {SnippetId}", randomSnippet.Id);
            return MapSnippetToChallengeDto(randomSnippet, session);
        }

        public async Task<CasinoChallengeDto?> GetDailyChallengeAsync(string[] userStacks, ISession session)
        {
            _logger.LogInformation("Fetching daily challenge for user stacks: {Stacks}", userStacks);
            var suitableSnippets = await _mongoContext.CodeSnippets
                .Find(s => userStacks.Contains(s.Language))
                .ToListAsync();

            if (!suitableSnippets.Any())
            {
                _logger.LogWarning("No suitable daily challenge found for user stacks: {Stacks}", userStacks);
                return null;
            }

            var dayOfYear = DateTime.UtcNow.DayOfYear;
            var dailySnippet = suitableSnippets[dayOfYear % suitableSnippets.Count];
            _logger.LogInformation("Today's daily challenge is Snippet with ID: {SnippetId}", dailySnippet.Id);
            return MapSnippetToChallengeDto(dailySnippet, session);
        }

        public async Task<BetResultDto> ProcessBetAsync(int userId, CasinoBetDto bet, string correctAnswer)
        {
            var user = await _postgresContext.Users.FindAsync(userId);
            if (user == null)
            {
                _logger.LogError("Bet processing failed: User with ID {UserId} not found.", userId);
                return new BetResultDto { IsCorrect = false, Message = "User not found.", NewScore = 0 };
            }

            if (user.Score < bet.Points || bet.Points <= 0)
            {
                _logger.LogWarning("User {Username} made an invalid bet of {Points} with a current score of {Score}", user.Username, bet.Points, user.Score);
                return new BetResultDto { IsCorrect = false, Message = "Invalid bet amount.", NewScore = user.Score };
            }

            var isBetCorrect = bet.ChosenOption.Equals(correctAnswer, StringComparison.OrdinalIgnoreCase);

            if (isBetCorrect)
            {
                user.WinStreak++;
                var multiplier = _dailyFeatureService.GetLuckMultiplier(user.ZodiacSign!);
                var baseWinnings = bet.Points * 2;
                var finalWinnings = (int)(baseWinnings * multiplier);
                var message = $"Correct! You won {finalWinnings} points.";

                if (user.WinStreak >= 3)
                {
                    int streakBonus = 50;
                    user.Score += streakBonus;
                    message += $" You also get a {streakBonus} points bonus for a win streak of {user.WinStreak}!";
                    _logger.LogInformation("User {Username} received a streak bonus of {BonusPoints}", user.Username, streakBonus);
                }

                user.Score += finalWinnings;

                if (multiplier > 1.0)
                {
                    message += $" (Your zodiac sign '{user.ZodiacSign}' gave you a x{multiplier} bonus!)";
                }

                await _postgresContext.SaveChangesAsync();
                _logger.LogInformation("User {Username} won bet on snippet {SnippetId}. New score: {NewScore}. Win streak: {WinStreak}", user.Username, bet.SnippetId, user.Score, user.WinStreak);
                return new BetResultDto { IsCorrect = true, Message = message, NewScore = user.Score };
            }
            else
            {
                var oldStreak = user.WinStreak;
                user.WinStreak = 0;
                user.Score -= bet.Points;
                await _postgresContext.SaveChangesAsync();
                _logger.LogInformation("User {Username} lost bet on snippet {SnippetId}. Win streak reset from {OldStreak}. New score: {NewScore}", user.Username, bet.SnippetId, oldStreak, user.Score);
                return new BetResultDto { IsCorrect = false, Message = $"Wrong! You lost {bet.Points} points.", NewScore = user.Score };
            }
        }

        public async Task<IEnumerable<LeaderboardEntryDto>> GetLeaderboardAsync()
        {
            var topUsers = await _postgresContext.Users.OrderByDescending(u => u.Score).Take(10)
                .Select(u => new LeaderboardEntryDto { Username = u.Username, Stack = string.Join(", ", u.Stacks), Score = u.Score })
                .ToListAsync();
            return topUsers;
        }

        private CasinoChallengeDto MapSnippetToChallengeDto(CodeSnippet snippet, ISession session)
        {
            var isCorrectCodeOptionA = _random.Next(2) == 0;
            var correctOptionKey = $"CasinoAnswer_{snippet.Id}";
            var correctOptionValue = isCorrectCodeOptionA ? "A" : "B";
            session.SetString(correctOptionKey, correctOptionValue);
            return new CasinoChallengeDto { SnippetId = snippet.Id, Description = snippet.Description ?? "Guess which code snippet works correctly!", Language = snippet.Language, CodeOptionA = isCorrectCodeOptionA ? snippet.CorrectCode : snippet.BuggyCode, CodeOptionB = isCorrectCodeOptionA ? snippet.BuggyCode : snippet.CorrectCode, Source = snippet.Source };
        }
    }
}