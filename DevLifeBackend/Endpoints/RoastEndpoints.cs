using DevLifeBackend.Data;
using DevLifeBackend.DTOs;
using DevLifeBackend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevLifeBackend.Endpoints;

public static class RoastEndpoints
{
    public static WebApplication MapRoastEndpoints(this WebApplication app)
    {
        var roastGroup = app.MapGroup("/api/roast");

        // FIX: Added [FromQuery] string difficulty and passed it to the service
        roastGroup.MapGet("/challenge", async ([FromQuery] string language, [FromQuery] string difficulty, HttpContext httpContext, ICodeRoastService roastService) => {
            var userIdString = httpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString)) return Results.Unauthorized();

            // No need to check user's profile, per our last change

            var challenge = await roastService.GetChallengeAsync(language, difficulty);
            if (challenge == null) { return Results.NotFound($"No roast challenges found for {language} at {difficulty} difficulty."); }

            return Results.Ok(challenge);
        })
        .WithName("GetRoastChallenge")
        .Produces<CodeRoastChallengeDto>();

        roastGroup.MapPost("/submit", async (SubmitRoastSolutionDto submission, HttpContext httpContext, ICodeRoastService roastService) => {
            var userIdString = httpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString)) return Results.Unauthorized();

            var result = await roastService.GetRoastAnalysisAsync(submission.Language, submission.SourceCode);
            if (result == null) { return Results.Problem("Failed to get roast analysis."); }

            return Results.Ok(result);
        })
        .WithName("SubmitRoastSolution")
        .Produces<RoastResultDto>();

        return app;
    }
}