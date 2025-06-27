using DevLifeBackend.Data;
using DevLifeBackend.DTOs;
using DevLifeBackend.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace DevLifeBackend.Endpoints;

public static class BugChaseEndpoints
{
    public static WebApplication MapBugChaseEndpoints(this WebApplication app)
    {
        var bugChaseGroup = app.MapGroup("/api/bug-chase").WithTags("Bug Chase Game");

        bugChaseGroup.MapGet("/leaderboard", async (ApplicationDbContext db) =>
        {
            Log.Information("Fetching Bug Chase leaderboard");
            var leaderboard = await db.Users
                .OrderByDescending(u => u.BugChaseHighScore)
                .Take(10)
                .Select(u => new BugChaseLeaderboardEntryDto
                {
                    Username = u.Username,
                    ExperienceLevel = u.ExperienceLevel,
                    HighScore = u.BugChaseHighScore
                })
                .ToListAsync();

            return Results.Ok(leaderboard);
        })
        .WithName("GetBugChaseLeaderboard")
        .Produces<IEnumerable<BugChaseLeaderboardEntryDto>>();

        bugChaseGroup.MapPost("/submit-score", async (
            [FromBody] SubmitScoreDto request,
            HttpContext httpContext,
            ApplicationDbContext db) =>
        {
            var userIdString = httpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                Log.Warning("Unauthorized attempt to submit Bug Chase score.");
                return Results.Unauthorized();
            }

            var userId = int.Parse(userIdString);
            var user = await db.Users.FindAsync(userId);
            if (user == null)
            {
                Log.Warning("Bug Chase score submission failed: User with ID {UserId} not found.", userId);
                return Results.NotFound("User not found.");
            }

            string message;
            if (request.Score > user.BugChaseHighScore)
            {
                Log.Information("User {Username} set a new Bug Chase high score: {NewScore} (previous: {OldScore})", user.Username, request.Score, user.BugChaseHighScore);
                user.BugChaseHighScore = request.Score;
                await db.SaveChangesAsync();
                message = $"Congratulations! You've set a new high score: {request.Score}";
            }
            else
            {
                Log.Information("User {Username} submitted a score of {Score}, which is not a new high score.", user.Username, request.Score);
                message = $"Good game! Your score was {request.Score}. Your high score remains {user.BugChaseHighScore}.";
            }

            return Results.Ok(new { Message = message, NewHighScore = user.BugChaseHighScore });
        })
        .WithName("SubmitBugChaseScore")
        .Produces(StatusCodes.Status200OK);

        return app;
    }
}