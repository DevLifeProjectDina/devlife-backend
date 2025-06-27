using DevLifeBackend.Data;
using DevLifeBackend.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Serilog;

namespace DevLifeBackend.Hubs
{
    public class BugChaseHub : Hub
    {
        private readonly ApplicationDbContext _postgresContext;
        private readonly MongoDbContext _mongoContext;
        private readonly ILogger<BugChaseHub> _logger;

        public BugChaseHub(ApplicationDbContext postgresContext, MongoDbContext mongoContext, ILogger<BugChaseHub> logger)
        {
            _postgresContext = postgresContext;
            _mongoContext = mongoContext;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Client connected to BugChaseHub: {ConnectionId}", Context.ConnectionId);
            await Clients.Caller.SendAsync("ReceiveMessage", "Server", "Welcome to Bug Chase!");
            await Clients.Others.SendAsync("ReceiveMessage", "Server", $"Player {Context.ConnectionId} has joined the chase.");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("Client disconnected from BugChaseHub: {ConnectionId}", Context.ConnectionId);
            await Clients.All.SendAsync("ReceiveMessage", "Server", $"Player {Context.ConnectionId} has left the chase.");
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendPlayerAction(string action)
        {
            _logger.LogInformation("Player {ConnectionId} sent action: {Action}", Context.ConnectionId, action);
            await Clients.Others.SendAsync("ReceivePlayerAction", Context.ConnectionId, action);
        }

        public async Task SubmitScore(int finalScore)
        {
            var httpContext = Context.GetHttpContext();
            if (httpContext == null) return;
            var userIdString = httpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                _logger.LogWarning("Unauthorized score submission attempt from connection {ConnectionId}", Context.ConnectionId);
                return;
            }

            var user = await _postgresContext.Users.FindAsync(int.Parse(userIdString));
            if (user == null)
            {
                _logger.LogWarning("Score submission failed: User with ID {UserId} not found for connection {ConnectionId}", userIdString, Context.ConnectionId);
                return;
            }

            string message;
            if (finalScore > user.BugChaseHighScore)
            {
                _logger.LogInformation("User {Username} set a new Bug Chase high score: {NewScore} (previous: {OldScore})", user.Username, finalScore, user.BugChaseHighScore);
                user.BugChaseHighScore = finalScore;
                await _postgresContext.SaveChangesAsync();
                message = $"Congratulations! You've set a new high score: {finalScore}";
            }
            else
            {
                _logger.LogInformation("User {Username} submitted score {Score}, which is not a high score.", user.Username, finalScore);
                message = $"Good game! Your score was {finalScore}. Your high score remains {user.BugChaseHighScore}.";
            }
            await Clients.Caller.SendAsync("ReceiveGameResult", message);

            await CheckAndUnlockAchievements(userIdString, finalScore);
        }

        private async Task CheckAndUnlockAchievements(string userId, int finalScore)
        {
            if (finalScore >= 10000)
            {
                await UnlockAchievement(userId, "Bug Squasher", "Reached a score of 10,000 in Bug Chase!");
            }

            if (finalScore >= 50000)
            {
                await UnlockAchievement(userId, "Deadline Dodger", "Reached an epic score of 50,000!");
            }
        }

        private async Task UnlockAchievement(string userId, string achievementName, string description)
        {
            var existingAchievement = await _mongoContext.Achievements
                .Find(a => a.UserId == userId && a.Name == achievementName)
                .FirstOrDefaultAsync();

            if (existingAchievement == null)
            {
                var newAchievement = new Achievement
                {
                    UserId = userId,
                    Name = achievementName,
                    Description = description
                };
                await _mongoContext.Achievements.InsertOneAsync(newAchievement);

                _logger.LogInformation("User {UserId} unlocked achievement: {AchievementName}", userId, achievementName);
                await Clients.Caller.SendAsync("AchievementUnlocked", newAchievement);
            }
        }
    }
}