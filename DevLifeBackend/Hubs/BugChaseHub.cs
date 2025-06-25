// File: Hubs/BugChaseHub.cs
using DevLifeBackend.Data;
using DevLifeBackend.Models; // <-- Add this
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver; // <-- Add this

namespace DevLifeBackend.Hubs
{
    public class BugChaseHub : Hub
    {
        private readonly ApplicationDbContext _postgresContext;
        private readonly MongoDbContext _mongoContext; // Inject MongoDbContext

        public BugChaseHub(ApplicationDbContext postgresContext, MongoDbContext mongoContext)
        {
            _postgresContext = postgresContext;
            _mongoContext = mongoContext;
        }

        // ... (OnConnectedAsync, OnDisconnectedAsync, SendPlayerAction остаются без изменений)

        public async Task SubmitScore(int finalScore)
        {
            var httpContext = Context.GetHttpContext();
            if (httpContext == null) return;
            var userIdString = httpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString)) return;

            var user = await _postgresContext.Users.FindAsync(int.Parse(userIdString));
            if (user == null) return;

            string message;
            if (finalScore > user.BugChaseHighScore)
            {
                user.BugChaseHighScore = finalScore;
                await _postgresContext.SaveChangesAsync();
                message = $"Congratulations! You've set a new high score: {finalScore}";
            }
            else
            {
                message = $"Good game! Your score was {finalScore}. Your high score remains {user.BugChaseHighScore}.";
            }
            await Clients.Caller.SendAsync("ReceiveGameResult", message);

            // --- ACHIEVEMENT LOGIC ---
            await CheckAndUnlockAchievements(userIdString, finalScore);
        }

        private async Task CheckAndUnlockAchievements(string userId, int finalScore)
        {
            // Check for "Bug Squasher" achievement
            if (finalScore >= 10000)
            {
                await UnlockAchievement(userId, "Bug Squasher", "Reached a score of 10,000 in Bug Chase!");
            }

            // Check for "Deadline Dodger" achievement
            if (finalScore >= 50000)
            {
                await UnlockAchievement(userId, "Deadline Dodger", "Reached an epic score of 50,000!");
            }
        }

        private async Task UnlockAchievement(string userId, string achievementName, string description)
        {
            // Check if the user already has this achievement in MongoDB
            var existingAchievement = await _mongoContext.Achievements
                .Find(a => a.UserId == userId && a.Name == achievementName)
                .FirstOrDefaultAsync();

            if (existingAchievement == null)
            {
                // If not, create and save the new achievement
                var newAchievement = new Achievement
                {
                    UserId = userId,
                    Name = achievementName,
                    Description = description
                };
                await _mongoContext.Achievements.InsertOneAsync(newAchievement);

                // Notify the player that they unlocked a new achievement
                await Clients.Caller.SendAsync("AchievementUnlocked", newAchievement);
            }
        }
    }
}