// File: Services/CasinoService.cs
using DevLifeBackend.Data;
using DevLifeBackend.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace DevLifeBackend.Services
{
    public interface ICasinoService
    {
        // --- FIX IS HERE: I've added the missing method declarations back ---
        Task<CasinoChallengeDto?> GetRandomChallengeAsync(string language, string difficulty, ISession session);
        Task<BetResultDto> ProcessBetAsync(int userId, CasinoBetDto bet, string correctAnswer);
        Task<IEnumerable<LeaderboardEntryDto>> GetLeaderboardAsync();
    }

    public class CasinoService : ICasinoService
    {
        private readonly ApplicationDbContext _context;
        private readonly IDailyFeatureService _dailyFeatureService;
        private static readonly Random _random = new Random();

        public CasinoService(ApplicationDbContext context, IDailyFeatureService dailyFeatureService)
        {
            _context = context;
            _dailyFeatureService = dailyFeatureService;
        }

        public async Task<CasinoChallengeDto?> GetRandomChallengeAsync(string language, string difficulty, ISession session)
        {
            var snippetsForUser = await _context.CodeSnippets
                .Where(s => s.Language == language && s.Difficulty == difficulty)
                .ToListAsync();

            if (!snippetsForUser.Any()) return null;

            var randomSnippet = snippetsForUser[_random.Next(snippetsForUser.Count)];
            var isCorrectCodeOptionA = _random.Next(2) == 0;
            var correctOptionKey = $"CasinoAnswer_{randomSnippet.Id}";
            var correctOptionValue = isCorrectCodeOptionA ? "A" : "B";
            session.SetString(correctOptionKey, correctOptionValue);

            return new CasinoChallengeDto
            {
                SnippetId = randomSnippet.Id,
                Description = randomSnippet.Description ?? "Guess which code snippet works correctly!",
                Language = randomSnippet.Language,
                CodeOptionA = isCorrectCodeOptionA ? randomSnippet.CorrectCode : randomSnippet.BuggyCode,
                CodeOptionB = isCorrectCodeOptionA ? randomSnippet.BuggyCode : randomSnippet.CorrectCode,
                Source = randomSnippet.Source
            };
        }

        public async Task<BetResultDto> ProcessBetAsync(int userId, CasinoBetDto bet, string correctAnswer)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return new BetResultDto { IsCorrect = false, Message = "User not found.", NewScore = 0 };
            if (user.Score < bet.Points || bet.Points <= 0) return new BetResultDto { IsCorrect = false, Message = "Invalid bet amount.", NewScore = user.Score };

            var isBetCorrect = bet.ChosenOption.Equals(correctAnswer, StringComparison.OrdinalIgnoreCase);

            if (isBetCorrect)
            {
                var multiplier = _dailyFeatureService.GetLuckMultiplier(user.ZodiacSign!);
                var baseWinnings = bet.Points * 2;
                var finalWinnings = (int)(baseWinnings * multiplier);
                user.Score += finalWinnings;
                await _context.SaveChangesAsync();

                var message = $"Correct! You won {finalWinnings} points.";
                if (multiplier > 1.0)
                {
                    message += $" (Your zodiac sign '{user.ZodiacSign}' gave you a x{multiplier} bonus!)";
                }

                return new BetResultDto { IsCorrect = true, Message = message, NewScore = user.Score };
            }
            else
            {
                user.Score -= bet.Points;
                await _context.SaveChangesAsync();
                return new BetResultDto { IsCorrect = false, Message = $"Wrong! You lost {bet.Points} points.", NewScore = user.Score };
            }
        }

        public async Task<IEnumerable<LeaderboardEntryDto>> GetLeaderboardAsync()
        {
            var topUsers = await _context.Users
                .OrderByDescending(u => u.Score)
                .Take(10)
                .Select(u => new LeaderboardEntryDto
                {
                    Username = u.Username,
                    Stack = string.Join(", ", u.Stacks),
                    Score = u.Score
                })
                .ToListAsync();
            return topUsers;
        }
    }
}