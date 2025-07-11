
using DevLifeBackend.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DevLifeBackend.Models
{
    public class CodeSnippet
    {
        [BsonId] 
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("Language")]
        public required TechStack Language { get; set; }

        public required string CorrectCode { get; set; }
        public required string BuggyCode { get; set; }
        public required ExperienceLevel Difficulty { get; set; }
        public required string Source { get; set; }
        public string? Description { get; set; }
    }
}