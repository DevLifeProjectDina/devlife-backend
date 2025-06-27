
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DevLifeBackend.Models
{
    public class CharacterCustomization
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public int UserId { get; set; }

        public string? Hat { get; set; }
        public string? ShirtColor { get; set; }
        public string? Pet { get; set; }
    }
}