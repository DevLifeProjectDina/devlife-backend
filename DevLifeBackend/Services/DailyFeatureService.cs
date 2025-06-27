using Serilog;

namespace DevLifeBackend.Services
{
    public interface IDailyFeatureService
    {
        double GetLuckMultiplier(string zodiacSign);
        string GetLuckyZodiacSign();
    }

    public class DailyFeatureService : IDailyFeatureService
    {
        private readonly ILogger<DailyFeatureService> _logger;
        private readonly string[] _zodiacSigns = {
            "Aries", "Taurus", "Gemini", "Cancer", "Leo", "Virgo",
            "Libra", "Scorpio", "Sagittarius", "Capricorn", "Aquarius", "Pisces"
        };

        private const double LuckyMultiplier = 1.5;
        private const double DefaultMultiplier = 1.0;

        public DailyFeatureService(ILogger<DailyFeatureService> logger)
        {
            _logger = logger;
            // Log the lucky sign once when the service is created for a request.
            _logger.LogInformation("Today's lucky zodiac sign is {LuckySign}", GetLuckyZodiacSign());
        }

        public string GetLuckyZodiacSign()
        {
            int dayOfYear = DateTime.UtcNow.DayOfYear;
            int luckyIndex = dayOfYear % _zodiacSigns.Length;
            return _zodiacSigns[luckyIndex];
        }

        public double GetLuckMultiplier(string zodiacSign)
        {
            if (string.Equals(zodiacSign, GetLuckyZodiacSign(), StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Applying lucky multiplier for user with zodiac sign: {ZodiacSign}", zodiacSign);
                return LuckyMultiplier;
            }

            return DefaultMultiplier;
        }
    }
}