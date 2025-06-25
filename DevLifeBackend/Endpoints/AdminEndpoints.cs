using DevLifeBackend.Data;
using DevLifeBackend.DTOs;
using DevLifeBackend.Models;
using DevLifeBackend.Services;

namespace DevLifeBackend.Endpoints;

public static class AdminEndpoints
{
    public static WebApplication MapAdminEndpoints(this WebApplication app)
    {
        var adminGroup = app.MapGroup("/api/admin").WithTags("Admin");
        adminGroup.MapPost("/casino/generate-snippet",
            async (GenerateSnippetRequestDto request, ICodewarsService codewarsService, IAiSnippetGeneratorService aiService, ApplicationDbContext db) =>
            {
                // FIX 1: Use the difficulty from the request
                var task = await codewarsService.GetRandomTaskAsync(request.Language, request.Difficulty);
                if (task == null) { return Results.NotFound($"Could not find a task for language '{request.Language}' and difficulty '{request.Difficulty}'."); }

                var correctCode = await aiService.GenerateCorrectSnippetAsync(task.Description, request.Language);
                if (string.IsNullOrWhiteSpace(correctCode)) { return Results.Problem("AI failed to generate a correct code solution."); }

                var buggyCode = await aiService.GenerateBuggySnippetAsync(correctCode, request.Language);
                if (string.IsNullOrWhiteSpace(buggyCode)) { return Results.Problem("AI failed to generate a buggy version of the code."); }

                var newSnippet = new CodeSnippet
                {
                    Language = request.Language,
                    CorrectCode = correctCode,
                    BuggyCode = buggyCode,
                    Description = $"Challenge: {task.Name}",
                    // FIX 2: Use the difficulty from the request
                    Difficulty = request.Difficulty,
                    Source = task.Source
                };
                db.CodeSnippets.Add(newSnippet);
                await db.SaveChangesAsync();

                return Results.Created($"/api/casino/snippets/{newSnippet.Id}", newSnippet);
            })
        .WithName("GenerateCasinoSnippetFullyAutomated")
        .Produces<CodeSnippet>(StatusCodes.Status201Created);

        return app;
    }
}