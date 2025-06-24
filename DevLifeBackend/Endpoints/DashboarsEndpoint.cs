// File: Endpoints/DashboardEndpoints.cs
using DevLifeBackend.Data;
using DevLifeBackend.DTOs;
using DevLifeBackend.Services;
using Microsoft.EntityFrameworkCore;

namespace DevLifeBackend.Endpoints;

public static class DashboardEndpoints
{
    public static WebApplication MapDashboardEndpoints(this WebApplication app)
    {
        app.MapGet("/dashboard", async (HttpContext httpContext, ApplicationDbContext db, IHoroscopeService horoscopeService, IDailyFeatureService dailyFeatureService) => {
            var userIdString = httpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString)) return Results.Unauthorized();

            var userId = int.Parse(userIdString);
            var user = await db.Users.FindAsync(userId);
            if (user == null) return Results.NotFound("User associated with this session not found.");

            // Get all the necessary data from our services
            var horoscopeText = await horoscopeService.GetHoroscopeAsync(user.ZodiacSign);
            var luckyTech = GetLuckyTechnology();
            var luckySign = dailyFeatureService.GetLuckyZodiacSign();

            // --- LOGIC UPDATE IS HERE ---
            // Create a descriptive message for the daily bonus
            var dailyBonusMessage = $"Today's lucky sign is {luckySign}! ✨ They get a 1.5x bonus in the Code Casino.";

            var welcomeMessage = $"Hello {user.Name}! As a {user.ZodiacSign}, here is your personalized advice for today:";

            // Create the response DTO with the new field and message
            var response = new DashboardDto
            {
                WelcomeMessage = welcomeMessage,
                DailyHoroscope = horoscopeText,
                LuckyTechnology = luckyTech,
                DailyBonusInfo = dailyBonusMessage // <-- Use the new property and message
            };
            return Results.Ok(response);
        })
        .WithName("GetDashboard");

        return app;
    }

    private static string GetLuckyTechnology()
    {
        var technologies = new[] { "React", "Angular", ".NET", "Python", "Go", "Rust", "TypeScript", "Docker", "Kubernetes", "GraphQL" };
        return technologies[Random.Shared.Next(0, technologies.Length)];
    }
}