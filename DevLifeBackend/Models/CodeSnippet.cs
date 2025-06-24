using System.ComponentModel.DataAnnotations;

namespace DevLifeBackend.Models
{
    public class CodeSnippet
    {
        public int Id { get; set; }
        public required string Language { get; set; }
        public required string CorrectCode { get; set; }
        public required string BuggyCode { get; set; }
        public required string Difficulty { get; set; }
        public required string Source { get; set; }
        public string? Description { get; set; }
    }
}