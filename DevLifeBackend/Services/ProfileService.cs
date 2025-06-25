// File: Services/ProfileService.cs
using DevLifeBackend.Data;
using DevLifeBackend.Models;
using MongoDB.Driver;

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

        public ProfileService(MongoDbContext mongoContext)
        {
            _mongoContext = mongoContext;
        }

        public async Task<CharacterCustomization> GetCustomizationAsync(int userId)
        {
            var customization = await _mongoContext.CharacterCustomizations
                .Find(c => c.UserId == userId)
                .FirstOrDefaultAsync();

            // If no customization exists, return a default one
            return customization ?? new CharacterCustomization { UserId = userId };
        }

        public async Task SaveCustomizationAsync(int userId, CharacterCustomization newCustomization)
        {
            var filter = Builders<CharacterCustomization>.Filter.Eq(c => c.UserId, userId);

            // This will update the existing document or insert a new one if it doesn't exist
            await _mongoContext.CharacterCustomizations
                .ReplaceOneAsync(filter, newCustomization, new ReplaceOptions { IsUpsert = true });
        }
    }
}