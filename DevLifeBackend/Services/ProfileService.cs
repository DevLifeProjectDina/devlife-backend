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

        //public async Task SaveCustomizationAsync(int userId, CharacterCustomization newCustomization)
        //{
        //    var filter = Builders<CharacterCustomization>.Filter.Eq(c => c.UserId, userId);

        //    // This will update the existing document or insert a new one if it doesn't exist
        //    await _mongoContext.CharacterCustomizations
        //        .ReplaceOneAsync(filter, newCustomization, new ReplaceOptions { IsUpsert = true });
        //}
        public async Task SaveCustomizationAsync(int userId, CharacterCustomization newCustomization)
        {
            // 1. Убедитесь, что UserId в объекте newCustomization совпадает с переданным userId.
            // Это важно, так как newCustomization приходит извне (например, из HTTP-запроса),
            // и его UserId может быть некорректным или отсутствовать.
            newCustomization.UserId = userId;

            // 2. Попробуйте найти существующую кастомизацию по UserId.
            // Это более явный подход, чем полагаться только на upsert с _id: null.
            var existingCustomization = await _mongoContext.CharacterCustomizations
                .Find(c => c.UserId == userId)
                .FirstOrDefaultAsync();

            if (existingCustomization != null)
            {
                // Если кастомизация для этого пользователя уже существует:
                // Обновим её поля из newCustomization.
                // Важно: Id существующей кастомизации сохраняется!
                newCustomization.Id = existingCustomization.Id; // Переносим _id от существующего документа

                // Теперь newCustomization содержит правильный Id существующего документа.
                // Выполняем замену:
                // Фильтр должен быть по _id, чтобы гарантировать, что обновляем правильный документ.
                var result = await _mongoContext.CharacterCustomizations
                    .ReplaceOneAsync(
                        c => c.Id == existingCustomization.Id, // Фильтр по уникальному _id
                        newCustomization); // newCustomization теперь содержит правильный _id
            }
            else
            {
                // Если кастомизация для этого пользователя НЕ существует:
                // Это новая запись. Убедимся, что Id в newCustomization равен null,
                // чтобы MongoDB Driver сгенерировал новый ObjectId при вставке.
                newCustomization.Id = null; // Убеждаемся, что Id для новой записи null

                // Вставляем новый документ. InsertOneAsync более прямолинеен для новых записей.
                await _mongoContext.CharacterCustomizations.InsertOneAsync(newCustomization);
            }
        }
    }
}