// File: Models/Achievement.cs
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DevLifeBackend.Models
{
    public class Achievement
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public required string UserId { get; set; } // To link it to a user
        public required string Name { get; set; }
        public required string Description { get; set; }
        public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;
    }
}