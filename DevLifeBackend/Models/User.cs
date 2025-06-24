// File: Models/User.cs
namespace DevLifeBackend.Models
{
    public class User
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string Name { get; set; }
        public required string Surname { get; set; }
        public DateTime DateOfBirth { get; set; }
        public required string[] Stacks { get; set; } // CHANGED FROM string Stack
        public required string ExperienceLevel { get; set; }
        public string? ZodiacSign { get; set; }
        public int Score { get; set; } = 100;
    }
}