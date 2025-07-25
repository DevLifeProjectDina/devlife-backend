﻿
using DevLifeBackend.Enums;

namespace DevLifeBackend.DTOs
{
    public record BugChaseLeaderboardEntryDto
    {
        public required string Username { get; init; }
        public required ExperienceLevel ExperienceLevel { get; init; }
        public int HighScore { get; init; }
    }

    public record SubmitScoreDto
    {
        public int Score { get; init; }
    }
}