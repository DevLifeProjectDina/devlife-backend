// File: Endpoints/BugChaseEndpoints.cs
using DevLifeBackend.Data;
using DevLifeBackend.DTOs;
using DevLifeBackend.Hubs; // <-- Добавляем using для хаба
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR; // <-- Добавляем using для SignalR
using Microsoft.EntityFrameworkCore;

namespace DevLifeBackend.Endpoints;

public static class BugChaseEndpoints
{
    public static WebApplication MapBugChaseEndpoints(this WebApplication app)
    {
        var bugChaseGroup = app.MapGroup("/api/bug-chase").WithTags("Bug Chase Game");

        // Этот эндпоинт у нас уже был
        bugChaseGroup.MapGet("/leaderboard", async (ApplicationDbContext db) =>
        {
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

        // --- ADD THIS NEW ENDPOINT ---
        // Этот эндпоинт будет имитировать завершение игры и отправку счета
        bugChaseGroup.MapPost("/submit-score", async (
            [FromBody] SubmitScoreDto request,
            HttpContext httpContext,
            ApplicationDbContext db,
            IHubContext<BugChaseHub> hubContext) =>
        {
            var userIdString = httpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return Results.Unauthorized();
            }

            var user = await db.Users.FindAsync(int.Parse(userIdString));
            if (user == null)
            {
                return Results.NotFound("User not found.");
            }

            string message;
            // Проверяем, является ли новый счет рекордом
            if (request.Score > user.BugChaseHighScore)
            {
                user.BugChaseHighScore = request.Score;
                await db.SaveChangesAsync();
                message = $"Congratulations! You've set a new high score: {request.Score}";
            }
            else
            {
                message = $"Good game! Your score was {request.Score}. Your high score remains {user.BugChaseHighScore}.";
            }

            // Здесь мы могли бы также вызвать логику достижений, если бы она была в сервисе.
            // Например, отправить уведомление через SignalR.

            return Results.Ok(new { Message = message, NewHighScore = user.BugChaseHighScore });
        })
        .WithName("SubmitBugChaseScore")
        .Produces(StatusCodes.Status200OK);

        return app;
    }
}