// File: Models/CodeSnippet.cs
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DevLifeBackend.Models
{
    public class CodeSnippet
    {
        [BsonId] // Indicates this is the primary key
        [BsonRepresentation(BsonType.ObjectId)] // Store it as an ObjectId in Mongo, but use it as a string in C#
        public string? Id { get; set; }

        [BsonElement("Language")] // Good practice to explicitly name the field in the database
        public required string Language { get; set; }

        public required string CorrectCode { get; set; }
        public required string BuggyCode { get; set; }
        public required string Difficulty { get; set; }
        public required string Source { get; set; }
        public string? Description { get; set; }
    }
}