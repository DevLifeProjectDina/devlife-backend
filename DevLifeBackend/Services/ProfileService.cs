using DevLifeBackend.Data;
using DevLifeBackend.Models;
using MongoDB.Driver;
using Serilog;

namespace DevLifeBackend.Services
{
    public interface IProfileService
    {
        Task<CharacterCustomization> GetCustomizationAsync(int userId);
        Task SaveCustomizationAsync(int userId, CharacterCustomization newCustomization);
    }

    public class ProfileService : IProfileService
    {
        private readonly MongoDbContext _mongoContext;
        private readonly ILogger<ProfileService> _logger;

        public ProfileService(MongoDbContext mongoContext, ILogger<ProfileService> logger)
        {
            _mongoContext = mongoContext;
            _logger = logger;
        }

        public async Task<CharacterCustomization> GetCustomizationAsync(int userId)
        {
            _logger.LogInformation("Fetching character customization for User ID: {UserId}", userId);
            var customization = await _mongoContext.CharacterCustomizations
                .Find(c => c.UserId == userId)
                .FirstOrDefaultAsync();

            if (customization == null)
            {
                _logger.LogInformation("No existing customization found for User ID: {UserId}. Returning a new default profile.", userId);
                return new CharacterCustomization { UserId = userId };
            }

            return customization;
        }

        public async Task SaveCustomizationAsync(int userId, CharacterCustomization newCustomization)
        {
            newCustomization.UserId = userId;

            var filter = Builders<CharacterCustomization>.Filter.Eq(c => c.UserId, userId);

            var existing = await _mongoContext.CharacterCustomizations.Find(filter).FirstOrDefaultAsync();

            if (existing != null)
            {
                newCustomization.Id = existing.Id; 
            }

            _logger.LogInformation("Saving character customization for User ID: {UserId}", userId);

            await _mongoContext.CharacterCustomizations
                .ReplaceOneAsync(filter, newCustomization, new ReplaceOptions { IsUpsert = true });
        }
    }
}