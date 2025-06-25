// File: Data/MongoDbContext.cs
using DevLifeBackend.Models;
using MongoDB.Driver;

namespace DevLifeBackend.Data
{
    public class MongoDbContext
    {
        private readonly IMongoCollection<CodeSnippet> _codeSnippets;
        private readonly IMongoCollection<Achievement> _achievements;
        private readonly IMongoCollection<CharacterCustomization> _customizations; // <-- ADDED

        public MongoDbContext()
        {
            var connectionString = Environment.GetEnvironmentVariable("MONGODB_URL");
            var mongoClient = new MongoClient(connectionString);
            var mongoDatabase = mongoClient.GetDatabase("devlife_db");

            _codeSnippets = mongoDatabase.GetCollection<CodeSnippet>("CodeSnippets");
            _achievements = mongoDatabase.GetCollection<Achievement>("Achievements");
            _customizations = mongoDatabase.GetCollection<CharacterCustomization>("CharacterCustomizations"); // <-- ADDED
        }

        public IMongoCollection<CodeSnippet> CodeSnippets => _codeSnippets;
        public IMongoCollection<Achievement> Achievements => _achievements;
        public IMongoCollection<CharacterCustomization> CharacterCustomizations => _customizations; // <-- ADDED
    }
}