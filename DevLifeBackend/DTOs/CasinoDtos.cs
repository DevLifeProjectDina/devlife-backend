// File: DTOs/CasinoDtos.cs
namespace DevLifeBackend.DTOs
{
    // 1. DTO для получения задачи казино
    public record CasinoChallengeDto
    {
        public string? SnippetId { get; init; } // Changed to string for MongoDB compatibility
        public required string Description { get; init; }
        public required string Language { get; init; }
        public required string CodeOptionA { get; init; }
        public required string CodeOptionB { get; init; }
        public required string Source { get; init; }
    }

    // 2. DTO для отправки ставки
    public record CasinoBetDto
    {
        public string? SnippetId { get; init; } // Changed to string for MongoDB compatibility
        public required string ChosenOption { get; init; }
        public int Points { get; init; }
    }

    // 3. DTO для ответа на ставку (которого у вас не было)
    public record BetResultDto
    {
        public bool IsCorrect { get; init; }
        public required string Message { get; init; }
        public int NewScore { get; init; }
    }

    // 4. DTO для таблицы лидеров (которого у вас не было)
    public record LeaderboardEntryDto
    {
        public required string Username { get; init; }
        public required string Stack { get; init; }
        public int Score { get; init; }
    }
}