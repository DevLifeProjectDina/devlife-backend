// File: DTOs/DashboardDto.cs
namespace DevLifeBackend.DTOs
{
    public record DashboardDto
    {
        public string WelcomeMessage { get; init; } = default!;
        public string DailyHoroscope { get; init; } = default!;
        public string LuckyTechnology { get; init; } = default!;
        // The property is renamed for better clarity
        public string DailyBonusInfo { get; init; } = default!;
    }
}