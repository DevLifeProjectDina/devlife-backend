
namespace DevLifeBackend.Models
{
    public class BugObstacle
    {
        public Guid Id { get; } = Guid.NewGuid();
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public string Type { get; set; } = "Bug";
    }
}