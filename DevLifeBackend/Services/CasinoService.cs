using DevLifeBackend.Data;
using DevLifeBackend.DTOs;
using DevLifeBackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver; // <-- Важный using
using DevLifeBackend.DTOs;

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
        private static readonly Random _random = new Random();

        public CasinoService(ApplicationDbContext postgresContext, MongoDbContext mongoContext, IDailyFeatureService dailyFeatureService)
        {
            _postgresContext = postgresContext;
            _mongoContext = mongoContext;
            _dailyFeatureService = dailyFeatureService;
        }

        public async Task<CasinoChallengeDto?> GetRandomChallengeAsync(string language, string difficulty, ISession session)
        {
            var snippets = await _mongoContext.CodeSnippets
                .Find(s => s.Language == language && s.Difficulty == difficulty)
                .ToListAsync();
            if (!snippets.Any()) return null;
            var randomSnippet = snippets[_random.Next(snippets.Count)];
            return MapSnippetToChallengeDto(randomSnippet, session);
        }

        public async Task<CasinoChallengeDto?> GetDailyChallengeAsync(string[] userStacks, ISession session)
        {
            var suitableSnippets = await _mongoContext.CodeSnippets
                .Find(s => userStacks.Contains(s.Language))
                .ToListAsync();
            if (!suitableSnippets.Any()) return null;
            var dayOfYear = DateTime.UtcNow.DayOfYear;
            var dailySnippet = suitableSnippets[dayOfYear % suitableSnippets.Count];
            return MapSnippetToChallengeDto(dailySnippet, session);
        }

        public async Task<BetResultDto> ProcessBetAsync(int userId, CasinoBetDto bet, string correctAnswer)
        {
            var user = await _postgresContext.Users.FindAsync(userId);
            if (user == null) return new BetResultDto { IsCorrect = false, Message = "User not found.", NewScore = 0 };
            if (user.Score < bet.Points || bet.Points <= 0) return new BetResultDto { IsCorrect = false, Message = "Invalid bet amount.", NewScore = user.Score };
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
                }
                user.Score += finalWinnings;
                if (multiplier > 1.0)
                {
                    message += $" (Your zodiac sign '{user.ZodiacSign}' gave you a x{multiplier} bonus!)";
                }
                await _postgresContext.SaveChangesAsync();
                return new BetResultDto { IsCorrect = true, Message = message, NewScore = user.Score };
            }
            else
            {
                user.WinStreak = 0;
                user.Score -= bet.Points;
                await _postgresContext.SaveChangesAsync();
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