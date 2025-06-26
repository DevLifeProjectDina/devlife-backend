// File: Models/ProfileModels.cs
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DevLifeBackend.Models
{
    public class CharacterCustomization
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        // Link to the user in PostgreSQL
        public int UserId { get; set; }

        // Customizable parts
        public string? Hat { get; set; }
        public string? ShirtColor { get; set; }
        public string? Pet { get; set; } // e.g., "Dragon", "Rubber Duck"
    }
}