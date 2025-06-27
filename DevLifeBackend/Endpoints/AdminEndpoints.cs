
using DevLifeBackend.Data;
using DevLifeBackend.DTOs;
using DevLifeBackend.Models;
using DevLifeBackend.Services;
using Serilog;

namespace DevLifeBackend.Endpoints;

public static class AdminEndpoints
{
    public static WebApplication MapAdminEndpoints(this WebApplication app)
    {
        var adminGroup = app.MapGroup("/api/admin").WithTags("Admin");

        adminGroup.MapPost("/casino/generate-snippet",
            async (GenerateSnippetRequestDto request, ICodewarsService codewarsService, IAiSnippetGeneratorService aiService, MongoDbContext mongoDb) =>
            {
                Log.Information("Starting snippet generation for language: {Language}, difficulty: {Difficulty}", request.Language, request.Difficulty);

                var task = await codewarsService.GetRandomTaskAsync(request.Language, request.Difficulty);
                if (task == null)
                {
                    Log.Warning("Could not find a Codewars task for language: {Language}, difficulty: {Difficulty}", request.Language, request.Difficulty);
                    return Results.NotFound($"Could not find a task for language: {request.Language}");
                }
                Log.Information("Successfully fetched task '{TaskName}' from Codewars", task.Name);

                var correctCode = await aiService.GenerateCorrectSnippetAsync(task.Description, request.Language);
                if (string.IsNullOrWhiteSpace(correctCode))
                {
                    Log.Error("AI failed to generate a correct code solution for task: {TaskName}", task.Name);
                    return Results.Problem("AI failed to generate a correct code solution.");
                }
                Log.Information("AI successfully generated a correct solution");

                var buggyCode = await aiService.GenerateBuggySnippetAsync(correctCode, request.Language);
                if (string.IsNullOrWhiteSpace(buggyCode))
                {
                    Log.Error("AI failed to generate a buggy version of the code for task: {TaskName}", task.Name);
                    return Results.Problem("AI failed to generate a buggy version of the code.");
                }
                Log.Information("AI successfully generated a buggy version");

                var newSnippet = new CodeSnippet
                {
                    Language = request.Language,
                    CorrectCode = correctCode,
                    BuggyCode = buggyCode,
                    Description = $"Challenge: {task.Name}",
                    Difficulty = request.Difficulty,
                    Source = task.Source
                };

                await mongoDb.CodeSnippets.InsertOneAsync(newSnippet);
                Log.Information("Successfully inserted new snippet with ID {SnippetId} into MongoDB", newSnippet.Id);

                return Results.Created($"/api/casino/snippets/{newSnippet.Id}", newSnippet);
            })
        .WithName("GenerateCasinoSnippetFullyAutomated")
        .Produces<CodeSnippet>(StatusCodes.Status201Created);

        return app;
    }
}