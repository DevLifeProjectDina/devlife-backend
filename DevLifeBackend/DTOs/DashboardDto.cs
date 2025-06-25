// File: DTOs/DashboardDto.cs
namespace DevLifeBackend.DTOs
{
    public record DashboardDto
    {
        public string WelcomeMessage { get; init; } = default!;
        public string DailyHoroscope { get; init; } = default!;
        public string LuckyTechnology { get; init; } = default!;
        public string DailyBonusInfo { get; init; } = default!;
        public int WinStreak { get; init; } // <-- ADDED THIS
    }
}