// File: DTOs/BugChaseDtos.cs
namespace DevLifeBackend.DTOs
{
    // DTO для эндпоинта GET /leaderboard
    public record BugChaseLeaderboardEntryDto
    {
        public required string Username { get; init; }
        public required string ExperienceLevel { get; init; }
        public int HighScore { get; init; }
    }

    // DTO для нового эндпоинта POST /submit-score
    public record SubmitScoreDto
    {
        public int Score { get; init; }
    }
}