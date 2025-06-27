using DevLifeBackend.Data;
using DevLifeBackend.DTOs;
using DevLifeBackend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace DevLifeBackend.Endpoints;

public static class RoastEndpoints
{
    public static WebApplication MapRoastEndpoints(this WebApplication app)
    {
        var roastGroup = app.MapGroup("/api/roast").WithTags("Code Roast");

        roastGroup.MapGet("/challenge", async ([FromQuery] string language, [FromQuery] string difficulty, HttpContext httpContext, ICodeRoastService roastService) => {
            var userIdString = httpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                Log.Warning("Unauthorized request for roast challenge.");
                return Results.Unauthorized();
            }

            Log.Information("User {UserId} is requesting a roast challenge for language {Language} and difficulty {Difficulty}", userIdString, language, difficulty);

            var challenge = await roastService.GetChallengeAsync(language, difficulty);
            if (challenge == null)
            {
                Log.Warning("No roast challenges found for criteria: {Language}, {Difficulty}", language, difficulty);
                return Results.NotFound($"No roast challenges found for {language} at {difficulty} difficulty.");
            }

            return Results.Ok(challenge);
        })
        .WithName("GetRoastChallenge")
        .Produces<CodeRoastChallengeDto>();

        roastGroup.MapPost("/submit", async (SubmitRoastSolutionDto submission, HttpContext httpContext, ICodeRoastService roastService) => {
            var userIdString = httpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                Log.Warning("Unauthorized roast submission for language {Language}", submission.Language);
                return Results.Unauthorized();
            }

            Log.Information("User {UserId} is submitting a roast solution for language {Language}", userIdString, submission.Language);

            var result = await roastService.GetRoastAnalysisAsync(submission.Language, submission.SourceCode);
            if (result == null)
            {
                Log.Error("Roast analysis failed for user {UserId} and language {Language}", userIdString, submission.Language);
                return Results.Problem("Failed to get roast analysis.");
            }

            Log.Information("Roast analysis for user {UserId} completed with status: {Status}", userIdString, result.ExecutionStatus);

            return Results.Ok(result);
        })
        .WithName("SubmitRoastSolution")
        .Produces<RoastResultDto>();

        return app;
    }
}