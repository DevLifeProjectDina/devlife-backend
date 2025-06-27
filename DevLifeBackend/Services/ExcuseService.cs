
using DevLifeBackend.DTOs;
using Serilog;

namespace DevLifeBackend.Services
{
    public interface IExcuseService
    {
       
        ExcuseDto? GetRandomExcuse(string meetingCategory);
        List<ExcuseDto> GetAllExcuses();
    }

    public class ExcuseService : IExcuseService
    {
        private readonly ILogger<ExcuseService> _logger;
        private static readonly Random _random = new();
        private readonly List<ExcuseDto> _excuses = new()
        {
          
            new() { Id = 1, Text = "My local Kubernetes cluster just crashed, trying to bring the pods back up.", Type = "Technical", MeetingCategories = new List<string> { "Daily standup", "Sprint planning" }, BelievabilityScore = 85 },
            new() { Id = 2, Text = "I'm in the middle of a critical database migration, can't step away.", Type = "Technical", MeetingCategories = new List<string> { "Daily standup", "Sprint planning", "Client meeting" }, BelievabilityScore = 90 },
            new() { Id = 3, Text = "A rogue memory leak is eating all my RAM, I need to debug it ASAP.", Type = "Technical", MeetingCategories = new List<string> { "Daily standup" }, BelievabilityScore = 80 },

            new() { Id = 10, Text = "My cat just walked across my keyboard and deployed to production. Need to fix it!", Type = "Personal", MeetingCategories = new List<string> { "Daily standup", "Team building" }, BelievabilityScore = 60 },
            new() { Id = 11, Text = "The coffee machine is broken. This is a code red emergency.", Type = "Personal", MeetingCategories = new List<string> { "Daily standup", "Sprint planning", "Team building" }, BelievabilityScore = 50 },

         
            new() { Id = 20, Text = "I have to testify in a code-of-conduct hearing for a rogue AI.", Type = "Creative", MeetingCategories = new List<string> { "Client meeting", "Team building" }, BelievabilityScore = 20 },
            new() { Id = 21, Text = "My IDE achieved sentience and is refusing to compile until we discuss its feelings.", Type = "Creative", MeetingCategories = new List<string> { "Sprint planning", "Daily standup" }, BelievabilityScore = 15 }
        };

        public ExcuseService(ILogger<ExcuseService> logger)
        {
            _logger = logger;
        }

        public ExcuseDto? GetRandomExcuse(string meetingCategory)
        {
            _logger.LogInformation("Attempting to get a random excuse for meeting category: {MeetingCategory}", meetingCategory);

            var suitableExcuses = _excuses
                .Where(e => e.MeetingCategories.Contains(meetingCategory, StringComparer.OrdinalIgnoreCase))
                .ToList();

            if (!suitableExcuses.Any())
            {
                _logger.LogWarning("No excuses found for meeting category: {MeetingCategory}", meetingCategory);
                return null;
            }

            var excuse = suitableExcuses[_random.Next(suitableExcuses.Count)];
            _logger.LogInformation("Found excuse with ID {ExcuseId} for meeting category {MeetingCategory}", excuse.Id, meetingCategory);
            return excuse;
        }

        public List<ExcuseDto> GetAllExcuses() => _excuses;
    }
}