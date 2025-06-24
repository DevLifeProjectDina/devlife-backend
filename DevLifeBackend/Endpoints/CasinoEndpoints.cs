// File: Endpoints/CasinoEndpoints.cs
using DevLifeBackend.Data;
using DevLifeBackend.DTOs;
using DevLifeBackend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevLifeBackend.Endpoints;

public static class CasinoEndpoints
{
    public static WebApplication MapCasinoEndpoints(this WebApplication app)
    {
        var casinoGroup = app.MapGroup("/api/casino");

        casinoGroup.MapGet("/challenge", async ([FromQuery] string language, [FromQuery] string difficulty, HttpContext httpContext, ICasinoService casinoService, ApplicationDbContext db) => {
            var userIdString = httpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString)) return Results.Unauthorized();

            var user = await db.Users.FindAsync(int.Parse(userIdString));
            if (user == null) return Results.NotFound("User not found.");

            var challenge = await casinoService.GetRandomChallengeAsync(language, difficulty, httpContext.Session);
            if (challenge == null) { return Results.NotFound($"No challenges found for {language} at {difficulty} difficulty."); }
            return Results.Ok(challenge);
        })
        .WithName("GetCasinoChallenge")
        .Produces<CasinoChallengeDto>();

        casinoGroup.MapPost("/bet", async (CasinoBetDto betDto, HttpContext httpContext, ICasinoService casinoService) => {
            var userIdString = httpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString)) return Results.Unauthorized();
            var correctAnswerKey = $"CasinoAnswer_{betDto.SnippetId}";
            var correctAnswer = httpContext.Session.GetString(correctAnswerKey);
            if (string.IsNullOrEmpty(correctAnswer)) { return Results.BadRequest("Challenge session expired or invalid. Please get a new challenge."); }
            httpContext.Session.Remove(correctAnswerKey);
            var result = await casinoService.ProcessBetAsync(int.Parse(userIdString), betDto, correctAnswer);
            if (result.Message == "User not found.") return Results.NotFound(result);
            if (result.Message == "Invalid bet amount.") return Results.BadRequest(result);
            return Results.Ok(result);
        })
        .WithName("PlaceCasinoBet")
        .Produces<BetResultDto>();

        casinoGroup.MapGet("/leaderboard", async (ICasinoService casinoService) =>
        {
            var leaderboard = await casinoService.GetLeaderboardAsync();
            return Results.Ok(leaderboard);
        })
        .WithName("GetLeaderboard")
        .Produces<IEnumerable<LeaderboardEntryDto>>();

        return app;
    }
}