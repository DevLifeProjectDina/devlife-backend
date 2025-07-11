
using DevLifeBackend.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DevLifeBackend.Models
{
    public class DatingProfile
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public required string Name { get; set; }
        public int Age { get; set; }
        public required TechStack Stacks { get; set; }
        public required string Bio { get; set; }
        public required string CharacterPrompt { get; set; } 
    }
}