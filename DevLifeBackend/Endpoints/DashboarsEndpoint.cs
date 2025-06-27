using DevLifeBackend.Data;
using DevLifeBackend.DTOs;
using DevLifeBackend.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace DevLifeBackend.Endpoints;

public static class DashboardEndpoints
{
    public static WebApplication MapDashboardEndpoints(this WebApplication app)
    {
        app.MapGet("/dashboard", async (HttpContext httpContext, ApplicationDbContext db, IHoroscopeService horoscopeService, IDailyFeatureService dailyFeatureService) => {
            var userIdString = httpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                Log.Warning("Unauthorized request to dashboard.");
                return Results.Unauthorized();
            }

            var userId = int.Parse(userIdString);
            var user = await db.Users.FindAsync(userId);
            if (user == null)
            {
                Log.Warning("Dashboard request failed: User with ID {UserId} not found.", userId);
                return Results.NotFound("User associated with this session not found.");
            }

            Log.Information("Fetching dashboard data for user {Username}", user.Username);

            var horoscopeText = await horoscopeService.GetHoroscopeAsync(user.ZodiacSign);
            var luckyTech = GetLuckyTechnology();
            var luckySign = dailyFeatureService.GetLuckyZodiacSign();

            var dailyBonusMessage = $"Today's lucky sign is {luckySign}! ✨ They get a 1.5x bonus in the Code Casino.";
            var welcomeMessage = $"Hello {user.Name}! As a {user.ZodiacSign}, here is your personalized advice for today:";

            var response = new DashboardDto
            {
                WelcomeMessage = welcomeMessage,
                DailyHoroscope = horoscopeText,
                LuckyTechnology = luckyTech,
                DailyBonusInfo = dailyBonusMessage,
                WinStreak = user.WinStreak
            };
            return Results.Ok(response);
        })
        .WithName("GetDashboard")
        .WithTags("Dashboard");

        return app;
    }

    private static string GetLuckyTechnology()
    {
        var technologies = new[] { "React", "Angular", ".NET", "Python", "Go", "Rust", "TypeScript", "Docker", "Kubernetes", "GraphQL" };
        return technologies[Random.Shared.Next(0, technologies.Length)];
    }
}