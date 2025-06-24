namespace DevLifeBackend.DTOs
{
    public record CasinoChallengeDto
    {
        public int SnippetId { get; init; }
        public required string Description { get; init; }
        public required string Language { get; init; }
        public required string CodeOptionA { get; init; }
        public required string CodeOptionB { get; init; }
        public required string Source { get; init; }
    }

    public record CasinoBetDto
    {
        public int SnippetId { get; init; }
        public required string ChosenOption { get; init; }
        public int Points { get; init; }
    }

    public record BetResultDto
    {
        public bool IsCorrect { get; init; }
        public required string Message { get; init; }
        public int NewScore { get; init; }
    }

    public record LeaderboardEntryDto
    {
        public required string Username { get; init; }
        public required string Stack { get; init; }
        public int Score { get; init; }
    }
}