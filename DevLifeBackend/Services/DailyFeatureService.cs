// File: Services/DailyFeatureService.cs
namespace DevLifeBackend.Services
{
    public interface IDailyFeatureService
    {
        double GetLuckMultiplier(string zodiacSign);
        string GetLuckyZodiacSign();
    }

    public class DailyFeatureService : IDailyFeatureService
    {
        private readonly string[] _zodiacSigns = {
            "Aries", "Taurus", "Gemini", "Cancer", "Leo", "Virgo",
            "Libra", "Scorpio", "Sagittarius", "Capricorn", "Aquarius", "Pisces"
        };

        private const double LuckyMultiplier = 1.5;
        private const double DefaultMultiplier = 1.0;

        public string GetLuckyZodiacSign()
        {
            // The lucky sign is determined by the day of the year.
            // This makes it consistent for the whole day but different each day.
            int dayOfYear = DateTime.UtcNow.DayOfYear;
            int luckyIndex = dayOfYear % _zodiacSigns.Length;
            return _zodiacSigns[luckyIndex];
        }

        public double GetLuckMultiplier(string zodiacSign)
        {
            if (string.Equals(zodiacSign, GetLuckyZodiacSign(), StringComparison.OrdinalIgnoreCase))
            {
                return LuckyMultiplier;
            }

            return DefaultMultiplier;
        }
    }
}