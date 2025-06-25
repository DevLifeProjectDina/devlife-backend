// File: Models/GameModels.cs
namespace DevLifeBackend.Models
{
    // Describes a single "bug" obstacle in the game
    public class BugObstacle
    {
        public Guid Id { get; } = Guid.NewGuid(); // Unique ID for each bug
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public string Type { get; set; } = "Bug"; // We could have other types like "Deadline"
    }
}