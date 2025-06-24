// File: Services/ZodiacService.cs
namespace DevLifeBackend.Services
{
    public interface IZodiacService
    {
        string GetZodiacSign(DateTime dateOfBirth);
    }

    public class ZodiacService : IZodiacService
    {
        public string GetZodiacSign(DateTime dateOfBirth)
        {
            int month = dateOfBirth.Month;
            int day = dateOfBirth.Day;
            switch (month)
            {
                case 1: return day <= 20 ? "Capricorn" : "Aquarius";
                case 2: return day <= 18 ? "Aquarius" : "Pisces";
                case 3: return day <= 20 ? "Pisces" : "Aries";
                case 4: return day <= 20 ? "Aries" : "Taurus";
                case 5: return day <= 20 ? "Taurus" : "Gemini";
                case 6: return day <= 21 ? "Gemini" : "Cancer";
                case 7: return day <= 22 ? "Cancer" : "Leo";
                case 8: return day <= 22 ? "Leo" : "Virgo";
                case 9: return day <= 22 ? "Virgo" : "Libra";
                case 10: return day <= 23 ? "Libra" : "Scorpio";
                case 11: return day <= 22 ? "Scorpio" : "Sagittarius";
                case 12: return day <= 21 ? "Sagittarius" : "Capricorn";
                default: return "Unknown";
            }
        }
    }
}