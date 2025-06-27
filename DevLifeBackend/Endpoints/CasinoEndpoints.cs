using DevLifeBackend.Data;
using DevLifeBackend.DTOs;
using DevLifeBackend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace DevLifeBackend.Endpoints;

public static class CasinoEndpoints
{
    public static WebApplication MapCasinoEndpoints(this WebApplication app)
    {
        var casinoGroup = app.MapGroup("/api/casino").WithTags("Casino");

        casinoGroup.MapGet("/challenge", async ([FromQuery] string language, [FromQuery] string difficulty, HttpContext httpContext, ICasinoService casinoService, ApplicationDbContext db) => {
            var userIdString = httpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                Log.Warning("Unauthorized request for casino challenge.");
                return Results.Unauthorized();
            }

            var user = await db.Users.FindAsync(int.Parse(userIdString));
            if (user == null) return Results.NotFound("User not found.");

            Log.Information("User {Username} is requesting a casino challenge for language {Language} and difficulty {Difficulty}", user.Username, language, difficulty);

            var challenge = await casinoService.GetRandomChallengeAsync(language, difficulty, httpContext.Session);
            if (challenge == null)
            {
                Log.Warning("No casino challenges found for User {Username} with criteria: {Language}, {Difficulty}", user.Username, language, difficulty);
                return Results.NotFound($"No challenges found for {language} at {difficulty} difficulty.");
            }

            Log.Information("Delivered challenge {SnippetId} to User {Username}", challenge.SnippetId, user.Username);
            return Results.Ok(challenge);
        })
        .WithName("GetCasinoChallenge")
        .Produces<CasinoChallengeDto>();

        casinoGroup.MapPost("/bet", async (CasinoBetDto betDto, HttpContext httpContext, ICasinoService casinoService) => {
            var userIdString = httpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                Log.Warning("Unauthorized bet attempt for snippet {SnippetId}", betDto.SnippetId);
                return Results.Unauthorized();
            }

            var userId = int.Parse(userIdString);
            var correctAnswerKey = $"CasinoAnswer_{betDto.SnippetId}";
            var correctAnswer = httpContext.Session.GetString(correctAnswerKey);

            if (string.IsNullOrEmpty(correctAnswer))
            {
                Log.Warning("User {UserId} attempted to bet on snippet {SnippetId} with an expired/invalid session key", userId, betDto.SnippetId);
                return Results.BadRequest("Challenge session expired or invalid. Please get a new challenge.");
            }

            httpContext.Session.Remove(correctAnswerKey);

            var result = await casinoService.ProcessBetAsync(userId, betDto, correctAnswer);

            if (result.IsCorrect)
            {
                Log.Information("User {UserId} won a bet of {Points} on snippet {SnippetId}. New score: {NewScore}", userId, betDto.Points, betDto.SnippetId, result.NewScore);
            }
            else
            {
                Log.Information("User {UserId} lost a bet of {Points} on snippet {SnippetId}. New score: {NewScore}", userId, betDto.Points, betDto.SnippetId, result.NewScore);
            }

            if (result.Message == "User not found.") return Results.NotFound(result);
            if (result.Message == "Invalid bet amount.") return Results.BadRequest(result);

            return Results.Ok(result);
        })
        .WithName("PlaceCasinoBet")
        .Produces<BetResultDto>();

        casinoGroup.MapGet("/leaderboard", async (ICasinoService casinoService) =>
        {
            Log.Information("Fetching casino leaderboard");
            var leaderboard = await casinoService.GetLeaderboardAsync();
            return Results.Ok(leaderboard);
        })
        .WithName("GetLeaderboard")
        .Produces<IEnumerable<LeaderboardEntryDto>>();

        casinoGroup.MapGet("/daily-challenge", async (HttpContext httpContext, ICasinoService casinoService, ApplicationDbContext db) =>
        {
            var userIdString = httpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString)) return Results.Unauthorized();

            var user = await db.Users.FindAsync(int.Parse(userIdString));
            if (user == null) return Results.NotFound("User not found.");

            Log.Information("User {Username} is requesting the daily challenge", user.Username);
            var challenge = await casinoService.GetDailyChallengeAsync(user.Stacks, httpContext.Session);

            if (challenge == null)
            {
                Log.Warning("No suitable daily challenge found for User {Username}", user.Username);
                return Results.NotFound("No suitable daily challenge found for your registered stacks.");
            }

            Log.Information("Delivered daily challenge {SnippetId} to User {Username}", challenge.SnippetId, user.Username);
            return Results.Ok(challenge);
        })
        .WithName("GetDailyChallenge")
        .Produces<CasinoChallengeDto>();

        return app;
    }
}